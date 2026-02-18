using Domain.DTOs.OrderItemDto;
using Domain.Filters;
using Domain.Responses;

namespace Infrastructure.Interfaces;

public interface IOrderItemService
{
    Task<Responce<string>> CreateOrderItem(CreateOrderItemDto dto);
    Task<Responce<string>> UpdateOrderItem(UpdateOrderItemDto dto);
    Task<Responce<string>> DeleteOrderItem(int id);
    Task<Responce<GetOrderItemDto>> GetOrderItemById(int id);
    Task<PaginationResponce<List<GetOrderItemDto>>> GetOrderItems(OrderItemFilter filter);
    
}