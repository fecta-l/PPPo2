using Payments.Principles4.Application.Abstractions;

namespace Payments.Principles4.Infrastructure;

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
