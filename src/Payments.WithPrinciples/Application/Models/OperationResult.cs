namespace Payments.WithPrinciples.Application.Models;

/// <summary>
/// Универсальный результат операции (блокировка карты, аннулирование счёта и т. п.).
/// </summary>
public sealed record OperationResult(bool IsSuccess, string Message)
{
    public static OperationResult Ok(string message) => new(true, message);

    public static OperationResult Fail(string message) => new(false, message);
}
