using System;
using System.Collections.Generic;
using System.Text;


namespace Orders.Application.DTO;

public sealed record CreateOrderRequest
{
    public Guid CustomerId { get; init; }
    public required string CustomerName { get; init; }

    public required IReadOnlyCollection<CreateOrderItemRequest> Items { get; init; }
}
