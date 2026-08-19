using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Dtos.Notification
{
    public class UpdateNotificationDto
    {
        [Required]
        public bool IsRead { get; set; }    
    }
}
