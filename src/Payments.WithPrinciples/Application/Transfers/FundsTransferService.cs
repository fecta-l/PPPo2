using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Common;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Transfers;

/// <summary>
/// Единственная обязанность — выполнить перевод между счетами,
/// предварительно получив результат проверки от ITransferValidator.
/// </summary>
public sealed class FundsTransferService : IFundsTransferService
{
    private readonly ITransferValidator _validator;
    private readonly IPaymentJournal _journal;
    private readonly IUserNotifier _notifier;

    public FundsTransferService(
        ITransferValidator validator,
        IPaymentJournal journal,
        IUserNotifier notifier)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(notifier);

        _validator = validator;
        _journal = journal;
        _notifier = notifier;
    }

    public TransferResult Transfer(Client client, Account from, Account to, decimal amount)
    {
        var validation = _validator.Validate(client, from, to, amount);
        if (!validation.IsAllowed)
        {
            _journal.Record(
                $"ОТКАЗ: перевод {from.AccountNumber} -> {to.AccountNumber}: {validation.Message}.");
            _notifier.OperationRejected(validation.Message);
            return TransferResult.Failure(validation.Message);
        }

        try
        {
            from.Withdraw(amount);
            to.Deposit(amount);
        }
        catch (DomainException exception)
        {
            _journal.Record(
                $"ОТКАЗ: перевод {from.AccountNumber} -> {to.AccountNumber}: {exception.Message}.");
            _notifier.OperationRejected(exception.Message);
            return TransferResult.Failure(exception.Message);
        }

        _journal.Record(
            $"ПЕРЕВОД {Money.Format(amount)} руб. со счёта {from.AccountNumber} на счёт {to.AccountNumber}.");
        _notifier.TransferCompleted(client, from, to, amount);
        return TransferResult.Success(amount);
    }
}
