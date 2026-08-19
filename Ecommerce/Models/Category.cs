using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models;

public class Category : AuditableEntity
{
    [Key]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}