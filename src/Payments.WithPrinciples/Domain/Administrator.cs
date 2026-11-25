namespace Payments.WithPrinciples.Domain;

/// <summary>
/// Администратор банка. Отвечает только за собственные данные;
/// операции администрирования выполняет CreditControlService.
/// </summary>
public sealed class Administrator : User
{
    public Administrator(int id, string name)
        : base(id, name)
    {
    }
}
