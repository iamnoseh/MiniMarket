using Domain.DTOs.BrandDto;
using Domain.Entities;

namespace Infrastructure.Mapping;

public static class BrandMapping
{
    public static GetBrandDto ToDto(this Brand brand)
    {
        return new GetBrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            CreatedAt = brand.CreatedAt,
            UpdatedAt = brand.UpdatedAt
        };
    }
}
