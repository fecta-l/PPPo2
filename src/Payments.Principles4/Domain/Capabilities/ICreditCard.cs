namespace Payments.Principles4.Domain.Capabilities;

/// <summary>
/// Роль кредитной карты: блокировка + сведения о лимите.
/// Интерфейс собран из двух узких возможностей, а не является «жирным».
/// </summary>
public interface ICreditCard : IBlockable, ILimitInfo
{
    new string Number { get; }
}
