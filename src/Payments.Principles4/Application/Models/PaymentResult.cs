namespace Payments.Principles4.Application.Models;

public sealed record PaymentResult(bool IsSuccess, string Message, decimal ChargedAmount, decimal Commission)
{
    public static PaymentResult Success(decimal chargedAmount, decimal commission) =>
        new(true, "Оплата выполнена.", chargedAmount, commission);

    public static PaymentResult Failure(string message) =>
        new(false, message, 0m, 0m);
}
