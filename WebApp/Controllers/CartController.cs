using Domain.DTOs.CartItemDto;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Extensions;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController(ICartService service):ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> AddCartItem(CreateCartItemDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var res = await service.AddToCart(dto, userId);
        return this.ToActionResult(res);
    }

    [HttpPut]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> UpdateCartItem(UpdateCartItemDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var res = await service.UpdateCart(dto, userId);
        return this.ToActionResult(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> DeleteCart(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var res = await service.DeleteCart(id, userId);
        return this.ToActionResult(res);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetCart()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var res = await service.GetCartItem(userId);
        return this.ToActionResult(res);
    }
}