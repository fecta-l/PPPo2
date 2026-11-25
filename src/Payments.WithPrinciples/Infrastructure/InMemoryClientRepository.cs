using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Infrastructure;

/// <summary>
/// Простое хранилище клиентов в памяти. Может быть заменено реализацией на БД
/// без изменения прикладных сервисов.
/// </summary>
public sealed class InMemoryClientRepository : IClientRepository
{
    private readonly List<Client> _clients = new();

    public IReadOnlyList<Client> GetAll() => _clients;

    public Client? FindById(int id) => _clients.FirstOrDefault(client => client.Id == id);

    public void Add(Client client)
    {
        ArgumentNullException.ThrowIfNull(client);

        if (_clients.Any(existing => existing.Id == client.Id))
        {
            throw new InvalidOperationException($"Клиент с id={client.Id} уже существует.");
        }

        _clients.Add(client);
    }
}
