using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Application.DTO;

public class OrderItemDto
{
    public Guid Id { get; set; }

    public string ProductName { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal Price { get; set; }
}