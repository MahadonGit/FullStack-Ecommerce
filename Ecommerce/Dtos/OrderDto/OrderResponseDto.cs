using Ecommerce.Dto.OrderDto;
using Ecommerce.Models;

namespace Ecommerce.Dto.OrderDto;

public class OrderResponseDto
{
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; }

    public List<OrderDetailResponseDto> OrderDetails { get; set; }
        = new();
}