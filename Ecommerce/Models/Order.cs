using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models;

public class Order : AuditableEntity
{
    [Key]
    public int OrderId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Required]
    public decimal TotalAmount { get; set; }
    
    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public ICollection<OrderDetail> OrderDetails { get; set; }
        = new List<OrderDetail>();


}