using System.Globalization;

namespace Payments.WithoutPrinciples;

/// <summary>
/// "Божественный объект": хранит данные, проверяет правила, выполняет операции,
/// ведёт журнал и сам печатает отчёты в консоль.
/// </summary>
public class PaymentSystem
{
    private readonly List<Client> _clients = new();
    private readonly List<Administrator> _administrators = new();
    private readonly List<Order> _orders = new();
    private readonly List<string> _journal = new();

    public void AddClient(Client client) => _clients.Add(client);

    public void AddAdministrator(Administrator administrator) => _administrators.Add(administrator);

    public void AddOrder(Order order) => _orders.Add(order);

    public Client GetClient(int id) => _clients.First(c => c.Id == id);

    public Administrator GetAdministrator(int id) => _administrators.First(a => a.Id == id);

    public Order GetOrder(int id) => _orders.First(o => o.OrderId == id);

    public void PayForOrder(int clientId, int orderId, string instrumentNumber, PaymentType type)
    {
        var client = _clients.FirstOrDefault(c => c.Id == clientId);
        if (client == null)
        {
            Console.WriteLine("  Отказ: клиент не найден.");
            return;
        }

        var order = _orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null)
        {
            Console.WriteLine("  Отказ: заказ не найден.");
            return;
        }

        if (order.Status == "Paid")
        {
            Console.WriteLine("  Отказ: заказ уже оплачен.");
            return;
        }

        // Комиссия зависит от способа оплаты. При добавлении нового способа
        // приходится править и эту конструкцию, и ветвления ниже.
        decimal commissionRate;
        if (type == PaymentType.Card)
        {
            commissionRate = 0m;
        }
        else if (type == PaymentType.BankAccount)
        {
            commissionRate = 0.01m;
        }
        else if (type == PaymentType.BonusPoints)
        {
            commissionRate = 0m;
        }
        else
        {
            Console.WriteLine("  Отказ: неизвестный способ оплаты.");
            return;
        }

        var commission = order.Amount * commissionRate;
        var total = order.Amount + commission;

        if (type == PaymentType.Card)
        {
            var card = client.Cards.FirstOrDefault(c => c.CardNumber == instrumentNumber);
            if (card == null)
            {
                Console.WriteLine("  Отказ: карта не принадлежит клиенту.");
                return;
            }

            if (card.IsBlocked)
            {
                Console.WriteLine($"  Отказ: карта {card.CardNumber} заблокирована.");
                return;
            }

            if (card.CurrentDebt + total > card.CreditLimit)
            {
                Console.WriteLine(
                    $"  Отказ: недостаточно доступного лимита (требуется {Money(total)} руб., " +
                    $"доступно {Money(card.CreditLimit - card.CurrentDebt)} руб.).");
                return;
            }

            card.CurrentDebt += total;
            order.Status = "Paid";
            _journal.Add($"ОПЛАТА заказа №{order.OrderId} картой {card.CardNumber} на {Money(total)} руб.");
            Console.WriteLine($"  Проверка: карта не заблокирована, доступный лимит {Money(card.CreditLimit - card.CurrentDebt + total)} руб.");
            Console.WriteLine($"  Комиссия: {Money(commission)} руб.");
            Console.WriteLine($"  Списано: {Money(total)} руб. Задолженность по карте: {Money(card.CurrentDebt)} руб.");
            Console.WriteLine($"  Заказ №{order.OrderId}: статус \"Paid\".");
            Console.WriteLine("  Результат: оплата выполнена.");
            return;
        }

        if (type == PaymentType.BankAccount)
        {
            var account = client.Accounts.FirstOrDefault(a => a.AccountNumber == instrumentNumber);
            if (account == null)
            {
                Console.WriteLine("  Отказ: счёт не принадлежит клиенту.");
                return;
            }

            if (account.Status != "Active")
            {
                Console.WriteLine($"  Отказ: счёт {account.AccountNumber} аннулирован.");
                return;
            }

            if (account.Balance < total)
            {
                Console.WriteLine(
                    $"  Отказ: недостаточно средств (требуется {Money(total)} руб., " +
                    $"доступно {Money(account.Balance)} руб.).");
                return;
            }

            account.Balance -= total;
            order.Status = "Paid";
            _journal.Add($"ОПЛАТА заказа №{order.OrderId} со счёта {account.AccountNumber} на {Money(total)} руб.");
            Console.WriteLine($"  Проверка: счёт активен, баланс {Money(account.Balance + total)} руб.");
            Console.WriteLine($"  Комиссия (1%): {Money(commission)} руб.");
            Console.WriteLine($"  Списано: {Money(total)} руб. Остаток на счёте: {Money(account.Balance)} руб.");
            Console.WriteLine($"  Заказ №{order.OrderId}: статус \"Paid\".");
            Console.WriteLine("  Результат: оплата выполнена.");
            return;
        }

        // Ветка BonusPoints. Чтобы её добавить, пришлось изменить enum PaymentType,
        // класс Client (поле BonusBalance) и этот метод — три причины изменения одного класса.
        if (client.BonusBalance < total)
        {
            Console.WriteLine(
                $"  Отказ: недостаточно бонусных баллов (требуется {Money(total)}, " +
                $"доступно {Money(client.BonusBalance)}).");
            return;
        }

        client.BonusBalance -= total;
        order.Status = "Paid";
        _journal.Add($"ОПЛАТА заказа №{order.OrderId} бонусными баллами на {Money(total)}.");
        Console.WriteLine($"  Проверка: бонусный баланс {Money(client.BonusBalance + total)}.");
        Console.WriteLine($"  Комиссия: {Money(commission)}.");
        Console.WriteLine($"  Списано: {Money(total)} баллов. Остаток: {Money(client.BonusBalance)}.");
        Console.WriteLine($"  Заказ №{order.OrderId}: статус \"Paid\".");
        Console.WriteLine("  Результат: оплата выполнена.");
    }

    public void TransferFunds(int clientId, string fromNumber, string toNumber, decimal amount)
    {
        var client = _clients.FirstOrDefault(c => c.Id == clientId);
        if (client == null)
        {
            Console.WriteLine("  Отказ: клиент не найден.");
            return;
        }

        var from = client.Accounts.FirstOrDefault(a => a.AccountNumber == fromNumber);
        if (from == null)
        {
            Console.WriteLine("  Отказ: счёт списания не принадлежит клиенту.");
            return;
        }

        if (from.Status != "Active")
        {
            Console.WriteLine("  Отказ: счёт списания аннулирован.");
            return;
        }

        if (amount <= 0)
        {
            Console.WriteLine("  Отказ: сумма перевода должна быть положительной.");
            return;
        }

        if (from.Balance < amount)
        {
            Console.WriteLine($"  Отказ: недостаточно средств на счёте (доступно {Money(from.Balance)} руб.).");
            return;
        }

        if (fromNumber == toNumber)
        {
            Console.WriteLine("  Отказ: счета списания и зачисления совпадают.");
            return;
        }

        Account? to = null;
        foreach (var candidate in _clients)
        {
            to = candidate.Accounts.FirstOrDefault(a => a.AccountNumber == toNumber);
            if (to != null)
            {
                break;
            }
        }

        if (to == null)
        {
            Console.WriteLine("  Отказ: счёт зачисления не найден.");
            return;
        }

        if (to.Status != "Active")
        {
            Console.WriteLine("  Отказ: счёт зачисления аннулирован.");
            return;
        }

        from.Balance -= amount;
        to.Balance += amount;
        _journal.Add($"ПЕРЕВОД {Money(amount)} руб. со счёта {from.AccountNumber} на счёт {to.AccountNumber}.");
        Console.WriteLine($"  Списано: {Money(amount)} руб. Остаток на счёте {from.AccountNumber}: {Money(from.Balance)} руб.");
        Console.WriteLine($"  Зачислено на счёт {to.AccountNumber}. Баланс: {Money(to.Balance)} руб.");
        Console.WriteLine("  Результат: перевод выполнен.");
    }

    public void BlockCard(int clientId, string cardNumber)
    {
        var client = _clients.FirstOrDefault(c => c.Id == clientId);
        if (client == null)
        {
            Console.WriteLine("  Отказ: клиент не найден.");
            return;
        }

        var card = client.Cards.FirstOrDefault(c => c.CardNumber == cardNumber);
        if (card == null)
        {
            Console.WriteLine("  Отказ: карта не принадлежит клиенту.");
            return;
        }

        if (card.IsBlocked)
        {
            Console.WriteLine($"  Карта {card.CardNumber} уже заблокирована.");
            return;
        }

        card.IsBlocked = true;
        _journal.Add($"БЛОКИРОВКА карты {card.CardNumber} клиентом {client.Name}.");
        Console.WriteLine($"  Карта {card.CardNumber} заблокирована клиентом.");
    }

    public void CancelAccount(int clientId, string accountNumber)
    {
        var client = _clients.FirstOrDefault(c => c.Id == clientId);
        if (client == null)
        {
            Console.WriteLine("  Отказ: клиент не найден.");
            return;
        }

        var account = client.Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
        if (account == null)
        {
            Console.WriteLine("  Отказ: счёт не принадлежит клиенту.");
            return;
        }

        if (account.Status != "Active")
        {
            Console.WriteLine("  Отказ: счёт уже аннулирован.");
            return;
        }

        if (account.Balance != 0m)
        {
            Console.WriteLine($"  Отказ: нельзя аннулировать счёт с ненулевым балансом ({Money(account.Balance)} руб.).");
            return;
        }

        account.Status = "Cancelled";
        _journal.Add($"АННУЛИРОВАНИЕ счёта {account.AccountNumber} клиентом {client.Name}.");
        Console.WriteLine($"  Счёт {account.AccountNumber} аннулирован.");
    }

    public void BlockCardForDebt(int administratorId, string cardNumber)
    {
        var administrator = _administrators.FirstOrDefault(a => a.Id == administratorId);
        if (administrator == null)
        {
            Console.WriteLine("  Отказ: администратор не найден.");
            return;
        }

        CreditCard? card = null;
        foreach (var client in _clients)
        {
            card = client.Cards.FirstOrDefault(c => c.CardNumber == cardNumber);
            if (card != null)
            {
                break;
            }
        }

        if (card == null)
        {
            Console.WriteLine("  Отказ: карта не найдена.");
            return;
        }

        if (card.CurrentDebt <= card.CreditLimit)
        {
            Console.WriteLine("  Превышение кредитного лимита отсутствует — блокировка не требуется.");
            return;
        }

        if (card.IsBlocked)
        {
            Console.WriteLine($"  Карта {card.CardNumber} уже заблокирована.");
            return;
        }

        card.IsBlocked = true;
        _journal.Add($"БЛОКИРОВКА карты {card.CardNumber} администратором {administrator.Name} за превышение кредита.");
        Console.WriteLine($"  Карта {card.CardNumber} заблокирована администратором за превышение кредитного лимита.");
    }

    public void CheckCreditLimits(int administratorId)
    {
        var administrator = _administrators.FirstOrDefault(a => a.Id == administratorId);
        if (administrator == null)
        {
            Console.WriteLine("  Отказ: администратор не найден.");
            return;
        }

        Console.WriteLine("  Проверка кредитных лимитов:");
        var exceeded = 0;
        foreach (var client in _clients)
        {
            foreach (var card in client.Cards)
            {
                var isExceeded = card.CurrentDebt > card.CreditLimit;
                if (isExceeded)
                {
                    exceeded++;
                }

                Console.WriteLine(
                    $"    {card.CardNumber} ({client.Name}): долг {Money(card.CurrentDebt)} / " +
                    $"лимит {Money(card.CreditLimit)} — {(isExceeded ? "ПРЕВЫШЕН" : "в норме")}");
            }
        }

        Console.WriteLine($"  Карт с превышением лимита: {exceeded}.");
    }

    public void PrintJournal()
    {
        Console.WriteLine("  Журнал операций:");
        foreach (var record in _journal)
        {
            Console.WriteLine($"    - {record}");
        }
    }

    public void PrintState()
    {
        foreach (var client in _clients)
        {
            Console.WriteLine($"  {client.Name}: бонусы {Money(client.BonusBalance)}");
            foreach (var account in client.Accounts)
            {
                Console.WriteLine($"    счёт {account.AccountNumber}: {Money(account.Balance)} руб., статус {account.Status}");
            }

            foreach (var card in client.Cards)
            {
                Console.WriteLine(
                    $"    карта {card.CardNumber}: долг {Money(card.CurrentDebt)} / лимит {Money(card.CreditLimit)}, " +
                    $"{(card.IsBlocked ? "заблокирована" : "активна")}");
            }
        }

        Console.WriteLine("  Заказы:");
        foreach (var order in _orders)
        {
            Console.WriteLine($"    №{order.OrderId}: {Money(order.Amount)} руб., статус {order.Status}");
        }
    }

    private static string Money(decimal value) =>
        value.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")).Replace('\u00A0', ' ');
}
