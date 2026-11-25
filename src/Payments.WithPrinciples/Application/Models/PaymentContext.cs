using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Models;

/// <summary>
/// Данные, необходимые правилу проверки платежа.
/// </summary>
public sealed record PaymentContext(
    Client Client,
    Order Order,
    IPaymentInstrument Instrument,
    decimal Commission)
{
    public decimal TotalAmount => Order.Amount + Commission;
}
