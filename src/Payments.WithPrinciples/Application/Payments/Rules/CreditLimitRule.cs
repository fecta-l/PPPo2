using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Application.Payments.Instruments;
using Payments.WithPrinciples.Common;

namespace Payments.WithPrinciples.Application.Payments.Rules;

/// <summary>
/// Правило: сумма платежа с учётом комиссии не должна превышать доступный лимит карты.
/// </summary>
public sealed class CreditLimitRule : IPaymentRule
{
    public string Name => "Кредитный лимит не превышен";

    public ValidationResult Check(PaymentContext context)
    {
        if (context.Instrument is not CardPaymentInstrument instrument)
        {
            return ValidationResult.Allow();
        }

        var card = instrument.Card;

        if (card.IsLimitExceeded)
        {
            return ValidationResult.Deny($"превышен кредитный лимит карты {card.CardNumber}");
        }

        if (context.TotalAmount > card.AvailableLimit)
        {
            return ValidationResult.Deny(
                $"недостаточно доступного лимита (требуется {Money.Format(context.TotalAmount)} руб., " +
                $"доступно {Money.Format(card.AvailableLimit)} руб.)");
        }

        return ValidationResult.Allow();
    }
}
