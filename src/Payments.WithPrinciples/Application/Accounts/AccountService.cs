using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Common;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Accounts;

/// <summary>
/// Аннулирование счёта клиентом. Проверки и уведомления не смешиваются
/// с бизнес-логикой самого счёта.
/// </summary>
public sealed class AccountService : IAccountService
{
    private readonly IPaymentJournal _journal;
    private readonly IUserNotifier _notifier;

    public AccountService(IPaymentJournal journal, IUserNotifier notifier)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(notifier);

        _journal = journal;
        _notifier = notifier;
    }

    public OperationResult Cancel(Client client, Account account)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(account);

        if (!client.Owns(account))
        {
            return Reject("счёт не принадлежит клиенту");
        }

        if (!account.IsActive)
        {
            return Reject($"счёт {account.AccountNumber} уже аннулирован");
        }

        if (account.Balance != 0m)
        {
            return Reject($"нельзя аннулировать счёт с ненулевым балансом ({Money.Format(account.Balance)} руб.)");
        }

        account.Cancel();
        _journal.Record($"АННУЛИРОВАНИЕ счёта {account.AccountNumber} клиентом {client.Name}.");
        _notifier.AccountCancelled(client, account);
        return OperationResult.Ok($"счёт {account.AccountNumber} аннулирован");
    }

    private OperationResult Reject(string message)
    {
        _notifier.OperationRejected(message);
        return OperationResult.Fail(message);
    }
}
