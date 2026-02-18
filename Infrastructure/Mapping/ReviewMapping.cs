using Domain.DTOs.ReviewDto;
using Domain.Entities;

namespace Infrastructure.Mapping;

public static class ReviewMapping
{
    public static GetReviewDto ToDto(this Review review)
    {
        return new GetReviewDto
        {
            Id = review.Id,
            UserId = review.UserId,
            ProductId = review.ProductId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }
}
