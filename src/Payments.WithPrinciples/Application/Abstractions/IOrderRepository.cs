using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Abstractions;

public interface IOrderRepository
{
    IReadOnlyList<Order> GetAll();

    Order? FindById(int id);

    void Add(Order order);
}
