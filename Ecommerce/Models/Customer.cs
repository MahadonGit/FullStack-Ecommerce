using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models;

public class Customer : AuditableEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;

    public ApplicationUser ApplicationUser { get; set; } = null!;

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    // Navigation property for the Cart
    public Cart Cart { get; set; } = null!;

    // Navigation property for the Orders
    public ICollection<Order> Orders { get; set; }
    = new List<Order>();

    // Navigation property for the Feedbacks
    public ICollection<Feedback> Feedbacks { get; set; }
    = new List<Feedback>();

    // Na
    public ICollection<Address> Addresses { get; set; }
        = new List<Address>();
}