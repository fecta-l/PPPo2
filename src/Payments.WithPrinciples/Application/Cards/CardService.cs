using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Cards;

/// <summary>
/// Операции клиента с собственной картой. Отдельный сервис,
/// чтобы клиент не занимался журналированием и уведомлениями.
/// </summary>
public sealed class CardService : ICardService
{
    private readonly IPaymentJournal _journal;
    private readonly IUserNotifier _notifier;

    public CardService(IPaymentJournal journal, IUserNotifier notifier)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(notifier);

        _journal = journal;
        _notifier = notifier;
    }

    public OperationResult Block(Client client, CreditCard card, string reason)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(card);

        if (!client.Owns(card))
        {
            return Reject("карта не принадлежит клиенту");
        }

        if (card.IsBlocked)
        {
            return Reject($"карта {card.CardNumber} уже заблокирована");
        }

        card.Block(reason);
        _journal.Record($"БЛОКИРОВКА карты {card.CardNumber} клиентом {client.Name}: {reason}.");
        _notifier.CardBlocked(client, card, reason, $"клиентом {client.Name}");
        return OperationResult.Ok($"карта {card.CardNumber} заблокирована");
    }

    private OperationResult Reject(string message)
    {
        _notifier.OperationRejected(message);
        return OperationResult.Fail(message);
    }
}
