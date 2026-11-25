using Payments.Principles4.Domain;
using Payments.Principles4.Domain.Payments;

namespace Payments.Principles4.Application.Abstractions;

/// <summary>
/// Уведомления только о платежах. Сервис оплаты не зависит от методов
/// для переводов, блокировок и отчётов (ISP).
/// </summary>
public interface IPaymentNotifier
{
    void PaymentCompleted(Order order, PaymentInstrument instrument, decimal amount, decimal commission);

    void PaymentRejected(Order order, PaymentInstrument instrument, string reason);
}
