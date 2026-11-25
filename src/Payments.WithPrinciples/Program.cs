using System.Text;
using Payments.WithPrinciples.Application.Payments.Instruments;
using Payments.WithPrinciples.Application.Payments.Rules;
using Payments.WithPrinciples.Common;
using Payments.WithPrinciples.Composition;
using Payments.WithPrinciples.Domain;

Console.OutputEncoding = Encoding.UTF8;

var app = AppComposition.Create();
var (client, secondClient, administrator) = Seed(app);

Console.WriteLine(new string('=', 78));
Console.WriteLine("  СИСТЕМА «ПЛАТЕЖИ» — ВАРИАНТ 2");
Console.WriteLine("  ВЕРСИЯ С ПРИМЕНЕНИЕМ ПРИНЦИПОВ SRP И OCP");
Console.WriteLine(new string('=', 78));

Console.WriteLine();
Console.WriteLine("--- Сценарий 1. Оплата заказа картой ---");
Console.WriteLine("Клиент: Иванов Иван Иванович (id=1)");
Console.WriteLine("Оплата заказа №1001 на сумму 12 500,00 руб. картой 4276 0000 1111 1111");
app.Payments.PayForOrder(
    client,
    OrderById(app, 1001),
    new CardPaymentInstrument(client, CardOf(client, "4276 0000 1111 1111")));

Console.WriteLine();
Console.WriteLine("--- Сценарий 2. Оплата заказа со счёта ---");
Console.WriteLine("Оплата заказа №1002 на сумму 3 200,00 руб. со счёта 40817810000000000001");
app.Payments.PayForOrder(
    client,
    OrderById(app, 1002),
    new AccountPaymentInstrument(client, AccountOf(client, "40817810000000000001")));

Console.WriteLine();
Console.WriteLine("--- Сценарий 3. Превышение кредитного лимита ---");
Console.WriteLine("Оплата заказа №1003 на сумму 60 000,00 руб. картой 4276 0000 1111 1111");
app.Payments.PayForOrder(
    client,
    OrderById(app, 1003),
    new CardPaymentInstrument(client, CardOf(client, "4276 0000 1111 1111")));

Console.WriteLine();
Console.WriteLine("--- Сценарий 4. Перевод средств на другой счёт ---");
Console.WriteLine("Перевод 5 000,00 руб. со счёта 40817810000000000001 на счёт 40817810000000000003");
app.Transfers.Transfer(
    client,
    AccountOf(client, "40817810000000000001"),
    AccountOf(secondClient, "40817810000000000003"),
    5000m);

Console.WriteLine();
Console.WriteLine("--- Сценарий 5. Блокировка карты клиентом ---");
Console.WriteLine("Блокировка карты 4276 0000 1111 1111");
app.Cards.Block(client, CardOf(client, "4276 0000 1111 1111"), "по требованию клиента");

Console.WriteLine();
Console.WriteLine("--- Сценарий 6. Оплата заблокированной картой ---");
Console.WriteLine("Оплата заказа №1004 на сумму 800,00 руб. картой 4276 0000 1111 1111");
app.Payments.PayForOrder(
    client,
    OrderById(app, 1004),
    new CardPaymentInstrument(client, CardOf(client, "4276 0000 1111 1111")));

Console.WriteLine();
Console.WriteLine("--- Сценарий 7. Администратор блокирует карту за превышение кредита ---");
Console.WriteLine("Администратор: Петрова Анна Сергеевна (id=100)");
app.CreditControl.CheckLimits(administrator);
app.CreditControl.BlockCardForDebt(administrator, CardOf(client, "4276 0000 2222 2222"));

Console.WriteLine();
Console.WriteLine("--- Сценарий 8. Оплата картой, заблокированной администратором ---");
Console.WriteLine("Оплата заказа №1004 на сумму 800,00 руб. картой 4276 0000 2222 2222");
app.Payments.PayForOrder(
    client,
    OrderById(app, 1004),
    new CardPaymentInstrument(client, CardOf(client, "4276 0000 2222 2222")));

Console.WriteLine();
Console.WriteLine("--- Сценарий 9. Оплата заказа бонусными баллами ---");
Console.WriteLine("Оплата заказа №1004 на сумму 800,00 руб. бонусными баллами");
app.Payments.PayForOrder(client, OrderById(app, 1004), new BonusPaymentInstrument(client));

Console.WriteLine();
Console.WriteLine("--- Сценарий 10. Аннулирование счёта ---");
Console.WriteLine("Аннулирование счёта 40817810000000000002 (баланс 0,00 руб.)");
app.Accounts.Cancel(client, AccountOf(client, "40817810000000000002"));
Console.WriteLine("Аннулирование счёта 40817810000000000001 (ненулевой баланс)");
app.Accounts.Cancel(client, AccountOf(client, "40817810000000000001"));

Console.WriteLine();
Console.WriteLine("--- Итоговое состояние ---");
PrintState(app);
Console.WriteLine();
Console.WriteLine("  Журнал операций:");
foreach (var record in app.Journal.Entries)
{
    Console.WriteLine($"    - {record}");
}

Console.WriteLine();
Console.WriteLine("--- Демонстрация OCP: новое правило без изменения существующих классов ---");
Console.WriteLine("В композицию добавлено правило: new DailyLimitRule(50 000,00 руб.).");
Console.WriteLine("Класс OrderPaymentService и существующие правила не изменялись.");

var standardWorld = AppComposition.Create();
var (standardClient, _, _) = Seed(standardWorld);
Console.WriteLine();
Console.WriteLine("Без дополнительного правила:");
Console.WriteLine("Оплата заказа №1005 на сумму 55 000,00 руб. со счёта 40817810000000000001");
standardWorld.Payments.PayForOrder(
    standardClient,
    OrderById(standardWorld, 1005),
    new AccountPaymentInstrument(standardClient, AccountOf(standardClient, "40817810000000000001")));

var limitedWorld = AppComposition.Create(new[] { new DailyLimitRule(50000m) });
var (limitedClient, _, _) = Seed(limitedWorld);
Console.WriteLine();
Console.WriteLine("С дополнительным правилом DailyLimitRule:");
Console.WriteLine("Оплата заказа №1005 на сумму 55 000,00 руб. со счёта 40817810000000000001");
limitedWorld.Payments.PayForOrder(
    limitedClient,
    OrderById(limitedWorld, 1005),
    new AccountPaymentInstrument(limitedClient, AccountOf(limitedClient, "40817810000000000001")));

Console.WriteLine();
Console.WriteLine(new string('=', 78));
Console.WriteLine("  Работа программы завершена.");
Console.WriteLine(new string('=', 78));

static (Client Client, Client SecondClient, Administrator Administrator) Seed(AppComposition app)
{
    var client = new Client(1, "Иванов Иван Иванович", 5000m);
    client.AddAccount(new Account("40817810000000000001", 120000m));
    client.AddAccount(new Account("40817810000000000002"));
    client.AddCard(new CreditCard("4276 0000 1111 1111", 50000m));
    client.AddCard(new CreditCard("4276 0000 2222 2222", 30000m, 31000m));

    var secondClient = new Client(2, "Петров Пётр Петрович");
    secondClient.AddAccount(new Account("40817810000000000003", 15000m));

    var administrator = new Administrator(100, "Петрова Анна Сергеевна");

    app.Clients.Add(client);
    app.Clients.Add(secondClient);

    app.Orders.Add(new Order(1001, 12500m));
    app.Orders.Add(new Order(1002, 3200m));
    app.Orders.Add(new Order(1003, 60000m));
    app.Orders.Add(new Order(1004, 800m));
    app.Orders.Add(new Order(1005, 55000m));

    return (client, secondClient, administrator);
}

static Order OrderById(AppComposition app, int id) =>
    app.Orders.FindById(id) ?? throw new InvalidOperationException($"Заказ №{id} не найден.");

static Account AccountOf(Client client, string number) =>
    client.Accounts.Single(account => account.AccountNumber == number);

static CreditCard CardOf(Client client, string number) =>
    client.Cards.Single(card => card.CardNumber == number);

static void PrintState(AppComposition app)
{
    foreach (var client in app.Clients.GetAll())
    {
        Console.WriteLine($"  {client.Name}: бонусы {Money.Format(client.BonusBalance)}");
        foreach (var account in client.Accounts)
        {
            Console.WriteLine($"    счёт {account.AccountNumber}: {Money.Format(account.Balance)} руб., статус {account.Status}");
        }

        foreach (var card in client.Cards)
        {
            Console.WriteLine(
                $"    карта {card.CardNumber}: долг {Money.Format(card.CurrentDebt)} / лимит {Money.Format(card.CreditLimit)}, " +
                $"{(card.IsBlocked ? "заблокирована" : "активна")}");
        }
    }

    Console.WriteLine("  Заказы:");
    foreach (var order in app.Orders.GetAll())
    {
        Console.WriteLine($"    №{order.OrderId}: {Money.Format(order.Amount)} руб., статус {order.Status}");
    }
}
