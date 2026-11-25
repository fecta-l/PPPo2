using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Models;

/// <summary>
/// Строка отчёта о проверке кредитного лимита.
/// </summary>
public sealed record CreditLimitEntry(Client Client, CreditCard Card, bool IsExceeded);

/// <summary>
/// Отчёт администратора о состоянии кредитных лимитов.
/// </summary>
public sealed record CreditLimitReport(IReadOnlyList<CreditLimitEntry> Entries, int ExceededCount);
