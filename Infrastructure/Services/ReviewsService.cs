using System.Net;
using Domain.DTOs.ReviewDto;
using Domain.Entities;
using Domain.Enums;
using Domain.Responces;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Infrastructure.Services;

public class ReviewsService(DataContext context) : IReviewsService
{
public async Task<Responce<string>> AddReview(CreateReviewDto dto)
{
    try
    {
        Log.Information("Start adding review");
        var isBuy = await context.Orders
            .AnyAsync(o => o.UserId == dto.UserId
                        && o.Status == Status.Delivered
                        && o.OrderItems.Any(oi => oi.ProductId == dto.ProductId));
        if (!isBuy)
        {
            return new Responce<string>(HttpStatusCode.BadRequest,
                "Шумо ин маҳсулотро нахаридаед, бинобар ин наметавонед шарҳ гузоред!");
        }

        var existingReview = await context.Reviews
            .FirstOrDefaultAsync(x => x.UserId == dto.UserId && x.ProductId == dto.ProductId);

        if (existingReview != null)
        {
            return new Responce<string>(HttpStatusCode.BadRequest,
                "Шумо аллакай ба ин маҳсулот шарҳ гузоштаед!");
        }

        var review = new Review()
        {
            UserId = dto.UserId,
            ProductId = dto.ProductId,
            Comment = dto.Comment,
            Rating = dto.Rating,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.Reviews.AddAsync(review);
        await context.SaveChangesAsync();

        var product = await context.Products.FirstOrDefaultAsync(x => x.Id == dto.ProductId);
        if (product == null)
            return new Responce<string>(HttpStatusCode.NotFound, "Product not found");

        product.RatingCount = await context.Reviews.CountAsync(r => r.ProductId == product.Id);
        product.AverageRating = await context.Reviews
            .Where(r => r.ProductId == product.Id)
            .AverageAsync(r => r.Rating);

        await context.SaveChangesAsync();

        Log.Information("Review successfully added");

        return new Responce<string>(HttpStatusCode.OK, "Review added successfully");
    }
    catch (Exception e)
    {
        Log.Error("Error in AddReview");
        return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
    }
}


    public async Task<Responce<string>> UpdateReview(UpdateReviewDto dto)
    {
        try
        {
            Log.Information("Updating review");
            var  review = await context.Reviews.FirstOrDefaultAsync(x => x.UserId == dto.UserId && x.ProductId == dto.ProductId);
            if (review == null) return new Responce<string>(HttpStatusCode.NotFound,"Review not found");
            review.Comment = dto.Comment;
            review.Rating = dto.Rating;
            review.UpdatedAt = DateTime.UtcNow;
            var res = await context.SaveChangesAsync();
            if (res > 0)
            {
                Log.Information("Updating review");
            }
            else
            {
                Log.Fatal("Updating review fail");
            }
            return res > 0
                ? new Responce<string>(HttpStatusCode.OK,"Review updated")
                : new Responce<string>(HttpStatusCode.BadRequest,"Review not updated");
        }
        catch (Exception e)
        {
            Log.Error("Error in UpdateReview");
            return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<string>> DeleteReview(int reviewId)
    {
        try
        {
            Log.Information("Deleting review");
            var review = await context.Reviews.FirstOrDefaultAsync(x => x.Id ==  reviewId);
            if (review == null) return new Responce<string>(HttpStatusCode.NotFound,"Review not found");
            context.Reviews.Remove(review);
            var res = await context.SaveChangesAsync();
            if (res > 0)
            {
                Log.Information("Deleting review");
            }
            else
            {
                Log.Fatal("Deleting review fail");
            }
            return res > 0
                ? new Responce<string>(HttpStatusCode.OK,"Review deleted")
                : new Responce<string>(HttpStatusCode.NotFound,"Review not deleted");
        }
        catch (Exception e)
        {
            Log.Error("Error in DeleteReview");
            return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<List<GetReviewDto>>> GetReviews(int userId)
    {
        try
        {
            Log.Information("Getting reviews");
            var reviews = await context.Reviews.Where(x => x.UserId == userId).ToListAsync();
            if(reviews.Count == 0) return new Responce<List<GetReviewDto>>(HttpStatusCode.NotFound, "Review not found");
            var dto =  reviews.Select(x=>new GetReviewDto()
            {
                Id = x.Id,
                UserId = x.UserId,
                Comment = x.Comment,
                Rating = x.Rating,
                ProductId = x.ProductId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                
            }).ToList();
            return new Responce<List<GetReviewDto>>(dto);
        }
        catch (Exception e)
        {
            Log.Error("Error in  GetReviews");
            return new Responce<List<GetReviewDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<List<GetReviewDto>>> GetAllReviews()
    {
        try
        {
            Log.Information("Getting all reviews");
            var reviews = await context.Reviews.ToListAsync();
            if(reviews.Count == 0) return new Responce<List<GetReviewDto>>(HttpStatusCode.NotFound, "Reviews not found");
            var dtos = reviews.Select(x=> new GetReviewDto()
            {
                Id = x.Id,
                UserId = x.UserId,
                Comment = x.Comment,
                Rating = x.Rating,
                ProductId = x.ProductId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
            }).ToList();
            return new Responce<List<GetReviewDto>>(dtos);
        }
        catch (Exception e)
        {
            Log.Error("Error in GetAllReviews");
            return new Responce<List<GetReviewDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }
}