using Domain.DTOs.ProductDto;
using Domain.Entities;
using Domain.Filters;
using Domain.Responces;

namespace Infrastructure.Mapping;

public static class ProductMapping
{
    public static GetProductDto ToDto(this Product product)
    {
        return new GetProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Quantity = product.Quantity,
            CategoryId = product.CategoryId,
            BrandName = product.Brand?.Name ?? "n/a",
            AverageRating = product.AverageRating,
            RatingCount = product.RatingCount,
            ImageUrl = product.ImageUrl,
            UpdatedAt = product.UpdatedAt,
            CreatedAt = product.CreatedAt
        };
    }
}
