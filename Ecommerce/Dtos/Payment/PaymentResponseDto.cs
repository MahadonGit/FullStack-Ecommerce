using Ecommerce.Enums;

namespace Ecommerce.Dtos.Payment
{
    public class PaymentResponseDto
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public decimal Amount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public string? TransactionId { get; set; }

        public DateTime PaymentDate { get; set; }

        public string? Remarks { get; set; }
    }
}
