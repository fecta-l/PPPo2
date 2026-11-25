namespace Payments.WithPrinciples.Application.Models;

/// <summary>
/// Результат проверки правила. Не содержит ссылок на консоль и другие детали представления.
/// </summary>
public sealed record ValidationResult(bool IsAllowed, string Message)
{
    public static ValidationResult Allow() => new(true, string.Empty);

    public static ValidationResult Deny(string message) => new(false, message);
}
