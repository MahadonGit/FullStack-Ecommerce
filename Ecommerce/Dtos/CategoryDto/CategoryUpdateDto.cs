using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Dto.CategoryDto;

public class CategoryUpdateDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}