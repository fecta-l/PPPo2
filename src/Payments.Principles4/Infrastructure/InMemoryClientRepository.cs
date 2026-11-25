using Payments.Principles4.Application.Abstractions;
using Payments.Principles4.Domain;

namespace Payments.Principles4.Infrastructure;

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
