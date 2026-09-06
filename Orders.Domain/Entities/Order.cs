using Orders.Domain.Enums;
using Orders.Domain.Exceptions;
using System;
using System.Collections.Generic;

using System.Text;

namespace Orders.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }

    public string CustomerName { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public OrderStatus Status { get; private set; }

    public decimal TotalAmount { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order()
    {
    }

    public Order(string customerName, List<OrderItem> items)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("CustomerName is required.");
        if (items is null || items.Count == 0 || items.Any(x => x is null))
            throw new ArgumentException("Order must contain at least one valid item.");
        if (items.Any(x => x.OrderId != Guid.Empty) || items.Select(x => x.Id).Distinct().Count() != items.Count)
            throw new ArgumentException("Items must be unique and must not belong to another order.");
        Id = Guid.NewGuid();
        CustomerName = customerName;
        CreatedAt = DateTimeOffset.UtcNow;
        Status = OrderStatus.New;
        _items.AddRange(items);

        foreach (var item in Items)
        {
            item.SetOrderId(Id);
        }

        TotalAmount = Items.Sum(x => x.Quantity * x.Price);
    }

    public void ChangeStatus(OrderStatus newStatus)
    {
        var allowed = Status switch
        {
            OrderStatus.New => newStatus == OrderStatus.Paid,
            OrderStatus.Paid => newStatus == OrderStatus.Shipped,
            OrderStatus.Shipped => newStatus == OrderStatus.Completed,
            _ => false
        };

        if (!allowed)
        {
            throw new DomainRuleException(
                $"Transition from {Status} to {newStatus} is not allowed.");
        }

        Status = newStatus;
    }
}
