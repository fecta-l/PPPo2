using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Infrastructure;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = new();

    public IReadOnlyList<Order> GetAll() => _orders;

    public Order? FindById(int id) => _orders.FirstOrDefault(order => order.OrderId == id);

    public void Add(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (_orders.Any(existing => existing.OrderId == order.OrderId))
        {
            throw new InvalidOperationException($"Заказ №{order.OrderId} уже существует.");
        }

        _orders.Add(order);
    }
}
