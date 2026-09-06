using Microsoft.EntityFrameworkCore;
using Orders.Application.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Repositories;

public sealed class OrderRepository(OrdersDbContext context) : IOrderRepository
{
    public async Task AddAsync(Order order, CancellationToken cancellationToken)
        => await context.Orders.AddAsync(order, cancellationToken);

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => context.Orders.Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<List<Order>> GetAsync(OrderStatus? status, DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo, CancellationToken cancellationToken)
    {
        var query = context.Orders.AsNoTracking().Include(x => x.Items).AsQueryable();
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        if (dateFrom.HasValue)
        {
            var from = dateFrom.Value.ToUniversalTime();
            query = query.Where(x => x.CreatedAt >= from);
        }
        if (dateTo.HasValue)
        {
            var to = dateTo.Value.ToUniversalTime();
            query = query.Where(x => x.CreatedAt <= to);
        }
        return query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    }

    public Task DeleteAsync(Order order, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        context.Orders.Remove(order);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
        => await context.SaveChangesAsync(cancellationToken);
}
