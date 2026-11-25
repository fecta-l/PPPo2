using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Abstractions;

/// <summary>
/// Представление результатов операций. Сервисы не работают с Console напрямую.
/// </summary>
public interface IUserNotifier
{
    void PaymentCompleted(Client client, Order order, IPaymentInstrument instrument, decimal amount, decimal commission);

    void PaymentRejected(Client client, Order order, IPaymentInstrument instrument, string reason);

    void TransferCompleted(Client client, Account from, Account to, decimal amount);

    void CardBlocked(Client? client, CreditCard card, string reason, string initiator);

    void AccountCancelled(Client client, Account account);

    void CreditLimitsChecked(CreditLimitReport report);

    void OperationRejected(string reason);
}
