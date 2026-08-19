namespace Ecommerce.Dto.CategoryDto;

public class CategoryResponseDto
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }
}