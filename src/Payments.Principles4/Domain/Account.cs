namespace Payments.Principles4.Domain;

/// <summary>
/// Банковский счёт. Отвечает за собственный баланс и допустимые операции над ним.
/// </summary>
public sealed class Account
{
    public Account(string accountNumber, decimal initialBalance = 0m)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            throw new DomainException("Номер счёта не может быть пустым.");
        }

        if (initialBalance < 0m)
        {
            throw new DomainException("Начальный баланс счёта не может быть отрицательным.");
        }

        AccountNumber = accountNumber;
        Balance = initialBalance;
        Status = AccountStatus.Active;
    }

    public string AccountNumber { get; }

    public decimal Balance { get; private set; }

    public AccountStatus Status { get; private set; }

    public bool IsActive => Status == AccountStatus.Active;

    public void Deposit(decimal amount)
    {
        EnsureActive();

        if (amount <= 0m)
        {
            throw new DomainException("Сумма пополнения должна быть положительной.");
        }

        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        EnsureActive();

        if (amount <= 0m)
        {
            throw new DomainException("Сумма списания должна быть положительной.");
        }

        if (Balance < amount)
        {
            throw new DomainException($"Недостаточно средств на счёте {AccountNumber}.");
        }

        Balance -= amount;
    }

    public void Cancel()
    {
        EnsureActive();

        if (Balance != 0m)
        {
            throw new DomainException("Нельзя аннулировать счёт с ненулевым балансом.");
        }

        Status = AccountStatus.Cancelled;
    }

    private void EnsureActive()
    {
        if (!IsActive)
        {
            throw new DomainException($"Счёт {AccountNumber} аннулирован.");
        }
    }
}

