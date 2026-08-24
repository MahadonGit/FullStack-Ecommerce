
using Microsoft.AspNetCore.Identity;
namespace Ecommerce.Models
{
    public class ApplicationUser  : IdentityUser
    {

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

       
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Notification> Notifications
        { get; set; } = new List<Notification>();

    }
}
