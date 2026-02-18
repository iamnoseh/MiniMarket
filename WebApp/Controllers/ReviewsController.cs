using Domain.DTOs.ReviewDto;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Extensions;

namespace WebApp.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ReviewsController(IReviewsService service): ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> CreateReview(CreateReviewDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var res = await service.AddReview(dto, userId);
        return this.ToActionResult(res);
    }

    [HttpPut]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> UpdateReview(UpdateReviewDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var res = await service.UpdateReview(dto, userId);
        return this.ToActionResult(res);
    }

    [HttpDelete("{reviewId}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var res = await service.DeleteReview(reviewId, userId);
        return this.ToActionResult(res);
    }
    
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var res = await service.GetReviews(userId);
        return this.ToActionResult(res);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetAllReviews()
    {
        var res = await service.GetAllReviews();
        return this.ToActionResult(res);
    }
}