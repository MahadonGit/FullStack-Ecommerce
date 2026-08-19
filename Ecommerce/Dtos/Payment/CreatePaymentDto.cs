using Ecommerce.Enums;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Dtos.Payment
{
    public class CreatePaymentDto
    {

        [Required]
        public int OrderId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        public string? Remarks { get; set; }
    }
}
