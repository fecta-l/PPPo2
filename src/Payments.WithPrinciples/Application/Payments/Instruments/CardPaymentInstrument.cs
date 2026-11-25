using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Payments.Instruments;

/// <summary>
/// Оплата кредитной картой. Реализация знает только о своей карте и её лимите.
/// </summary>
public sealed class CardPaymentInstrument : IPaymentInstrument
{
    public CardPaymentInstrument(Client owner, CreditCard card)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(card);

        if (!owner.Owns(card))
        {
            throw new DomainException($"Карта {card.CardNumber} не принадлежит клиенту {owner.Name}.");
        }

        Owner = owner;
        Card = card;
    }

    public CreditCard Card { get; }

    public Client Owner { get; }

    public string Description => $"карта {Card.CardNumber}";

    public string Unit => "руб.";

    public decimal AvailableAmount => Card.AvailableLimit;

    public decimal CommissionRate => 0m;

    public decimal CalculateCommission(decimal amount) => 0m;

    public void Debit(decimal amount) => Card.Charge(amount);

    public void Credit(decimal amount) => Card.Repay(amount);
}
