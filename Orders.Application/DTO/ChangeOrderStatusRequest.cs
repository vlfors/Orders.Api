using System.Text.Json.Serialization;
using Orders.Domain.Enums;

namespace Orders.Application.DTO;

public record ChangeOrderStatusRequest(
    [property: JsonRequired] OrderStatus Status);
