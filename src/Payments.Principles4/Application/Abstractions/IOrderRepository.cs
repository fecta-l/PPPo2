using Payments.Principles4.Domain;

namespace Payments.Principles4.Application.Abstractions;

public interface IOrderRepository
{
    IReadOnlyList<Order> GetAll();

    Order? FindById(int id);

    void Add(Order order);
}
