using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Dtos.Auth
{
    public class ConfirmEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }
}
