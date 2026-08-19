using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models;

public class Product : AuditableEntity
{
    [Key]
    public int ProductId { get; set; }

    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Range(0, 100)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal DiscountPercentage { get; set; }

    [Required]
    public int StockQuantity { get; set; }

    [Required]
    public int CategoryId { get; set; }

    // Navigation property for the category
    public Category Category { get; set; } = null!;

    // Navigation property for the cart items
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    // Navigation property for the order details
    public ICollection<OrderDetail> OrderDetails { get; set; }
    = new List<OrderDetail>();

    // Navigation property for the feedbacks
    public ICollection<Feedback> Feedbacks { get; set; }
    = new List<Feedback>();
}