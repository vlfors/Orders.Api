# Orders API

ASP.NET Core 10, EF Core, PostgreSQL. Слои: Domain → Application → Infrastructure; API настраивает DI.

## Запуск

Требуются .NET SDK 10 и работающий PostgreSQL. Выполняйте команды из корня решения.

1. Задайте подключение (пример PowerShell; замените значения своими):

   ```powershell
   $env:ConnectionStrings__Orders = "Host=localhost;Port=5432;Database=orders;Username=postgres;Password=YOUR_PASSWORD"
   ```

2. Восстановите зависимости и примените миграцию:

   ```powershell
   dotnet restore Orders.Api.slnx
   dotnet tool restore
   dotnet ef database update --project Orders.Infrastructure --startup-project Orders.Api
   dotnet run --project Orders.Api
   ```

Миграции не применяются автоматически при старте. Пароль не хранится в appsettings.json.
SQL начальной миграции находится в Orders.Infrastructure/Migrations/InitialCreate.sql.

## API

- POST /orders — 201 Created, тело с id и заголовок Location.
- GET /orders/{id} — 200, заказ с позициями; 404 при отсутствии.
- GET /orders?status=New&dateFrom=2026-09-01T00:00:00Z&dateTo=2026-09-30T23:59:59Z — 200, список.
- PUT /orders/{id}/status — 204; тело: {"status":"Paid"}.
- DELETE /orders/{id} — 204; позиции удаляются каскадно.

Пример создания:

```json
{
  "customerName": "Иван",
  "items": [
    { "productName": "Клавиатура", "quantity": 2, "price": 1500.50 }
  ]
}
```

Статусы в JSON передаются строками: New, Paid, Shipped, Completed.
Разрешены только последовательные переходы; повтор текущего статуса запрещён.
Начальный статус — New, сумма вычисляется сервером.
Фильтры комбинируются через AND; границы дат включительные.
Даты передаются в ISO 8601 с часовым поясом и нормализуются в UTC.
dateTo — точный момент времени, а не автоматически конец дня.
Цена хранится как PostgreSQL numeric без принудительного округления.

Ошибки возвращаются в формате ProblemDetails:
400 — некорректные данные/фильтры; 404 — заказ отсутствует;
409 — запрещённый переход статуса; 500 — непредвиденная ошибка без внутренних подробностей.

## Проверка

```powershell
dotnet build Orders.Api.slnx
dotnet test Orders.Api.slnx
```

Unit-тесты проверяют сумму, начальные значения, все 16 комбинаций переходов,
валидацию, создание/чтение через сервис, отсутствие заказа, удаление и сохранение.
Unit-тесты не требуют PostgreSQL; сквозная проверка хранения требует запущенной БД.
