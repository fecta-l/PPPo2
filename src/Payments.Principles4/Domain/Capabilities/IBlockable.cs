namespace Payments.Principles4.Domain.Capabilities;

/// <summary>
/// Узкая возможность «карту можно заблокировать». Реализуется только картой,
/// поэтому бонусный инструмент не обязан иметь бессмысленный Block().
/// </summary>
public interface IBlockable
{
    string Number { get; }

    bool IsBlocked { get; }

    string? BlockReason { get; }

    void Block(string reason);

    void Unblock();
}
