using Payments.Principles4.Application.Models;
using Payments.Principles4.Domain.Capabilities;

namespace Payments.Principles4.Application.Abstractions;

/// <summary>
/// Блокировка карты. Сервис зависит только от IBlockable и не знает
/// о способах оплаты, переводах и счетах (ISP).
/// </summary>
public interface ICardService
{
    OperationResult Block(IBlockable card, string reason);
}
