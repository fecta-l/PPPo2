namespace Payments.WithPrinciples.Application.Abstractions;

/// <summary>
/// Журнал операций. Вынесен отдельно, чтобы сервисы не занимались хранением истории.
/// </summary>
public interface IPaymentJournal
{
    IReadOnlyList<string> Entries { get; }

    void Record(string entry);
}
