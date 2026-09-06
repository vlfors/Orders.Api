using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Application.DTO;

public sealed record CreateOrderItemRequest
{
    public Guid ProductId { get; init; }

    public required string ProductName { get; init; }

    public int Quantity { get; init; }

    public decimal Price { get; init; }
}