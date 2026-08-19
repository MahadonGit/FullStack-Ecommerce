
namespace Ecommerce.Dto.CartDto;

public class CartResponseDto
{
    public int CartId { get; set; }

    public int CustomerId { get; set; }

    public int TotalItems { get; set; }

    public decimal TotalAmount { get; set; }

    public List<CartItemResponseDto> Items { get; set; } = new();
}