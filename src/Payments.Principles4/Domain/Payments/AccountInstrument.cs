using Payments.Principles4.Common;
using Payments.Principles4.Domain;

namespace Payments.Principles4.Domain.Payments;

/// <summary>Оплата с банковского счёта (комиссия 1 %).</summary>
public sealed class AccountInstrument : PaymentInstrument
{
    public AccountInstrument(Client owner, Account account)
        : base(owner)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (!owner.Owns(account))
        {
            throw new DomainException($"Счёт {account.AccountNumber} не принадлежит клиенту {owner.Name}.");
        }

        Account = account;
    }

    public Account Account { get; }

    public override string Description => $"счёт {Account.AccountNumber}";

    public override string Unit => "руб.";

    public override decimal AvailableAmount => Account.Balance;

    public override decimal CommissionRate => 0.01m;

    public override PaymentCheck Validate(decimal amount)
    {
        if (!Account.IsActive)
        {
            return PaymentCheck.Deny($"счёт {Account.AccountNumber} аннулирован");
        }

        if (amount > Account.Balance)
        {
            return PaymentCheck.Deny(
                $"недостаточно средств (требуется {Money.Format(amount)} руб., " +
                $"доступно {Money.Format(Account.Balance)} руб.)");
        }

        return PaymentCheck.Allow();
    }

    public override void Debit(decimal amount) => Account.Withdraw(amount);

    public override void Credit(decimal amount) => Account.Deposit(amount);
}
