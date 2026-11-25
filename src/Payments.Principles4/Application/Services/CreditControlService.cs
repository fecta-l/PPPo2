using Payments.Principles4.Application.Abstractions;
using Payments.Principles4.Application.Models;
using Payments.Principles4.Domain;
using Payments.Principles4.Domain.Capabilities;

namespace Payments.Principles4.Application.Services;

/// <summary>
/// Административный контроль лимитов. Работает с ролями ICreditCard
/// (блокировка + лимит), не зная о способах оплаты (ISP, DIP).
/// </summary>
public sealed class CreditControlService : ICreditControlService
{
    private readonly IClientRepository _clients;
    private readonly IPaymentJournal _journal;
    private readonly IOperationNotifier _notifier;

    public CreditControlService(
        IClientRepository clients,
        IPaymentJournal journal,
        IOperationNotifier notifier)
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
            $"ПРОВЕРКА лимитов администратором {administrator.Name}: превышений — {report.ExceededCount}.");
        _notifier.CreditLimitsChecked(report);
        return report;
    }

    public OperationResult BlockCardForDebt(Administrator administrator, ICreditCard card)
    {
        ArgumentNullException.ThrowIfNull(administrator);
        ArgumentNullException.ThrowIfNull(card);

        if (!card.IsLimitExceeded)
        {
            return Reject("превышение кредитного лимита отсутствует");
        }

        if (card.IsBlocked)
        {
            return Reject($"карта {card.Number} уже заблокирована");
        }

        card.Block("превышение кредитного лимита");
        _journal.Record(
            $"БЛОКИРОВКА карты {card.Number} администратором {administrator.Name} " +
            "за превышение кредитного лимита.");
        _notifier.CardBlocked(card, "превышение кредитного лимита", $"администратором {administrator.Name}");
        return OperationResult.Ok($"карта {card.Number} заблокирована");
    }

    private OperationResult Reject(string message)
    {
        _notifier.OperationRejected(message);
        return OperationResult.Fail(message);
    }
}
