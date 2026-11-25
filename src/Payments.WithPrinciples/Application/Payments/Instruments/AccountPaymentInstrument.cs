using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Payments.Instruments;

/// <summary>
/// Оплата с банковского счёта. Комиссия — свойство данного способа оплаты,
/// поэтому добавляется вместе с новым классом, а не правкой сервиса.
/// </summary>
public sealed class AccountPaymentInstrument : IPaymentInstrument
{
    public AccountPaymentInstrument(Client owner, Account account)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(account);

        if (!owner.Owns(account))
        {
            throw new DomainException($"Счёт {account.AccountNumber} не принадлежит клиенту {owner.Name}.");
        }

        Owner = owner;
        Account = account;
    }

    public Account Account { get; }

    public Client Owner { get; }

    public string Description => $"счёт {Account.AccountNumber}";

    public string Unit => "руб.";

    public decimal AvailableAmount => Account.Balance;

    public decimal CommissionRate => 0.01m;

    public decimal CalculateCommission(decimal amount) =>
        decimal.Round(amount * CommissionRate, 2, MidpointRounding.AwayFromZero);

    public void Debit(decimal amount) => Account.Withdraw(amount);

    public void Credit(decimal amount) => Account.Deposit(amount);
}
