using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Dtos.Auth
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
