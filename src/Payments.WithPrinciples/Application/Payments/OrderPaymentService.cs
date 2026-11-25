using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Common;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Payments;

/// <summary>
/// Единственная обязанность сервиса — провести оплату заказа:
/// проверить правила, списать сумму с инструмента, перевести заказ в статус Paid.
/// </summary>
public sealed class OrderPaymentService : IOrderPaymentService
{
    private readonly IReadOnlyList<IPaymentRule> _rules;
    private readonly IPaymentJournal _journal;
    private readonly IUserNotifier _notifier;

    public OrderPaymentService(
        IEnumerable<IPaymentRule> rules,
        IPaymentJournal journal,
        IUserNotifier notifier)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(notifier);

        _rules = rules.ToList();
        _journal = journal;
        _notifier = notifier;
    }

    public PaymentResult PayForOrder(Client client, Order order, IPaymentInstrument instrument)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(instrument);

        var commission = instrument.CalculateCommission(order.Amount);
        var context = new PaymentContext(client, order, instrument, commission);

        foreach (var rule in _rules)
        {
            var validation = rule.Check(context);
            if (validation.IsAllowed)
            {
                continue;
            }

            _journal.Record(
                $"ОТКАЗ: заказ №{order.OrderId}, {instrument.Description}: " +
                $"{validation.Message} (правило «{rule.Name}»).");
            _notifier.PaymentRejected(client, order, instrument, validation.Message);
            return PaymentResult.Failure(validation.Message);
        }

        instrument.Debit(context.TotalAmount);
        try
        {
            order.MarkPaid();
        }
        catch
        {
            instrument.Credit(context.TotalAmount);
            throw;
        }

        _journal.Record(
            $"ОПЛАТА заказа №{order.OrderId} на {Money.Format(order.Amount)} руб.; " +
            $"{instrument.Description}; комиссия {Money.Format(commission)} руб.");
        _notifier.PaymentCompleted(client, order, instrument, context.TotalAmount, commission);
        return PaymentResult.Success(context.TotalAmount, commission);
    }
}
