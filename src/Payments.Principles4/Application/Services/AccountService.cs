using Payments.Principles4.Application.Abstractions;
using Payments.Principles4.Application.Models;
using Payments.Principles4.Domain;

namespace Payments.Principles4.Application.Services;

/// <summary>
/// Аннулирование счёта. Правило проверяется доменом, сервис лишь
/// переводит исключение в результат и уведомляет пользователя.
/// </summary>
public sealed class AccountService : IAccountService
{
    private readonly IPaymentJournal _journal;
    private readonly IOperationNotifier _notifier;

    public AccountService(IPaymentJournal journal, IOperationNotifier notifier)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(notifier);

        _journal = journal;
        _notifier = notifier;
    }

    public OperationResult Cancel(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        try
        {
            account.Cancel();
        }
        catch (DomainException exception)
        {
            _notifier.OperationRejected(exception.Message);
            return OperationResult.Fail(exception.Message);
        }

        _journal.Record($"АННУЛИРОВАНИЕ счёта {account.AccountNumber}.");
        _notifier.AccountCancelled(account);
        return OperationResult.Ok($"счёт {account.AccountNumber} аннулирован");
    }
}
