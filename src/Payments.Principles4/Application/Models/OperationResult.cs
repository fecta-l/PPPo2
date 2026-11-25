namespace Payments.Principles4.Application.Models;

public sealed record OperationResult(bool IsSuccess, string Message)
{
    public static OperationResult Ok(string message) => new(true, message);

    public static OperationResult Fail(string message) => new(false, message);
}
