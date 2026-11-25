using Payments.Principles4.Application.Models;
using Payments.Principles4.Domain;
using Payments.Principles4.Domain.Capabilities;

namespace Payments.Principles4.Application.Abstractions;

/// <summary>
/// Уведомления об остальных операциях. Отделены от IPaymentNotifier,
/// чтобы каждый сервис зависел только от нужных ему методов (ISP).
/// </summary>
public interface IOperationNotifier
{
    void TransferCompleted(Account from, Account to, decimal amount);

    void CardBlocked(IBlockable card, string reason, string initiator);

    void AccountCancelled(Account account);

    void CreditLimitsChecked(CreditLimitReport report);

    void OperationRejected(string reason);
}
