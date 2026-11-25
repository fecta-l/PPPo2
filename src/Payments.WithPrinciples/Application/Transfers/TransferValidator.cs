using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Common;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Transfers;

/// <summary>
/// Проверка перевода между счетами. Вынесена из сервиса, чтобы правила
/// можно было изменять и тестировать независимо.
/// </summary>
public sealed class TransferValidator : ITransferValidator
{
    public ValidationResult Validate(Client client, Account from, Account to, decimal amount)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        if (amount <= 0m)
        {
            return ValidationResult.Deny("сумма перевода должна быть положительной");
        }

        if (!client.Owns(from))
        {
            return ValidationResult.Deny("счёт списания не принадлежит клиенту");
        }

        if (!from.IsActive)
        {
            return ValidationResult.Deny($"счёт {from.AccountNumber} аннулирован");
        }

        if (!to.IsActive)
        {
            return ValidationResult.Deny($"счёт {to.AccountNumber} аннулирован");
        }

        if (ReferenceEquals(from, to) || from.AccountNumber == to.AccountNumber)
        {
            return ValidationResult.Deny("счета списания и зачисления совпадают");
        }

        if (from.Balance < amount)
        {
            return ValidationResult.Deny(
                $"недостаточно средств на счёте (доступно {Money.Format(from.Balance)} руб.)");
        }

        return ValidationResult.Allow();
    }
}
