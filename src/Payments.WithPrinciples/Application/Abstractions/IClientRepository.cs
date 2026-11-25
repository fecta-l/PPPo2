using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Abstractions;

/// <summary>
/// Хранилище клиентов. Позволяет заменить in-memory реализацию на БД без изменения сервисов.
/// </summary>
public interface IClientRepository
{
    IReadOnlyList<Client> GetAll();

    Client? FindById(int id);

    void Add(Client client);
}
