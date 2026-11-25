namespace Payments.Principles4.Domain;

/// <summary>
/// Клиент банка. Отвечает за состав своих счетов и карт, а также за бонусный баланс.
/// Правила проведения платежей в этот класс не входят.
/// </summary>
public sealed class Client : User
{
    private readonly List<Account> _accounts = new();
    private readonly List<CreditCard> _cards = new();

    public Client(int id, string name, decimal bonusBalance = 0m)
        : base(id, name)
    {
        if (bonusBalance < 0m)
        {
            throw new DomainException("Бонусный баланс не может быть отрицательным.");
        }

        BonusBalance = bonusBalance;
    }

    public decimal BonusBalance { get; private set; }

    public IReadOnlyList<Account> Accounts => _accounts;

    public IReadOnlyList<CreditCard> Cards => _cards;

    public void AddAccount(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (_accounts.Any(a => a.AccountNumber == account.AccountNumber))
        {
            throw new DomainException($"Счёт {account.AccountNumber} уже добавлен клиенту.");
        }

        _accounts.Add(account);
    }

    public void AddCard(CreditCard card)
    {
        ArgumentNullException.ThrowIfNull(card);

        if (_cards.Any(c => c.CardNumber == card.CardNumber))
        {
            throw new DomainException($"Карта {card.CardNumber} уже добавлена клиенту.");
        }

        _cards.Add(card);
    }

    public bool Owns(Account account) => _accounts.Contains(account);

    public bool Owns(CreditCard card) => _cards.Contains(card);

    public void AddBonus(decimal amount)
    {
        if (amount < 0m)
        {
            throw new DomainException("Начисляемая сумма бонусов не может быть отрицательной.");
        }

        BonusBalance += amount;
    }

    public void SpendBonus(decimal amount)
    {
        if (amount <= 0m)
        {
            throw new DomainException("Списываемая сумма бонусов должна быть положительной.");
        }

        if (BonusBalance < amount)
        {
            throw new DomainException("Недостаточно бонусных баллов.");
        }

        BonusBalance -= amount;
    }
}

