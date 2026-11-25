using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Application.Payments.Instruments;
using Payments.WithPrinciples.Common;

namespace Payments.WithPrinciples.Application.Payments.Rules;

/// <summary>
/// Правило: средств на счёте или бонусных баллов должно хватать на сумму с комиссией.
/// Для карты действует отдельное правило CreditLimitRule.
/// </summary>
public sealed class SufficientFundsRule : IPaymentRule
{
    public string Name => "Достаточно средств";

    public ValidationResult Check(PaymentContext context)
    {
        if (context.Instrument is CardPaymentInstrument)
        {
            return ValidationResult.Allow();
        }

        if (context.Instrument.AvailableAmount < context.TotalAmount)
        {
            return ValidationResult.Deny(
                $"недостаточно средств (требуется {Money.Format(context.TotalAmount)}, " +
                $"доступно {Money.Format(context.Instrument.AvailableAmount)})");
        }

        return ValidationResult.Allow();
    }
}
