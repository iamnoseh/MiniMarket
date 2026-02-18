using Domain.DTOs.CategoryDto;
using Domain.Entities;

namespace Infrastructure.Mapping;

public static class CategoryMapping
{
    public static GetCategoryDto ToDto(this Category category)
    {
        return new GetCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
        };
    }
}
