namespace Domain.DTOs.ReviewDto;

public class GetReviewDto:UpdateReviewDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}