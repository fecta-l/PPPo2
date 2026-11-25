using Payments.Principles4.Domain;

namespace Payments.Principles4.Domain.Payments;

/// <summary>
/// Базовый платёжный инструмент. Все наследники обязаны соблюдать контракт:
/// Validate не бросает исключение при ожидаемом отказе, а Debit вызывается
/// только после успешного Validate и уменьшает AvailableAmount на списанную сумму.
/// Именно этот контракт позволяет подставлять любой инструмент вместо базового (LSP).
/// </summary>
public abstract class PaymentInstrument
{
    protected PaymentInstrument(Client owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        Owner = owner;
    }

    public Client Owner { get; }

    public abstract string Description { get; }

    public abstract string Unit { get; }

    public abstract decimal AvailableAmount { get; }

    public abstract decimal CommissionRate { get; }

    public decimal CalculateCommission(decimal amount) =>
        decimal.Round(amount * CommissionRate, 2, MidpointRounding.AwayFromZero);

    /// <summary>Проверка возможности списания. Ожидаемый отказ — это Deny, а не исключение.</summary>
    public abstract PaymentCheck Validate(decimal amount);

    /// <summary>Списание суммы, прошедшей Validate.</summary>
    public abstract void Debit(decimal amount);

    /// <summary>Возврат суммы при откате операции.</summary>
    public abstract void Credit(decimal amount);
}
