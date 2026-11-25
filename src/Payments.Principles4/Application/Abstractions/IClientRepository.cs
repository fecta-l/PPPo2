using Payments.Principles4.Domain;

namespace Payments.Principles4.Application.Abstractions;

public interface IClientRepository
{
    IReadOnlyList<Client> GetAll();

    Client? FindById(int id);

    void Add(Client client);
}
