namespace Payments.Principles4.Domain.Capabilities;

/// <summary>
/// Узкая возможность «у инструмента есть кредитный лимит».
/// </summary>
public interface ILimitInfo
{
    string Number { get; }

    decimal CreditLimit { get; }

    decimal CurrentDebt { get; }

    decimal AvailableLimit { get; }

    bool IsLimitExceeded { get; }
}
