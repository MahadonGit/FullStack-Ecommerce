using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Dto.CartDto;

public class UpdateCartItemDto
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}