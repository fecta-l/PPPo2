using System.Text;
using Payments.Principles4.Application.Abstractions;
using Payments.Principles4.Application.Models;
using Payments.Principles4.Application.Services;
using Payments.Principles4.Common;
using Payments.Principles4.Composition;
using Payments.Principles4.Domain;
using Payments.Principles4.Domain.Capabilities;
using Payments.Principles4.Domain.Payments;
using Payments.Principles4.Infrastructure;

Console.OutputEncoding = Encoding.UTF8;

var tests = new List<(string Name, Action Run)>
{
    ("LSP: любой платёжный инструмент оплачивает заказ через базовый тип", () =>
    {
        var factories = new Func<Client, PaymentInstrument>[]
        {
            client => new CardInstrument(client, client.Cards[0]),
            client => new AccountInstrument(client, client.Accounts[0]),
            client => new BonusInstrument(client)
        };

        foreach (var factory in factories)
        {
            var (app, client, _) = NewWorld();
            var order = app.Orders.FindById(1)!;
            var instrument = factory(client);
            var before = instrument.AvailableAmount;
            var total = order.Amount + instrument.CalculateCommission(order.Amount);

            var result = app.Payments.PayForOrder(client, order, instrument);

            Assert(result.IsSuccess, $"{instrument.Description}: платёж должен пройти");
            Assert(order.IsPaid, $"{instrument.Description}: заказ должен быть оплачен");
            AssertEqual(before - total, instrument.AvailableAmount, $"{instrument.Description}: остаток инструмента");
        }
    }),

    ("LSP: заблокированная карта возвращает отказ, а не исключение", () =>
    {
        var (_, client, _) = NewWorld();
        var card = client.Cards[0];
        card.Block("тест");

        var check = new CardInstrument(client, card).Validate(1000m);

        Assert(!check.IsAllowed, "проверка должна вернуть отказ");
        Assert(check.Message.Contains("заблокирована"), "причина должна содержать «заблокирована»");
    }),

    ("LSP: после успешного Validate любой инструмент списывает без исключения", () =>
    {
        var (_, client, _) = NewWorld();
        var instruments = new PaymentInstrument[]
        {
            new CardInstrument(client, client.Cards[0]),
            new AccountInstrument(client, client.Accounts[0]),
            new BonusInstrument(client)
        };

        foreach (var instrument in instruments)
        {
            var check = instrument.Validate(500m);
            Assert(check.IsAllowed, $"{instrument.Description}: проверка должна пройти");
            instrument.Debit(500m);
            instrument.Credit(500m);
        }
    }),

    ("ISP: бонусный инструмент не зависит от Block/Unblock и кредитного лимита", () =>
    {
        var (_, client, _) = NewWorld();
        object bonus = new BonusInstrument(client);

        Assert(!(bonus is IBlockable), "бонусный инструмент не должен реализовывать IBlockable");
        Assert(!(bonus is ILimitInfo), "бонусный инструмент не должен реализовывать ILimitInfo");
    }),

    ("ISP: CardService зависит от узкого IBlockable", () =>
    {
        var (app, client, _) = NewWorld();
        IBlockable card = client.Cards[0];

        var result = app.Cards.Block(card, "по требованию клиента");

        Assert(result.IsSuccess, "блокировка должна пройти");
        Assert(card.IsBlocked, "карта должна быть заблокирована");
    }),

    ("DIP: сервис оплаты собирается из абстракций без консоли", () =>
    {
        var (_, client, _) = NewWorld();
        var journal = new InMemoryPaymentJournal();
        var notifier = new RecordingNotifier();
        var service = new OrderPaymentService(journal, notifier);

        var result = service.PayForOrder(
            client,
            new Order(10, 1000m),
            new CardInstrument(client, client.Cards[0]));

        Assert(result.IsSuccess, "платёж должен пройти");
        AssertEqual(1, journal.Entries.Count, "записей в журнале");
        AssertEqual(1, notifier.PaymentCompletedCount, "уведомлений об оплате");
    }),

    ("DIP: подмена реализации уведомлений не меняет сервис", () =>
    {
        var notifier = new RecordingNotifier();
        var (app, client, _) = NewWorld(notifier, notifier);

        app.Payments.PayForOrder(client, app.Orders.FindById(1)!, new BonusInstrument(client));

        Assert(notifier.Events.Any(item => item.StartsWith("payment:1")), "заглушка должна получить событие оплаты");
    }),

    ("Администратор блокирует карту за превышение кредита", () =>
    {
        var (app, client, administrator) = NewWorld();
        var card = client.Cards[1];

        var report = app.CreditControl.CheckLimits(administrator);
        var block = app.CreditControl.BlockCardForDebt(administrator, card);

        AssertEqual(1, report.ExceededCount, "карт с превышением");
        Assert(block.IsSuccess, "блокировка должна пройти");
        Assert(card.IsBlocked, "карта должна быть заблокирована");
    }),

    ("Аннулирование счёта с ненулевым балансом запрещено", () =>
    {
        var (app, client, _) = NewWorld();

        var rejected = app.Accounts.Cancel(client.Accounts[0]);
        var cancelled = app.Accounts.Cancel(client.Accounts[1]);

        Assert(!rejected.IsSuccess, "счёт с балансом нельзя аннулировать");
        Assert(cancelled.IsSuccess, "пустой счёт должен аннулироваться");
        AssertEqual(AccountStatus.Cancelled, client.Accounts[1].Status, "статус счёта");
    })
};

Console.WriteLine(new string('=', 78));
Console.WriteLine("  ТЕСТЫ Payments.Principles4 (LSP, ISP, DIP)");
Console.WriteLine(new string('=', 78));

var passed = 0;
foreach (var (name, run) in tests)
{
    try
    {
        run();
        passed++;
        Console.WriteLine($"  [OK]   {name}");
    }
    catch (Exception exception)
    {
        Console.WriteLine($"  [FAIL] {name}: {exception.Message}");
    }
}

Console.WriteLine(new string('-', 78));
Console.WriteLine($"  Пройдено тестов: {passed} из {tests.Count}.");
Console.WriteLine(new string('=', 78));

return passed == tests.Count ? 0 : 1;

static (AppComposition App, Client Client, Administrator Administrator) NewWorld(
    IPaymentNotifier? paymentNotifier = null,
    IOperationNotifier? operationNotifier = null)
{
    var silent = new SilentNotifier();
    var app = AppComposition.Create(paymentNotifier ?? silent, operationNotifier ?? silent);

    var client = new Client(1, "Тестовый клиент", 5000m);
    client.AddAccount(new Account("A1", 100000m));
    client.AddAccount(new Account("A2"));
    client.AddCard(new CreditCard("C1", 50000m));
    client.AddCard(new CreditCard("C2", 30000m, 31000m));

    var administrator = new Administrator(100, "Тестовый администратор");

    app.Clients.Add(client);
    app.Orders.Add(new Order(1, 1000m));
    app.Orders.Add(new Order(2, 60000m));

    return (app, client, administrator);
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static void AssertEqual<T>(T expected, T actual, string message)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{message}: ожидалось {expected}, получено {actual}");
    }
}

/// <summary>Заглушка уведомлений для тестов (DIP).</summary>
internal sealed class RecordingNotifier : IPaymentNotifier, IOperationNotifier
{
    public List<string> Events { get; } = new();

    public int PaymentCompletedCount { get; private set; }

    public void PaymentCompleted(Order order, PaymentInstrument instrument, decimal amount, decimal commission)
    {
        PaymentCompletedCount++;
        Events.Add($"payment:{order.OrderId}:{Money.Format(amount)}");
    }

    public void PaymentRejected(Order order, PaymentInstrument instrument, string reason) =>
        Events.Add($"payment-reject:{order.OrderId}:{reason}");

    public void TransferCompleted(Account from, Account to, decimal amount) =>
        Events.Add($"transfer:{amount}");

    public void CardBlocked(IBlockable card, string reason, string initiator) =>
        Events.Add($"block:{card.Number}");

    public void AccountCancelled(Account account) =>
        Events.Add($"cancel:{account.AccountNumber}");

    public void CreditLimitsChecked(CreditLimitReport report) =>
        Events.Add($"limits:{report.ExceededCount}");

    public void OperationRejected(string reason) =>
        Events.Add($"reject:{reason}");
}

/// <summary>Тихая заглушка, чтобы тесты не печатали в консоль.</summary>
internal sealed class SilentNotifier : IPaymentNotifier, IOperationNotifier
{
    public void PaymentCompleted(Order order, PaymentInstrument instrument, decimal amount, decimal commission)
    {
    }

    public void PaymentRejected(Order order, PaymentInstrument instrument, string reason)
    {
    }

    public void TransferCompleted(Account from, Account to, decimal amount)
    {
    }

    public void CardBlocked(IBlockable card, string reason, string initiator)
    {
    }

    public void AccountCancelled(Account account)
    {
    }

    public void CreditLimitsChecked(CreditLimitReport report)
    {
    }

    public void OperationRejected(string reason)
    {
    }
}
