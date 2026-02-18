using Domain.DTOs.ReviewDto;
using Domain.Responces;

namespace Infrastructure.Interfaces;

public interface IReviewsService
{
    Task<Responce<string>> AddReview(CreateReviewDto dto, int userId);
    Task<Responce<string>> UpdateReview(UpdateReviewDto dto, int userId);
    Task<Responce<string>> DeleteReview(int reviewId, int userId);
    Task<Responce<List<GetReviewDto>>> GetReviews(int userId);
    Task<Responce<List<GetReviewDto>>> GetAllReviews();
}