using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Brand : BaseEntities
{
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }
    [MaxLength(50)]
    public List<Product>? Products { get; set;}
}