using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Orders.Api.ExceptionHandling;
using Orders.Application.Interfaces;
using Orders.Application.Services;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddDbContext<OrdersDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("Orders")
    ?? throw new InvalidOperationException("ConnectionStrings:Orders is required.")));

var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
    app.MapOpenApi();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
