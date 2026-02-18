using Domain.DTOs.OrderDto;
using Domain.DTOs.OrderItemDto;
using Domain.Entities;

namespace Infrastructure.Mapping;

public static class OrderMapping
{
    public static GetOrderDto ToDto(this Order order)
    {
        return new GetOrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Address = order.Address,
            PaymentMethod = order.PaymentMethod,
            Status = order.Status,
            TotalAmount = order.OrderItems?.Sum(oi => oi.Price) ?? 0,
            OrderItems = order.OrderItems?.Select(oi => oi.ToDto()).ToList(),
            OrderDate = order.OrderDate,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
        };
    }

    public static GetOrderItemDto ToDto(this OrderItem item)
    {
        return new GetOrderItemDto
        {
            Id = item.Id,
            OrderId = item.OrderId,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            Price = item.Price,
        };
    }
}
