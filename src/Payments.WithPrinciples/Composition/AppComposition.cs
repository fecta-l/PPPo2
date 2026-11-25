using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Application.Accounts;
using Payments.WithPrinciples.Application.Administration;
using Payments.WithPrinciples.Application.Cards;
using Payments.WithPrinciples.Application.Payments;
using Payments.WithPrinciples.Application.Payments.Rules;
using Payments.WithPrinciples.Application.Transfers;
using Payments.WithPrinciples.Infrastructure;

namespace Payments.WithPrinciples.Composition;

/// <summary>
/// Композиционный корень: единственное место, где создаются конкретные реализации
/// и собирается граф зависимостей. Здесь же подключаются дополнительные правила,
/// что и обеспечивает расширение без изменения существующих классов.
/// </summary>
public sealed class AppComposition
{
    private AppComposition(
        IClientRepository clients,
        IOrderRepository orders,
        IPaymentJournal journal,
        IUserNotifier notifier,
        IOrderPaymentService payments,
        IFundsTransferService transfers,
        ICardService cards,
        IAccountService accounts,
        ICreditControlService creditControl)
    {
        Clients = clients;
        Orders = orders;
        Journal = journal;
        Notifier = notifier;
        Payments = payments;
        Transfers = transfers;
        Cards = cards;
        Accounts = accounts;
        CreditControl = creditControl;
    }

    public IClientRepository Clients { get; }

    public IOrderRepository Orders { get; }

    public IPaymentJournal Journal { get; }

    public IUserNotifier Notifier { get; }

    public IOrderPaymentService Payments { get; }

    public IFundsTransferService Transfers { get; }

    public ICardService Cards { get; }

    public IAccountService Accounts { get; }

    public ICreditControlService CreditControl { get; }

    public static AppComposition Create(
        IEnumerable<IPaymentRule>? additionalRules = null,
        IUserNotifier? notifier = null)
    {
        var clients = new InMemoryClientRepository();
        var orders = new InMemoryOrderRepository();
        var journal = new InMemoryPaymentJournal();
        notifier ??= new ConsoleNotifier();

        var rules = new List<IPaymentRule>
        {
            new OrderIsPayableRule(),
            new InstrumentOwnershipRule(),
            new CardIsNotBlockedRule(),
            new AccountIsActiveRule(),
            new CreditLimitRule(),
            new SufficientFundsRule()
        };

        if (additionalRules is not null)
        {
            rules.AddRange(additionalRules);
        }

        return new AppComposition(
            clients,
            orders,
            journal,
            notifier,
            new OrderPaymentService(rules, journal, notifier),
            new FundsTransferService(new TransferValidator(), journal, notifier),
            new CardService(journal, notifier),
            new AccountService(journal, notifier),
            new CreditControlService(clients, journal, notifier));
    }
}
