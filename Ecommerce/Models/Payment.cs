using Ecommerce.Enums;

namespace Ecommerce.Models
{
    public class Payment : AuditableEntity
    {

        public int Id { get; set; }

        // Will link to Order later
        public int OrderId { get; set; }

        public Order Order { get; set; } = null!;

        public decimal Amount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public string? TransactionId { get; set; }

        public DateTime PaymentDate { get; set; }

        public string? Remarks { get; set; }

       
    }
}
