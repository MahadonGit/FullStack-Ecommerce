
using Microsoft.AspNetCore.Identity;
namespace Ecommerce.Models
{
    public class ApplicationUser  : IdentityUser
    {

        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Notification> Notifications
        { get; set; } = new List<Notification>();

    }
}
