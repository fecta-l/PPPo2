namespace Payments.WithoutPrinciples;

/// <summary>
/// Способы оплаты заказа. Значение BonusPoints добавлено позже,
/// когда потребовалось расширить систему (см. комментарий в PaymentSystem.PayForOrder).
/// </summary>
public enum PaymentType
{
    Card = 0,
    BankAccount = 1,
    BonusPoints = 2
}

public class User
{
    public int Id;
    public string Name = string.Empty;
}

public class Client : User
{
    public List<Account> Accounts = new();
    public List<CreditCard> Cards = new();

    // Поле добавлено вместе с новым способом оплаты BonusPoints.
    public decimal BonusBalance;
}

public class Administrator : User
{
}

public class Account
{
    public string AccountNumber = string.Empty;
    public decimal Balance;
    public string Status = "Active";
}

public class CreditCard
{
    public string CardNumber = string.Empty;
    public decimal CreditLimit;
    public decimal CurrentDebt;
    public bool IsBlocked;
}

public class Order
{
    public int OrderId;
    public decimal Amount;
    public string Status = "New";
}
