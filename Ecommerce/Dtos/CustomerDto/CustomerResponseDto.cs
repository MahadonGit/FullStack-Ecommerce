using System.ComponentModel.DataAnnotations;
namespace Ecommerce.Dto.CustomerDto
{
    public class CustomerResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ApplicationUserId { get; set; } = string.Empty;
        public string? Phone { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
