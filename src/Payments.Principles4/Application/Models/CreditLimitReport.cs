using Payments.Principles4.Domain;
using Payments.Principles4.Domain.Capabilities;

namespace Payments.Principles4.Application.Models;

public sealed record CreditLimitEntry(Client Client, ILimitInfo Card, bool IsExceeded);

public sealed record CreditLimitReport(IReadOnlyList<CreditLimitEntry> Entries, int ExceededCount);
