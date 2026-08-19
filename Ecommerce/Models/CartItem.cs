using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models;

public class CartItem
{
    [Key]
    public int CartItemId { get; set; }

    [Required]
    public int CartId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    // Navigation properties
    public Cart Cart { get; set; } = null!;

    public Product Product { get; set; } = null!;
}