using Domain.DTOs.Account;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Extensions;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IAccountService service) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] Register request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var res = await service.Register(request);
        return this.ToActionResult(res);
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody]LoginDto request)
    {
        var res = await service.Login(request);
        return this.ToActionResult(res);
    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePassword changePasswordDto)
    {
        var res = await service.ChangePassword(changePasswordDto);
        return this.ToActionResult(res);
    }
}