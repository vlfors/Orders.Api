using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Persistence;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var order = modelBuilder.Entity<Order>();
        order.ToTable("orders");
        order.HasKey(x => x.Id);
        order.Property(x => x.CustomerName).IsRequired();
        order.Property(x => x.TotalAmount).HasColumnType("numeric");
        order.HasIndex(x => new { x.Status, x.CreatedAt });
        order.HasMany(x => x.Items).WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        order.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        var item = modelBuilder.Entity<OrderItem>();
        item.ToTable("order_items");
        item.HasKey(x => x.Id);
        item.Property(x => x.ProductName).IsRequired().HasMaxLength(200);
        item.Property(x => x.Price).HasColumnType("numeric");
    }
}
