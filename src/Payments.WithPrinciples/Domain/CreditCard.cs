namespace Payments.WithPrinciples.Domain;

/// <summary>
/// Кредитная карта. Отвечает за кредитный лимит, задолженность и блокировку.
/// </summary>
public sealed class CreditCard
{
    public CreditCard(string cardNumber, decimal creditLimit, decimal currentDebt = 0m)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            throw new DomainException("Номер карты не может быть пустым.");
        }

        if (creditLimit <= 0m)
        {
            throw new DomainException("Кредитный лимит должен быть положительным.");
        }

        if (currentDebt < 0m)
        {
            throw new DomainException("Задолженность по карте не может быть отрицательной.");
        }

        CardNumber = cardNumber;
        CreditLimit = creditLimit;
        CurrentDebt = currentDebt;
    }

    public string CardNumber { get; }

    public decimal CreditLimit { get; }

    public decimal CurrentDebt { get; private set; }

    public bool IsBlocked { get; private set; }

    public string? BlockReason { get; private set; }

    public decimal AvailableLimit => CreditLimit - CurrentDebt;

    public bool IsLimitExceeded => CurrentDebt > CreditLimit;

    public void Charge(decimal amount)
    {
        if (IsBlocked)
        {
            throw new DomainException($"Карта {CardNumber} заблокирована.");
        }

        if (amount <= 0m)
        {
            throw new DomainException("Сумма списания должна быть положительной.");
        }

        if (CurrentDebt + amount > CreditLimit)
        {
            throw new DomainException("Превышен кредитный лимит карты.");
        }

        CurrentDebt += amount;
    }

    public void Repay(decimal amount)
    {
        if (amount <= 0m)
        {
            throw new DomainException("Сумма возврата должна быть положительной.");
        }

        CurrentDebt = Math.Max(0m, CurrentDebt - amount);
    }

    public void Block(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Не указана причина блокировки карты.");
        }

        IsBlocked = true;
        BlockReason = reason;
    }

    public void Unblock()
    {
        IsBlocked = false;
        BlockReason = null;
    }
}
