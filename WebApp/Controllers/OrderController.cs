using Domain.DTOs.OrderDto;
using Domain.Filters;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Extensions;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController(IOrderService service): ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> CreateOrder(CreateOrderDto create)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var res =  await service.CreateOrder(create, userId);
        return this.ToActionResult(res);
    }
    
    [HttpPut]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> UpdateOrder(UpdateOrderDto dto)
    {
        var res = await service.UpdateStatusOrder(dto);
        return this.ToActionResult(res);
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetOrders([FromQuery]OrderFilter filter)
    {
        var res =  await service.GetOrders(filter);
        return this.ToActionResult(res);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetOrdersByUserId(int userId)
    {
        var res = await service.GetOrdersByUserId(userId);
        return this.ToActionResult(res);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var res = await service.GetOrderById(id);
        return this.ToActionResult(res);
    }

}