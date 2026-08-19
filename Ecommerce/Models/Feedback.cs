using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models;

public class Feedback : AuditableEntity
{
    [Key]
    public int FeedbackId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    [Required]
    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 2)]
    public string Comment { get; set; } = string.Empty;

    public DateTime FeedbackDate { get; set; } = DateTime.UtcNow;
}