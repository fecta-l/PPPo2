using Payments.Principles4.Application.Abstractions;
using Payments.Principles4.Application.Services;
using Payments.Principles4.Infrastructure;

namespace Payments.Principles4.Composition;

/// <summary>
/// Композиционный корень. Единственное место, где создаются конкретные
/// реализации; сервисы получают только абстракции (DIP).
/// </summary>
public sealed class AppComposition
{
    private AppComposition(
        IClientRepository clients,
        IOrderRepository orders,
        IPaymentJournal journal,
        IOrderPaymentService payments,
        ITransferService transfers,
        ICardService cards,
        IAccountService accounts,
        ICreditControlService creditControl)
    {
        Clients = clients;
        Orders = orders;
        Journal = journal;
        Payments = payments;
        Transfers = transfers;
        Cards = cards;
        Accounts = accounts;
        CreditControl = creditControl;
    }

    public IClientRepository Clients { get; }

    public IOrderRepository Orders { get; }

    public IPaymentJournal Journal { get; }

    public IOrderPaymentService Payments { get; }

    public ITransferService Transfers { get; }

    public ICardService Cards { get; }

    public IAccountService Accounts { get; }

    public ICreditControlService CreditControl { get; }

    public static AppComposition Create(
        IPaymentNotifier? paymentNotifier = null,
        IOperationNotifier? operationNotifier = null)
    {
        var console = new ConsoleNotifier();
        paymentNotifier ??= console;
        operationNotifier ??= console;

        var clients = new InMemoryClientRepository();
        var orders = new InMemoryOrderRepository();
        var journal = new InMemoryPaymentJournal();

        return new AppComposition(
            clients,
            orders,
            journal,
            new OrderPaymentService(journal, paymentNotifier),
            new TransferService(journal, operationNotifier),
            new CardService(journal, operationNotifier),
            new AccountService(journal, operationNotifier),
            new CreditControlService(clients, journal, operationNotifier));
    }
}
