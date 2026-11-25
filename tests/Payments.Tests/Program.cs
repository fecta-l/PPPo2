using System.Text;
using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Application.Payments.Instruments;
using Payments.WithPrinciples.Application.Payments.Rules;
using Payments.WithPrinciples.Composition;
using Payments.WithPrinciples.Domain;

Console.OutputEncoding = Encoding.UTF8;

var tests = new List<(string Name, Action Run)>
{
    ("Оплата картой: заказ оплачен, задолженность увеличена", () =>
    {
        var (app, client, _) = NewWorld();
        var card = client.Cards[0];

        var result = app.Payments.PayForOrder(
            client,
            app.Orders.FindById(1)!,
            new CardPaymentInstrument(client, card));

        Assert(result.IsSuccess, "платёж должен быть успешным");
        AssertEqual(1000m, card.CurrentDebt, "задолженность по карте");
        Assert(app.Orders.FindById(1)!.IsPaid, "заказ должен быть оплачен");
    }),

    ("Комиссия 1% при оплате с банковского счёта", () =>
    {
        var (app, client, _) = NewWorld();
        var account = client.Accounts[0];

        var result = app.Payments.PayForOrder(
            client,
            app.Orders.FindById(1)!,
            new AccountPaymentInstrument(client, account));

        Assert(result.IsSuccess, "платёж должен быть успешным");
        AssertEqual(1010m, result.ChargedAmount, "сумма списания с комиссией");
        AssertEqual(10m, result.Commission, "комиссия");
        AssertEqual(98990m, account.Balance, "остаток на счёте");
    }),

    ("Отказ при превышении кредитного лимита", () =>
    {
        var (app, client, _) = NewWorld();
        var card = client.Cards[0];

        var result = app.Payments.PayForOrder(
            client,
            app.Orders.FindById(2)!,
            new CardPaymentInstrument(client, card));

        Assert(!result.IsSuccess, "платёж должен быть отклонён");
        Assert(result.Message.Contains("лимита"), "причина должна содержать слово «лимита»");
        AssertEqual(0m, card.CurrentDebt, "задолженность не должна измениться");
        Assert(!app.Orders.FindById(2)!.IsPaid, "заказ не должен быть оплачен");
    }),

    ("Отказ при оплате заблокированной картой", () =>
    {
        var (app, client, _) = NewWorld();
        var card = client.Cards[0];
        card.Block("тестовая блокировка");

        var result = app.Payments.PayForOrder(
            client,
            app.Orders.FindById(1)!,
            new CardPaymentInstrument(client, card));

        Assert(!result.IsSuccess, "платёж должен быть отклонён");
        Assert(result.Message.Contains("заблокирована"), "причина должна содержать слово «заблокирована»");
    }),

    ("Клиент блокирует карту", () =>
    {
        var (app, client, _) = NewWorld();
        var card = client.Cards[0];

        var result = app.Cards.Block(client, card, "по требованию клиента");

        Assert(result.IsSuccess, "блокировка должна быть успешной");
        Assert(card.IsBlocked, "карта должна быть заблокирована");
    }),

    ("Администратор блокирует карту за превышение кредита", () =>
    {
        var (app, client, administrator) = NewWorld();
        var card = client.Cards[1];

        var report = app.CreditControl.CheckLimits(administrator);
        var blockResult = app.CreditControl.BlockCardForDebt(administrator, card);

        AssertEqual(1, report.ExceededCount, "количество карт с превышением лимита");
        Assert(blockResult.IsSuccess, "блокировка должна быть успешной");
        Assert(card.IsBlocked, "карта должна быть заблокирована");
    }),

    ("Перевод средств между счетами", () =>
    {
        var (app, client, _) = NewWorld();
        var from = client.Accounts[0];
        var to = client.Accounts[1];

        var result = app.Transfers.Transfer(client, from, to, 5000m);

        Assert(result.IsSuccess, "перевод должен быть успешным");
        AssertEqual(95000m, from.Balance, "баланс счёта списания");
        AssertEqual(5000m, to.Balance, "баланс счёта зачисления");
    }),

    ("Нельзя аннулировать счёт с ненулевым балансом", () =>
    {
        var (app, client, _) = NewWorld();
        var withMoney = client.Accounts[0];
        var empty = client.Accounts[1];

        var rejected = app.Accounts.Cancel(client, withMoney);
        var cancelled = app.Accounts.Cancel(client, empty);

        Assert(!rejected.IsSuccess, "счёт с балансом нельзя аннулировать");
        Assert(cancelled.IsSuccess, "пустой счёт должен аннулироваться");
        AssertEqual(AccountStatus.Cancelled, empty.Status, "статус пустого счёта");
    }),

    ("Оплата бонусными баллами", () =>
    {
        var (app, client, _) = NewWorld();

        var result = app.Payments.PayForOrder(client, app.Orders.FindById(1)!, new BonusPaymentInstrument(client));

        Assert(result.IsSuccess, "платёж бонусами должен быть успешным");
        AssertEqual(4000m, client.BonusBalance, "остаток бонусных баллов");
        Assert(app.Orders.FindById(1)!.IsPaid, "заказ должен быть оплачен");
    }),

    ("Новое правило DailyLimitRule отклоняет крупный платёж", () =>
    {
        var (app, client, _) = NewWorld(new IPaymentRule[] { new DailyLimitRule(500m) });

        var result = app.Payments.PayForOrder(
            client,
            app.Orders.FindById(1)!,
            new AccountPaymentInstrument(client, client.Accounts[0]));

        Assert(!result.IsSuccess, "платёж должен быть отклонён новым правилом");
        Assert(result.Message.Contains("дневной лимит"), "причина должна упоминать дневной лимит");
        Assert(!app.Orders.FindById(1)!.IsPaid, "заказ не должен быть оплачен");
    }),

    ("Нельзя создать инструмент для чужой карты", () =>
    {
        var (_, client, _) = NewWorld();
        var other = new Client(2, "Другой клиент");
        other.AddCard(new CreditCard("C3", 10000m));

        Expect<DomainException>(
            () => new CardPaymentInstrument(client, other.Cards[0]),
            "создание инструмента для чужой карты должно завершиться ошибкой");
    }),

    ("Повторная оплата заказа запрещена", () =>
    {
        var (app, client, _) = NewWorld();
        var order = app.Orders.FindById(1)!;
        var card = client.Cards[0];

        var first = app.Payments.PayForOrder(client, order, new CardPaymentInstrument(client, card));
        var second = app.Payments.PayForOrder(client, order, new CardPaymentInstrument(client, card));

        Assert(first.IsSuccess, "первый платёж должен пройти");
        Assert(!second.IsSuccess, "второй платёж должен быть отклонён");
        Assert(second.Message.Contains("статус"), "причина должна содержать статус заказа");
    }),

    ("Журнал фиксирует отказ в оплате", () =>
    {
        var (app, client, _) = NewWorld();
        app.Payments.PayForOrder(
            client,
            app.Orders.FindById(2)!,
            new CardPaymentInstrument(client, client.Cards[0]));

        Assert(
            app.Journal.Entries.Any(entry => entry.StartsWith("ОТКАЗ")),
            "в журнале должна быть запись об отказе");
    })
};

Console.WriteLine(new string('=', 78));
Console.WriteLine("  МОДУЛЬНЫЕ ТЕСТЫ Payments.WithPrinciples");
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
    IEnumerable<IPaymentRule>? additionalRules = null)
{
    var app = AppComposition.Create(additionalRules, new SilentNotifier());

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

static void Expect<TException>(Action action, string message)
    where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException(message);
}

/// <summary>
/// Заглушка представления для тестов: сервисы не печатают в консоль.
/// </summary>
internal sealed class SilentNotifier : IUserNotifier
{
    public void PaymentCompleted(Client client, Order order, IPaymentInstrument instrument, decimal amount, decimal commission)
    {
    }

    public void PaymentRejected(Client client, Order order, IPaymentInstrument instrument, string reason)
    {
    }

    public void TransferCompleted(Client client, Account from, Account to, decimal amount)
    {
    }

    public void CardBlocked(Client? client, CreditCard card, string reason, string initiator)
    {
    }

    public void AccountCancelled(Client client, Account account)
    {
    }

    public void CreditLimitsChecked(CreditLimitReport report)
    {
    }

    public void OperationRejected(string reason)
    {
    }
}
