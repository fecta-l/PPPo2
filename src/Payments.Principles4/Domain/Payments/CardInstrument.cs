using Payments.Principles4.Common;
using Payments.Principles4.Domain;

namespace Payments.Principles4.Domain.Payments;

/// <summary>Оплата кредитной картой.</summary>
public sealed class CardInstrument : PaymentInstrument
{
    public CardInstrument(Client owner, CreditCard card)
        : base(owner)
    {
        ArgumentNullException.ThrowIfNull(card);

        if (!owner.Owns(card))
        {
            throw new DomainException($"Карта {card.CardNumber} не принадлежит клиенту {owner.Name}.");
        }

        Card = card;
    }

    public CreditCard Card { get; }

    public override string Description => $"карта {Card.CardNumber}";

    public override string Unit => "руб.";

    public override decimal AvailableAmount => Card.AvailableLimit;

    public override decimal CommissionRate => 0m;

    public override PaymentCheck Validate(decimal amount)
    {
        if (Card.IsBlocked)
        {
            return PaymentCheck.Deny($"карта {Card.CardNumber} заблокирована ({Card.BlockReason})");
        }

        if (Card.IsLimitExceeded)
        {
            return PaymentCheck.Deny($"превышен кредитный лимит карты {Card.CardNumber}");
        }

        if (amount > Card.AvailableLimit)
        {
            return PaymentCheck.Deny(
                $"недостаточно доступного лимита (требуется {Money.Format(amount)} руб., " +
                $"доступно {Money.Format(Card.AvailableLimit)} руб.)");
        }

        return PaymentCheck.Allow();
    }

    public override void Debit(decimal amount) => Card.Charge(amount);

    public override void Credit(decimal amount) => Card.Repay(amount);
}
