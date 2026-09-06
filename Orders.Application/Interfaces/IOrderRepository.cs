using Orders.Domain.Entities;
using Orders.Domain.Enums;
namespace Orders.Application.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(
        Order order,
        CancellationToken cancellationToken);

    Task<Order?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<List<Order>> GetAsync(
        OrderStatus? status,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        Order order,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}