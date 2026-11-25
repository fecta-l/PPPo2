using Payments.Principles4.Application.Abstractions;
using Payments.Principles4.Application.Models;
using Payments.Principles4.Common;
using Payments.Principles4.Domain;

namespace Payments.Principles4.Application.Services;

/// <summary>
/// Перевод между счетами. Зависит от журнала и узкого интерфейса уведомлений (DIP, ISP).
/// </summary>
public sealed class TransferService : ITransferService
{
    private readonly IPaymentJournal _journal;
    private readonly IOperationNotifier _notifier;

    public TransferService(IPaymentJournal journal, IOperationNotifier notifier)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(notifier);

        _journal = journal;
        _notifier = notifier;
    }

    public TransferResult Transfer(Client client, Account from, Account to, decimal amount)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        if (!client.Owns(from))
        {
            return Reject("счёт списания не принадлежит клиенту");
        }

        if (!from.IsActive)
        {
            return Reject($"счёт {from.AccountNumber} аннулирован");
        }

        if (!to.IsActive)
        {
            return Reject($"счёт {to.AccountNumber} аннулирован");
        }

        if (ReferenceEquals(from, to) || from.AccountNumber == to.AccountNumber)
        {
            return Reject("счета списания и зачисления совпадают");
        }

        if (amount <= 0m)
        {
            return Reject("сумма перевода должна быть положительной");
        }

        if (from.Balance < amount)
        {
            return Reject($"недостаточно средств (доступно {Money.Format(from.Balance)} руб.)");
        }

        try
        {
            from.Withdraw(amount);
            to.Deposit(amount);
        }
        catch (DomainException exception)
        {
            return Reject(exception.Message);
        }

        _journal.Record(
            $"ПЕРЕВОД {Money.Format(amount)} руб. со счёта {from.AccountNumber} на счёт {to.AccountNumber}.");
        _notifier.TransferCompleted(from, to, amount);
        return TransferResult.Success(amount);
    }

    private TransferResult Reject(string message)
    {
        _notifier.OperationRejected(message);
        return TransferResult.Failure(message);
    }
}
