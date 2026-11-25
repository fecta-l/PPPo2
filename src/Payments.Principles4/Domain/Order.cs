namespace Payments.Principles4.Domain;

/// <summary>
/// Заказ клиента. Отвечает за сумму и статус оплаты.
/// </summary>
public sealed class Order
{
    public Order(int orderId, decimal amount)
    {
        if (orderId <= 0)
        {
            throw new DomainException("Номер заказа должен быть положительным.");
        }

        if (amount <= 0m)
        {
            throw new DomainException("Сумма заказа должна быть положительной.");
        }

        OrderId = orderId;
        Amount = amount;
        Status = OrderStatus.New;
    }

    public int OrderId { get; }

    public decimal Amount { get; }

    public OrderStatus Status { get; private set; }

    public bool IsPaid => Status == OrderStatus.Paid;

    public void MarkPaid()
    {
        if (Status != OrderStatus.New)
        {
            throw new DomainException($"Заказ №{OrderId} нельзя оплатить: текущий статус — {Status}.");
        }

        Status = OrderStatus.Paid;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Paid)
        {
            throw new DomainException($"Оплаченный заказ №{OrderId} нельзя отменить.");
        }

        Status = OrderStatus.Cancelled;
    }
}

