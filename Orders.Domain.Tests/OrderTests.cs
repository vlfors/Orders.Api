using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Domain.Exceptions;

namespace Orders.Domain.Tests;

public class OrderTests
{
    [Fact]
    public void Creation_CalculatesTotalAndSetsDefaults()
    {
        var before = DateTimeOffset.UtcNow;
        var items = new List<OrderItem> { new("A", 2, 12.50m), new("B", 3, 4.25m) };
        var order = new Order("Customer", items);
        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal("Customer", order.CustomerName);
        Assert.Equal(37.75m, order.TotalAmount);
        Assert.Equal(OrderStatus.New, order.Status);
        Assert.InRange(order.CreatedAt, before, DateTimeOffset.UtcNow);
        Assert.All(order.Items, item => Assert.Equal(order.Id, item.OrderId));
        items.Clear();
        Assert.Equal(2, order.Items.Count);
    }

    public static IEnumerable<object[]> Transitions()
    {
        foreach (var from in Enum.GetValues<OrderStatus>())
        foreach (var to in Enum.GetValues<OrderStatus>())
            yield return new object[] { from, to };
    }

    [Theory]
    [MemberData(nameof(Transitions))]
    public void ChangeStatus_OnlyAllowsNextStep(OrderStatus from, OrderStatus to)
    {
        var order = new Order("Customer", new() { new("A", 1, 10m) });
        for (var step = 1; step <= (int)from; step++)
            order.ChangeStatus((OrderStatus)step);

        if ((int)to == (int)from + 1)
        {
            order.ChangeStatus(to);
            Assert.Equal(to, order.Status);
        }
        else
        {
            Assert.Throws<DomainRuleException>(() => order.ChangeStatus(to));
            Assert.Equal(from, order.Status);
        }
    }

    [Theory]
    [InlineData("", 1, 1)]
    [InlineData(" ", 1, 1)]
    [InlineData("A", 0, 1)]
    [InlineData("A", -1, 1)]
    [InlineData("A", 1, -1)]
    public void InvalidItem_IsRejected(string name, int quantity, decimal price)
        => Assert.Throws<ArgumentException>(() => new OrderItem(name, quantity, price));

    [Fact]
    public void InvalidOrders_AreRejected()
    {
        Assert.Throws<ArgumentException>(() => new Order(" ", new() { new("A", 1, 0m) }));
        Assert.Throws<ArgumentException>(() => new Order("Customer", new()));
        var item = new OrderItem("A", 1, 1m);
        Assert.Throws<ArgumentException>(() => new Order("Customer", new() { item, item }));
        _ = new Order("First", new() { item });
        Assert.Throws<ArgumentException>(() => new Order("Second", new() { item }));
    }
}
