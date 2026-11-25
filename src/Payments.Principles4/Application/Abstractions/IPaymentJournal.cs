namespace Payments.Principles4.Application.Abstractions;

public interface IPaymentJournal
{
    IReadOnlyList<string> Entries { get; }

    void Record(string entry);
}
