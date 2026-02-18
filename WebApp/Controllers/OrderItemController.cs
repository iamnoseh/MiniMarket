using Domain.DTOs.OrderItemDto;
using Domain.Filters;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Extensions;

namespace WebApp.Controllers;


[ApiController]
[Route("api/[controller]")]
public class OrderItemController(IOrderItemService service) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> CreateOrderItem(CreateOrderItemDto dto)
    {
        var res =  await service.CreateOrderItem(dto);
        return this.ToActionResult(res);
    }

    [HttpPut]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> UpdateOrderItem(UpdateOrderItemDto dto)
    {
        var res = await service.UpdateOrderItem(dto);
        return this.ToActionResult(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> DeleteOrderItem(int id)
    {
        var res = await service.DeleteOrderItem(id);
        return this.ToActionResult(res);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetOrderItemDetail(int id)
    {
        var res = await service.GetOrderItemById(id);
        return this.ToActionResult(res);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetOrderItems([FromQuery] OrderItemFilter filter)
    {
        var res = await service.GetOrderItems(filter);
        return this.ToActionResult(res);
    }
}