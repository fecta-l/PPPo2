namespace Payments.WithPrinciples.Application.Models;

/// <summary>
/// Результат перевода средств между счетами.
/// </summary>
public sealed record TransferResult(bool IsSuccess, string Message, decimal Amount)
{
    public static TransferResult Success(decimal amount) => new(true, "Перевод выполнен.", amount);

    public static TransferResult Failure(string message) => new(false, message, 0m);
}
