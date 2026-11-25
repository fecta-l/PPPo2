using Payments.WithPrinciples.Application.Abstractions;

namespace Payments.WithPrinciples.Infrastructure;

/// <summary>
/// Журнал операций в памяти. Хранение истории вынесено из сервисов.
/// </summary>
public sealed class InMemoryPaymentJournal : IPaymentJournal
{
    private readonly List<string> _entries = new();

    public IReadOnlyList<string> Entries => _entries;

    public void Record(string entry)
    {
        if (string.IsNullOrWhiteSpace(entry))
        {
            throw new ArgumentException("Запись журнала не может быть пустой.", nameof(entry));
        }

        _entries.Add(entry);
    }
}
