using System.Net;
using Domain.DTOs.ReviewDto;
using Domain.Entities;
using Domain.Enums;
using Domain.Responces;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Mapping;
using Serilog;

namespace Infrastructure.Services;

public class ReviewsService(DataContext context) : IReviewsService
{
public async Task<Responce<string>> AddReview(CreateReviewDto dto, int userId)
{
    try
    {
        Log.Information("Start adding review for user {UserId}", userId);
        var isBuy = await context.Orders
            .AnyAsync(o => o.UserId == userId
                        && o.Status == Status.Delivered
                        && o.OrderItems.Any(oi => oi.ProductId == dto.ProductId));
        if (!isBuy)
        {
            return new Responce<string>(HttpStatusCode.BadRequest,
                "Шумо ин маҳсулотро нахаридаед, бинобар ин наметавонед шарҳ гузоред!");
        }

        var existingReview = await context.Reviews
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == dto.ProductId);

        if (existingReview != null)
        {
            return new Responce<string>(HttpStatusCode.BadRequest,
                "Шумо аллакай ба ин маҳсулот шарҳ гузоштаед!");
        }

        var review = new Review()
        {
            UserId = userId,
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

        Log.Information("Review successfully added for user {UserId}", userId);

        return new Responce<string>(HttpStatusCode.OK, "Review added successfully");
    }
    catch (Exception e)
    {
        Log.Error(e, "Error in AddReview");
        return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
    }
}


    public async Task<Responce<string>> UpdateReview(UpdateReviewDto dto, int userId)
    {
        try
        {
            Log.Information("Updating review for user {UserId}", userId);
            var  review = await context.Reviews.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == dto.ProductId);
            if (review == null) return new Responce<string>(HttpStatusCode.NotFound,"Review not found");
            review.Comment = dto.Comment;
            review.Rating = dto.Rating;
            review.UpdatedAt = DateTime.UtcNow;
            var product = await context.Products.FirstOrDefaultAsync(p => p.Id == review.ProductId);
            if (product == null)
                return new Responce<string>(HttpStatusCode.NotFound, "Product not found");
            product.RatingCount = await context.Reviews.CountAsync(r => r.ProductId == product.Id);
            product.AverageRating = await context.Reviews
                .Where(r => r.ProductId == product.Id)
                .AverageAsync(r => r.Rating);
            var res = await context.SaveChangesAsync();
            if (res > 0)
            {
                Log.Information("Review updated for user {UserId}", userId);
            }
            else
            {
                Log.Warning("Review update fail or no changes for user {UserId}", userId);
            }
            return res > 0
                ? new Responce<string>(HttpStatusCode.OK,"Review updated")
                : new Responce<string>(HttpStatusCode.BadRequest,"Review not updated");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in UpdateReview");
            return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<string>> DeleteReview(int reviewId, int userId)
    {
        try
        {
            Log.Information("Deleting review {ReviewId} for user {UserId}", reviewId, userId);
            var review = await context.Reviews.FirstOrDefaultAsync(x => x.Id ==  reviewId && !x.IsDeleted);
            if (review == null) return new Responce<string>(HttpStatusCode.NotFound, "Review not found");
            
            if (review.UserId != userId)
            {
                return new Responce<string>(HttpStatusCode.Forbidden, "You cannot delete other users' reviews");
            }

            review.IsDeleted = true;
            review.UpdatedAt = DateTime.UtcNow;

            var product = await context.Products.FirstOrDefaultAsync(p => p.Id == review.ProductId);
            if (product == null)
                return new Responce<string>(HttpStatusCode.NotFound, "Product not found");
            
            product.RatingCount = await context.Reviews.CountAsync(r => r.ProductId == product.Id && !r.IsDeleted);
            product.AverageRating = await context.Reviews
                .Where(r => r.ProductId == product.Id && !r.IsDeleted)
                .AverageAsync(r => r.Rating);
            
            var res = await context.SaveChangesAsync();
            if (res > 0)
            {
                Log.Information("Review {ReviewId} soft-deleted for user {UserId}", reviewId, userId);
            }
            else
            {
                Log.Warning("Review delete fail for user {UserId}", userId);
            }
            return res > 0
                ? new Responce<string>(HttpStatusCode.OK,"Review deleted")
                : new Responce<string>(HttpStatusCode.NotFound,"Review not deleted");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in DeleteReview");
            return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<List<GetReviewDto>>> GetReviews(int userId)
    {
        try
        {
            Log.Information("Getting reviews for user {UserId}", userId);
            var reviews = await context.Reviews
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .ToListAsync();
            
            if(reviews.Count == 0) return new Responce<List<GetReviewDto>>(HttpStatusCode.NotFound, "Reviews not found");
            
            var dtos = reviews.Select(x => x.ToDto()).ToList();
            return new Responce<List<GetReviewDto>>(dtos);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in GetReviews");
            return new Responce<List<GetReviewDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<List<GetReviewDto>>> GetAllReviews()
    {
        try
        {
            Log.Information("Getting all reviews");
            var reviews = await context.Reviews
                .Where(x => !x.IsDeleted)
                .ToListAsync();
            
            if(reviews.Count == 0) return new Responce<List<GetReviewDto>>(HttpStatusCode.NotFound, "Reviews not found");
            
            var dtos = reviews.Select(x => x.ToDto()).ToList();
            return new Responce<List<GetReviewDto>>(dtos);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in GetAllReviews");
            return new Responce<List<GetReviewDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }
}
