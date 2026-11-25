namespace Payments.WithPrinciples.Domain;

/// <summary>
/// Нарушение бизнес-правила предметной области.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
