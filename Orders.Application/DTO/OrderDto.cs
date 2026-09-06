using Orders.Domain.Enums;

namespace Orders.Application.DTO;

public class OrderDto
{
    public Guid Id { get; set; }

    public string CustomerName { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    public List<OrderItemDto> Items { get; set; } = new();
}