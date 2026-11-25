using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Payments.Rules;

/// <summary>
/// Правило: оплатить можно только заказ в статусе New.
/// </summary>
public sealed class OrderIsPayableRule : IPaymentRule
{
    public string Name => "Заказ доступен для оплаты";

    public ValidationResult Check(PaymentContext context)
    {
        return context.Order.Status == OrderStatus.New
            ? ValidationResult.Allow()
            : ValidationResult.Deny($"заказ №{context.Order.OrderId} имеет статус «{context.Order.Status}»");
    }
}
