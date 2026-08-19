using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Dtos.Auth
{
    public class ResendConfirmationDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;   
    }
}
