using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Common;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Infrastructure;

/// <summary>
/// Консольное представление. Единственное место, где формируется текст для пользователя.
/// </summary>
public sealed class ConsoleNotifier : IUserNotifier
{
    public void PaymentCompleted(
        Client client,
        Order order,
        IPaymentInstrument instrument,
        decimal amount,
        decimal commission)
    {
        Console.WriteLine($"  Комиссия: {Money.Format(commission)} {instrument.Unit.TrimEnd('.')}.");
        Console.WriteLine($"  Списано: {Money.Format(amount)} {instrument.Unit.TrimEnd('.')}.");
        Console.WriteLine($"  Доступно после операции: {Money.Format(instrument.AvailableAmount)} {instrument.Unit.TrimEnd('.')}.");
        Console.WriteLine($"  Заказ №{order.OrderId}: статус \"{order.Status}\".");
        Console.WriteLine("  Результат: оплата выполнена.");
    }

    public void PaymentRejected(Client client, Order order, IPaymentInstrument instrument, string reason)
    {
        Console.WriteLine($"  Отказ: {reason.TrimEnd('.')}.");
        Console.WriteLine($"  Заказ №{order.OrderId} не оплачен.");
    }

    public void TransferCompleted(Client client, Account from, Account to, decimal amount)
    {
        Console.WriteLine($"  Списано: {Money.Format(amount)} руб. Остаток на счёте {from.AccountNumber}: {Money.Format(from.Balance)} руб.");
        Console.WriteLine($"  Зачислено на счёт {to.AccountNumber}. Баланс: {Money.Format(to.Balance)} руб.");
        Console.WriteLine("  Результат: перевод выполнен.");
    }

    public void CardBlocked(Client? client, CreditCard card, string reason, string initiator)
    {
        Console.WriteLine($"  Карта {card.CardNumber} заблокирована {initiator}. Причина: {reason}.");
    }

    public void AccountCancelled(Client client, Account account)
    {
        Console.WriteLine($"  Счёт {account.AccountNumber} аннулирован клиентом {client.Name}.");
    }

    public void CreditLimitsChecked(CreditLimitReport report)
    {
        Console.WriteLine("  Проверка кредитных лимитов:");
        foreach (var entry in report.Entries)
        {
            Console.WriteLine(
                $"    {entry.Card.CardNumber} ({entry.Client.Name}): " +
                $"долг {Money.Format(entry.Card.CurrentDebt)} / лимит {Money.Format(entry.Card.CreditLimit)} — " +
                $"{(entry.IsExceeded ? "ПРЕВЫШЕН" : "в норме")}");
        }

        Console.WriteLine($"  Карт с превышением лимита: {report.ExceededCount}.");
    }

    public void OperationRejected(string reason)
    {
        Console.WriteLine($"  Отказ: {reason.TrimEnd('.')}.");
    }
}
