using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models;

public class Address : AuditableEntity
{
    [Key]
    public int AddressId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    [StringLength(150, MinimumLength = 5)]
    public string AddressLine { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string State { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    // Navigation property
    public Customer Customer { get; set; } = null!;
}