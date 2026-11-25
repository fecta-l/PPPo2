using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Abstractions;

public interface IOrderPaymentService
{
    PaymentResult PayForOrder(Client client, Order order, IPaymentInstrument instrument);
}
