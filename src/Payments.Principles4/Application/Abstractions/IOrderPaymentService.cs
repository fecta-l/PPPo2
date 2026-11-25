using Payments.Principles4.Application.Models;
using Payments.Principles4.Domain;
using Payments.Principles4.Domain.Payments;

namespace Payments.Principles4.Application.Abstractions;

/// <summary>
/// Узкий интерфейс оплаты заказа. Клиенту, которому нужно только оплатить заказ,
/// не приходится зависеть от переводов, блокировок и администрирования (ISP).
/// </summary>
public interface IOrderPaymentService
{
    PaymentResult PayForOrder(Client client, Order order, PaymentInstrument instrument);
}
