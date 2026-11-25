using Payments.Principles4.Application.Abstractions;
using Payments.Principles4.Application.Models;
using Payments.Principles4.Common;
using Payments.Principles4.Domain;
using Payments.Principles4.Domain.Payments;

namespace Payments.Principles4.Application.Services;

/// <summary>
/// Высокоуровневый сервис оплаты. Зависит только от абстракций:
/// PaymentInstrument, IPaymentJournal, IPaymentNotifier (DIP).
/// Не содержит switch по типу оплаты — работает с базовым классом (LSP).
/// </summary>
public sealed class OrderPaymentService : IOrderPaymentService
{
    private readonly IPaymentJournal _journal;
    private readonly IPaymentNotifier _notifier;

    public OrderPaymentService(IPaymentJournal journal, IPaymentNotifier notifier)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(notifier);

        _journal = journal;
        _notifier = notifier;
    }

    public PaymentResult PayForOrder(Client client, Order order, PaymentInstrument instrument)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(instrument);

        if (!ReferenceEquals(instrument.Owner, client))
        {
            return Reject(order, instrument, "платёжный инструмент принадлежит другому клиенту");
        }

        if (order.Status != OrderStatus.New)
        {
            return Reject(order, instrument, $"заказ №{order.OrderId} имеет статус {order.Status}");
        }

        var commission = instrument.CalculateCommission(order.Amount);
        var total = order.Amount + commission;

        var check = instrument.Validate(total);
        if (!check.IsAllowed)
        {
            return Reject(order, instrument, check.Message);
        }

        instrument.Debit(total);
        try
        {
            order.MarkPaid();
        }
        catch
        {
            instrument.Credit(total);
            throw;
        }

        _journal.Record(
            $"ОПЛАТА заказа №{order.OrderId} на {Money.Format(order.Amount)} руб.; " +
            $"{instrument.Description}; комиссия {Money.Format(commission)} руб.");
        _notifier.PaymentCompleted(order, instrument, total, commission);
        return PaymentResult.Success(total, commission);
    }

    private PaymentResult Reject(Order order, PaymentInstrument instrument, string reason)
    {
        _journal.Record(
            $"ОТКАЗ: заказ №{order.OrderId}, {instrument.Description}: {reason}.");
        _notifier.PaymentRejected(order, instrument, reason);
        return PaymentResult.Failure(reason);
    }
}
