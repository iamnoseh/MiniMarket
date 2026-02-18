
using Domain.DTOs.EmailDto;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SendEmailController(IEmailService service): ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendEmail(SendEmail dto)
    {
        await service.SendEmail(dto);
        return NoContent();
    }
}