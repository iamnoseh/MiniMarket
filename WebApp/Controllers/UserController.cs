using Domain.DTOs.UserDto;
using Domain.Entities;
using Domain.Filters;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Extensions;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService service) : ControllerBase
{
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateUser([FromForm]UpdateUserDto dto)
    {
        var res = await service.UpdateUser(dto);
        return this.ToActionResult(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var res = await service.DeleteUser(id);
        return this.ToActionResult(res);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetUser(int id)
    {
        var res = await service.GetUser(id);
        return this.ToActionResult(res);
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers([FromQuery] UserFilter filter)
    {
        var res  = await service.GetUsers(filter);
        return this.ToActionResult(res);
    }
}