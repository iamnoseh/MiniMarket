using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.BrandDto;

public class CreateBrandDto
{
    [Required]
    public required string Name { get; set; }
}