using Microsoft.Extensions.Logging.Abstractions;
using Orders.Application.DTO;
using Orders.Application.Interfaces;
using Orders.Application.Services;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Domain.Exceptions;

namespace Orders.Domain.Tests;

public class OrderServiceTests
{
    private readonly TestRepository _repository = new();
    private OrderService Service => new(_repository, NullLogger<OrderService>.Instance);

    // Созданный заказ доступен для чтения с рассчитанной суммой и сохраняется один раз.
    [Fact]
    public async Task CreateAndRead_ReturnsItemsAndCalculatedTotal()
    {
        var id = await Service.CreateAsync(new CreateOrderRequest
        {
            CustomerName = "Customer",
            Items = new[] { new CreateOrderItemRequest { ProductName = "A", Quantity = 2, Price = 5.5m } }
        }, default);
        var result = await Service.GetByIdAsync(id, default);
        Assert.Equal(id, result.Id);
        Assert.Equal(11m, result.TotalAmount);
        Assert.Equal(OrderStatus.New, result.Status);
        Assert.Equal("A", Assert.Single(result.Items).ProductName);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task InvalidTransition_DoesNotSave()
    {
        // Нельзя пропустить оплату: неудачный переход не меняет заказ и не вызывает сохранение.
        _repository.Order = new("Customer", new() { new("A", 1, 10m) });
        await Assert.ThrowsAsync<DomainRuleException>(() =>
            Service.ChangeStatusAsync(_repository.Order.Id, new(OrderStatus.Completed), default));
        Assert.Equal(0, _repository.SaveCount);
        Assert.Equal(OrderStatus.New, _repository.Order.Status);
        // После отказа допустимый переход по-прежнему выполняется и сохраняется.
        await Service.ChangeStatusAsync(_repository.Order.Id, new(OrderStatus.Paid), default);
        Assert.Equal(OrderStatus.Paid, _repository.Order.Status);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task MissingOrder_ThrowsForReadUpdateAndDelete()
    {
        // Чтение, смена статуса и удаление одинаково сообщают об отсутствии заказа.
        var id = Guid.NewGuid();
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Service.GetByIdAsync(id, default));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Service.ChangeStatusAsync(id, new(OrderStatus.Paid), default));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Service.DeleteAsync(id, default));
    }

    [Fact]
    public async Task Delete_RemovesOrderAndSaves()
    {
        // Удаление убирает заказ из репозитория и сохраняет изменение ровно один раз.
        _repository.Order = new("Customer", new() { new("A", 1, 10m) });
        await Service.DeleteAsync(_repository.Order.Id, default);
        Assert.Null(_repository.Order);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task InvalidFilters_AreRejected()
    {
        // Отклоняем обратный диапазон дат и значение, отсутствующее в перечислении статусов.
        var now = DateTimeOffset.UtcNow;
        await Assert.ThrowsAsync<ArgumentException>(() => Service.GetAsync(null, now, now.AddDays(-1), default));
        await Assert.ThrowsAsync<ArgumentException>(() => Service.GetAsync((OrderStatus)99, null, null, default));
    }

    // Репозиторий в памяти изолирует тесты сервиса от БД и учитывает вызовы сохранения.
    private sealed class TestRepository : IOrderRepository
    {
        public Order? Order { get; set; }
        public int SaveCount { get; private set; }
        public Task AddAsync(Order order, CancellationToken cancellationToken)
        {
            Order = order;
            return Task.CompletedTask;
        }
        public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(Order?.Id == id ? Order : null);
        public Task<List<Order>> GetAsync(OrderStatus? status, DateTimeOffset? from,
            DateTimeOffset? to, CancellationToken cancellationToken)
            => throw new NotSupportedException();
        public Task DeleteAsync(Order order, CancellationToken cancellationToken)
        {
            Order = null;
            return Task.CompletedTask;
        }
        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
