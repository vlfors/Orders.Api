using Orders.Application.DTO;
using Orders.Application.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Microsoft.Extensions.Logging;
namespace Orders.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repository,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            throw new ArgumentException(
                "CustomerName is required.");
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            throw new ArgumentException(
                "Order must contain at least one item.");
        }

        foreach (var item in request.Items)
        {
            if (item is null)
                throw new ArgumentException("Order item cannot be null.");
            if (item.Quantity <= 0)
            {
                throw new ArgumentException(
                    "Quantity must be greater than zero.");
            }

            if (item.Price < 0)
            {
                throw new ArgumentException(
                    "Price cannot be negative.");
            }
        }

        var items = request.Items
            .Select(x => new OrderItem(
                x.ProductName,
                x.Quantity,
                x.Price))
            .ToList();

        var order = new Order(
            request.CustomerName,
            items);

        await _repository.AddAsync(
            order,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Order {OrderId} created for customer {CustomerName}",
            order.Id,
            order.CustomerName);

        return order.Id;
    }

    public async Task<OrderDto> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException(
                $"Order {id} was not found.");
        }

        return Map(order);
    }

    public async Task<IReadOnlyCollection<OrderDto>> GetAsync(
        OrderStatus? status,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        CancellationToken cancellationToken)
    {
        if (status.HasValue && !Enum.IsDefined(status.Value))
            throw new ArgumentException("Unknown order status.");

        if (dateFrom.HasValue &&
            dateTo.HasValue &&
            dateFrom > dateTo)
        {
            throw new ArgumentException(
                "dateFrom cannot be greater than dateTo.");
        }

        var orders = await _repository.GetAsync(
            status,
            dateFrom,
            dateTo,
            cancellationToken);

        return orders
            .Select(Map)
            .ToList();
    }

    public async Task ChangeStatusAsync(
        Guid id,
        ChangeOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException(
                $"Order {id} was not found.");
        }

        var oldStatus = order.Status;

        // Бизнес-правило New -> Paid -> Shipped -> Completed
        // находится внутри Domain сущности Order.
        order.ChangeStatus(request.Status);

        await _repository.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Order {OrderId} status changed from {OldStatus} to {NewStatus}",
            order.Id,
            oldStatus,
            order.Status);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException(
                $"Order {id} was not found.");
        }

        await _repository.DeleteAsync(
            order,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Order {OrderId} deleted",
            id);
    }

    private static OrderDto Map(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CreatedAt = order.CreatedAt,
            Status = order.Status,
            TotalAmount = order.TotalAmount,

            Items = order.Items
                .Select(x => new OrderItemDto
                {
                    Id = x.Id,
                    ProductName = x.ProductName,
                    Quantity = x.Quantity,
                    Price = x.Price
                })
                .ToList()
        };
    }
}
