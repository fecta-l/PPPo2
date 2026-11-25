using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Application.Payments.Instruments;

namespace Payments.WithPrinciples.Application.Payments.Rules;

/// <summary>
/// Правило: оплата возможна только с активного счёта.
/// </summary>
public sealed class AccountIsActiveRule : IPaymentRule
{
    public string Name => "Счёт активен";

    public ValidationResult Check(PaymentContext context)
    {
        if (context.Instrument is AccountPaymentInstrument instrument && !instrument.Account.IsActive)
        {
            return ValidationResult.Deny($"счёт {instrument.Account.AccountNumber} аннулирован");
        }

        return ValidationResult.Allow();
    }
}
