using Domain.Entities;

namespace Domain.DTOs.BrandDto;

public class GetBrandDto : UpdateBrandDto
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}