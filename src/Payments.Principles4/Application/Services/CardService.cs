using Payments.Principles4.Application.Abstractions;
using Payments.Principles4.Application.Models;
using Payments.Principles4.Domain.Capabilities;

namespace Payments.Principles4.Application.Services;

/// <summary>
/// Блокировка карты. Зависит от IBlockable, а не от конкретного CreditCard
/// и не от «жирного» интерфейса платёжной системы (ISP, DIP).
/// </summary>
public sealed class CardService : ICardService
{
    private readonly IPaymentJournal _journal;
    private readonly IOperationNotifier _notifier;

    public CardService(IPaymentJournal journal, IOperationNotifier notifier)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(notifier);

        _journal = journal;
        _notifier = notifier;
    }

    public OperationResult Block(IBlockable card, string reason)
    {
        ArgumentNullException.ThrowIfNull(card);

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Reject("не указана причина блокировки");
        }

        if (card.IsBlocked)
        {
            return Reject($"карта {card.Number} уже заблокирована");
        }

        card.Block(reason);
        _journal.Record($"БЛОКИРОВКА карты {card.Number}: {reason}.");
        _notifier.CardBlocked(card, reason, "клиентом");
        return OperationResult.Ok($"карта {card.Number} заблокирована");
    }

    private OperationResult Reject(string message)
    {
        _notifier.OperationRejected(message);
        return OperationResult.Fail(message);
    }
}
