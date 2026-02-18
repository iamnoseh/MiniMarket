using Domain.DTOs.ReviewDto;
using Infrastructure.Interfaces.Reviews___Ratings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Extensions;

namespace WebApp.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ReviewsController(IReviewsRatings service): Controller
{
    [HttpPost]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> CreateReview(CreateReviewDto dto)
    {
        var res = await service.AddReview(dto);
        return this.ToActionResult(res);
    }

    [HttpPut]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> UpdateReview(UpdateReviewDto dto)
    {
        var res = await service.UpdateReview(dto);
        return this.ToActionResult(res);
    }

    [HttpDelete]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        var res = await service.DeleteReview(reviewId);
        return this.ToActionResult(res);
    }
    
    [HttpGet("{my-reviews}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetReviews(int userId)
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