using System.ComponentModel.DataAnnotations;
namespace Ecommerce.Dto.CustomerDto
{
    public class CustomerUpdateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }
    }
}
