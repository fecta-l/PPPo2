using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Administration;

/// <summary>
/// Административные операции: проверка лимитов и блокировка карт за превышение кредита.
/// </summary>
public sealed class CreditControlService : ICreditControlService
{
    private readonly IClientRepository _clients;
    private readonly IPaymentJournal _journal;
    private readonly IUserNotifier _notifier;

    public CreditControlService(
        IClientRepository clients,
        IPaymentJournal journal,
        IUserNotifier notifier)
    {
        ArgumentNullException.ThrowIfNull(clients);
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(notifier);

        _clients = clients;
        _journal = journal;
        _notifier = notifier;
    }

    public CreditLimitReport CheckLimits(Administrator administrator)
    {
        ArgumentNullException.ThrowIfNull(administrator);

        var entries = _clients.GetAll()
            .SelectMany(client => client.Cards.Select(card =>
                new CreditLimitEntry(client, card, card.IsLimitExceeded)))
            .ToList();

        var report = new CreditLimitReport(entries, entries.Count(entry => entry.IsExceeded));
        _journal.Record(
            $"ПРОВЕРКА лимитов администратором {administrator.Name}: карт с превышением — {report.ExceededCount}.");
        _notifier.CreditLimitsChecked(report);
        return report;
    }

    public OperationResult BlockCardForDebt(Administrator administrator, CreditCard card)
    {
        ArgumentNullException.ThrowIfNull(administrator);
        ArgumentNullException.ThrowIfNull(card);

        if (!card.IsLimitExceeded)
        {
            return Reject("превышение кредитного лимита отсутствует — блокировка не требуется");
        }

        if (card.IsBlocked)
        {
            return Reject($"карта {card.CardNumber} уже заблокирована");
        }

        card.Block("превышение кредитного лимита");
        _journal.Record(
            $"БЛОКИРОВКА карты {card.CardNumber} администратором {administrator.Name} " +
            "за превышение кредитного лимита.");
        _notifier.CardBlocked(null, card, "превышение кредитного лимита", $"администратором {administrator.Name}");
        return OperationResult.Ok($"карта {card.CardNumber} заблокирована");
    }

    private OperationResult Reject(string message)
    {
        _notifier.OperationRejected(message);
        return OperationResult.Fail(message);
    }
}
