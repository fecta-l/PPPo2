using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Application.Payments.Instruments;

namespace Payments.WithPrinciples.Application.Payments.Rules;

/// <summary>
/// Правило: заблокированной картой платить нельзя.
/// </summary>
public sealed class CardIsNotBlockedRule : IPaymentRule
{
    public string Name => "Карта не заблокирована";

    public ValidationResult Check(PaymentContext context)
    {
        if (context.Instrument is CardPaymentInstrument { Card.IsBlocked: true } instrument)
        {
            return ValidationResult.Deny(
                $"карта {instrument.Card.CardNumber} заблокирована ({instrument.Card.BlockReason})");
        }

        return ValidationResult.Allow();
    }
}
