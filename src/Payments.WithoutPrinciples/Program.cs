using System.Text;
using Payments.WithoutPrinciples;

Console.OutputEncoding = Encoding.UTF8;

var system = new PaymentSystem();

var client = new Client
{
    Id = 1,
    Name = "Иванов Иван Иванович",
    BonusBalance = 5000m
};
client.Accounts.Add(new Account { AccountNumber = "40817810000000000001", Balance = 120000m });
client.Accounts.Add(new Account { AccountNumber = "40817810000000000002", Balance = 0m });
client.Cards.Add(new CreditCard { CardNumber = "4276 0000 1111 1111", CreditLimit = 50000m });
client.Cards.Add(new CreditCard { CardNumber = "4276 0000 2222 2222", CreditLimit = 30000m, CurrentDebt = 31000m });

var secondClient = new Client { Id = 2, Name = "Петров Пётр Петрович" };
secondClient.Accounts.Add(new Account { AccountNumber = "40817810000000000003", Balance = 15000m });

var administrator = new Administrator { Id = 100, Name = "Петрова Анна Сергеевна" };

system.AddClient(client);
system.AddClient(secondClient);
system.AddAdministrator(administrator);

system.AddOrder(new Order { OrderId = 1001, Amount = 12500m });
system.AddOrder(new Order { OrderId = 1002, Amount = 3200m });
system.AddOrder(new Order { OrderId = 1003, Amount = 60000m });
system.AddOrder(new Order { OrderId = 1004, Amount = 800m });
system.AddOrder(new Order { OrderId = 1005, Amount = 55000m });

Console.WriteLine(new string('=', 78));
Console.WriteLine("  СИСТЕМА «ПЛАТЕЖИ» — ВАРИАНТ 2");
Console.WriteLine("  ВЕРСИЯ БЕЗ ПРИМЕНЕНИЯ ПРИНЦИПОВ SRP И OCP");
Console.WriteLine(new string('=', 78));

Console.WriteLine();
Console.WriteLine("--- Сценарий 1. Оплата заказа картой ---");
Console.WriteLine("Клиент: Иванов Иван Иванович (id=1)");
Console.WriteLine("Оплата заказа №1001 на сумму 12 500,00 руб. картой 4276 0000 1111 1111");
system.PayForOrder(1, 1001, "4276 0000 1111 1111", PaymentType.Card);

Console.WriteLine();
Console.WriteLine("--- Сценарий 2. Оплата заказа со счёта ---");
Console.WriteLine("Оплата заказа №1002 на сумму 3 200,00 руб. со счёта 40817810000000000001");
system.PayForOrder(1, 1002, "40817810000000000001", PaymentType.BankAccount);

Console.WriteLine();
Console.WriteLine("--- Сценарий 3. Превышение кредитного лимита ---");
Console.WriteLine("Оплата заказа №1003 на сумму 60 000,00 руб. картой 4276 0000 1111 1111");
system.PayForOrder(1, 1003, "4276 0000 1111 1111", PaymentType.Card);

Console.WriteLine();
Console.WriteLine("--- Сценарий 4. Перевод средств на другой счёт ---");
Console.WriteLine("Перевод 5 000,00 руб. со счёта 40817810000000000001 на счёт 40817810000000000003");
system.TransferFunds(1, "40817810000000000001", "40817810000000000003", 5000m);

Console.WriteLine();
Console.WriteLine("--- Сценарий 5. Блокировка карты клиентом ---");
Console.WriteLine("Блокировка карты 4276 0000 1111 1111");
system.BlockCard(1, "4276 0000 1111 1111");

Console.WriteLine();
Console.WriteLine("--- Сценарий 6. Оплата заблокированной картой ---");
Console.WriteLine("Оплата заказа №1004 на сумму 800,00 руб. картой 4276 0000 1111 1111");
system.PayForOrder(1, 1004, "4276 0000 1111 1111", PaymentType.Card);

Console.WriteLine();
Console.WriteLine("--- Сценарий 7. Администратор блокирует карту за превышение кредита ---");
Console.WriteLine("Администратор: Петрова Анна Сергеевна (id=100)");
system.CheckCreditLimits(100);
system.BlockCardForDebt(100, "4276 0000 2222 2222");

Console.WriteLine();
Console.WriteLine("--- Сценарий 8. Оплата картой, заблокированной администратором ---");
Console.WriteLine("Оплата заказа №1004 на сумму 800,00 руб. картой 4276 0000 2222 2222");
system.PayForOrder(1, 1004, "4276 0000 2222 2222", PaymentType.Card);

Console.WriteLine();
Console.WriteLine("--- Сценарий 9. Оплата заказа бонусными баллами ---");
Console.WriteLine("Оплата заказа №1004 на сумму 800,00 руб. бонусными баллами");
system.PayForOrder(1, 1004, string.Empty, PaymentType.BonusPoints);

Console.WriteLine();
Console.WriteLine("--- Сценарий 10. Аннулирование счёта ---");
Console.WriteLine("Аннулирование счёта 40817810000000000002 (баланс 0,00 руб.)");
system.CancelAccount(1, "40817810000000000002");
Console.WriteLine("Аннулирование счёта 40817810000000000001 (ненулевой баланс)");
system.CancelAccount(1, "40817810000000000001");

Console.WriteLine();
Console.WriteLine("--- Итоговое состояние ---");
system.PrintState();
Console.WriteLine();
system.PrintJournal();

Console.WriteLine();
Console.WriteLine(new string('=', 78));
Console.WriteLine("  Работа программы завершена.");
Console.WriteLine(new string('=', 78));
