using Payments.Principles4.Common;
using Payments.Principles4.Domain;

namespace Payments.Principles4.Domain.Payments;

/// <summary>Оплата бонусными баллами.</summary>
public sealed class BonusInstrument : PaymentInstrument
{
    public BonusInstrument(Client owner)
        : base(owner)
    {
    }

    public override string Description => "бонусные баллы";

    public override string Unit => "баллов";

    public override decimal AvailableAmount => Owner.BonusBalance;

    public override decimal CommissionRate => 0m;

    public override PaymentCheck Validate(decimal amount)
    {
        if (amount > Owner.BonusBalance)
        {
            return PaymentCheck.Deny(
                $"недостаточно бонусных баллов (требуется {Money.Format(amount)}, " +
                $"доступно {Money.Format(Owner.BonusBalance)})");
        }

        return PaymentCheck.Allow();
    }

    public override void Debit(decimal amount) => Owner.SpendBonus(amount);

    public override void Credit(decimal amount) => Owner.AddBonus(amount);
}
