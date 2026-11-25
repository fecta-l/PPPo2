using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Payments.Instruments;

/// <summary>
/// Оплата бонусными баллами — новый способ оплаты, добавленный без изменения
/// OrderPaymentService и существующих инструментов.
/// </summary>
public sealed class BonusPaymentInstrument : IPaymentInstrument
{
    public BonusPaymentInstrument(Client owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        Owner = owner;
    }

    public Client Owner { get; }

    public string Description => "бонусные баллы";

    public string Unit => "баллов";

    public decimal AvailableAmount => Owner.BonusBalance;

    public decimal CommissionRate => 0m;

    public decimal CalculateCommission(decimal amount) => 0m;

    public void Debit(decimal amount) => Owner.SpendBonus(amount);

    public void Credit(decimal amount) => Owner.AddBonus(amount);
}
