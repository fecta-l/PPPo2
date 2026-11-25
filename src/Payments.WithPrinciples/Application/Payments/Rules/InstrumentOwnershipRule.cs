using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Application.Models;

namespace Payments.WithPrinciples.Application.Payments.Rules;

/// <summary>
/// Правило: платёжный инструмент должен принадлежать плательщику.
/// </summary>
public sealed class InstrumentOwnershipRule : IPaymentRule
{
    public string Name => "Инструмент принадлежит клиенту";

    public ValidationResult Check(PaymentContext context)
    {
        if (!ReferenceEquals(context.Instrument.Owner, context.Client))
        {
            return ValidationResult.Deny("платёжный инструмент принадлежит другому клиенту");
        }

        return ValidationResult.Allow();
    }
}
