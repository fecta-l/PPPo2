using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Abstractions;

/// <summary>
/// Платёжный инструмент: карта, счёт или бонусные баллы.
/// Точка расширения OCP: новый способ оплаты добавляется новой реализацией,
/// класс OrderPaymentService при этом не изменяется.
/// </summary>
public interface IPaymentInstrument
{
    string Description { get; }

    /// <summary>Единица измерения для отображения (руб. или баллов).</summary>
    string Unit { get; }

    Client Owner { get; }

    /// <summary>Доступная сумма на момент проверки.</summary>
    decimal AvailableAmount { get; }

    decimal CommissionRate { get; }

    decimal CalculateCommission(decimal amount);

    void Debit(decimal amount);

    void Credit(decimal amount);
}
