namespace Orders.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }

    private OrderItem() { }

    public OrderItem(string productName, int quantity, decimal price)
    {
        if (string.IsNullOrWhiteSpace(productName) || productName.Length > 200)
            throw new ArgumentException("ProductName must contain 1 to 200 non-blank characters.");
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.");
        Id = Guid.NewGuid();
        ProductName = productName;
        Quantity = quantity;
        Price = price;
    }

    internal void SetOrderId(Guid orderId) => OrderId = orderId;
}
