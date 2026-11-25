using Payments.Principles4.Application.Abstractions;
using Payments.Principles4.Application.Models;
using Payments.Principles4.Common;
using Payments.Principles4.Domain;
using Payments.Principles4.Domain.Capabilities;
using Payments.Principles4.Domain.Payments;

namespace Payments.Principles4.Infrastructure;

/// <summary>
/// Единственное место, где формируется текст для консоли.
/// Реализует два узких интерфейса уведомлений (ISP).
/// </summary>
public sealed class ConsoleNotifier : IPaymentNotifier, IOperationNotifier
{
    public void PaymentCompleted(Order order, PaymentInstrument instrument, decimal amount, decimal commission)
    {
        Console.WriteLine($"  Комиссия: {Money.Format(commission)} {instrument.Unit.TrimEnd('.')}.");
        Console.WriteLine($"  Списано: {Money.Format(amount)} {instrument.Unit.TrimEnd('.')}.");
        Console.WriteLine($"  Доступно после операции: {Money.Format(instrument.AvailableAmount)} {instrument.Unit.TrimEnd('.')}.");
        Console.WriteLine($"  Заказ №{order.OrderId}: статус \"{order.Status}\".");
        Console.WriteLine("  Результат: оплата выполнена.");
    }

    public void PaymentRejected(Order order, PaymentInstrument instrument, string reason)
    {
        Console.WriteLine($"  Отказ: {reason.TrimEnd('.')}.");
        Console.WriteLine($"  Заказ №{order.OrderId} не оплачен.");
    }

    public void TransferCompleted(Account from, Account to, decimal amount)
    {
        Console.WriteLine($"  Списано: {Money.Format(amount)} руб. Остаток: {Money.Format(from.Balance)} руб.");
        Console.WriteLine($"  Зачислено на счёт {to.AccountNumber}. Баланс: {Money.Format(to.Balance)} руб.");
        Console.WriteLine("  Результат: перевод выполнен.");
    }

    public void CardBlocked(IBlockable card, string reason, string initiator)
    {
        Console.WriteLine($"  Карта {card.Number} заблокирована {initiator}. Причина: {reason}.");
    }

    public void AccountCancelled(Account account)
    {
        Console.WriteLine($"  Счёт {account.AccountNumber} аннулирован.");
    }

    public void CreditLimitsChecked(CreditLimitReport report)
    {
        Console.WriteLine("  Проверка кредитных лимитов:");
        foreach (var entry in report.Entries)
        {
            Console.WriteLine(
                $"    {entry.Card.Number}: долг {Money.Format(entry.Card.CurrentDebt)} / " +
                $"лимит {Money.Format(entry.Card.CreditLimit)} — " +
                $"{(entry.IsExceeded ? "ПРЕВЫШЕН" : "в норме")}");
        }

        Console.WriteLine($"  Карт с превышением лимита: {report.ExceededCount}.");
    }

    public void OperationRejected(string reason)
    {
        Console.WriteLine($"  Отказ: {reason.TrimEnd('.')}.");
    }
}
