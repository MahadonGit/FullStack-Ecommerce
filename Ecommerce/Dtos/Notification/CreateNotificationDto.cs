using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Dtos.Notification
{
    public class CreateNotificationDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Type { get; set; } = "Email";
    }
}
