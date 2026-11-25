namespace Payments.Principles4.Domain.Payments;

/// <summary>
/// Результат проверки платёжного инструмента.
/// </summary>
public readonly record struct PaymentCheck(bool IsAllowed, string Message)
{
    public static PaymentCheck Allow() => new(true, string.Empty);

    public static PaymentCheck Deny(string message) => new(false, message);
}
