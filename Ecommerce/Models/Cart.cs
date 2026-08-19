using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models;

public class Cart
{
    [Key]
    public int CartId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    // Navigation property
    public Customer Customer { get; set; } = null!;

    // Products inside this cart
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}