using Ecommerce.Enums;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Dtos.Payment
{
    public class UpdatePaymentStatusDto
    {
        [Required]
        public PaymentStatus PaymentStatus { get; set; }

        public string? TransactionId { get; set; }

        public string? Remarks { get; set; }
    }
}
