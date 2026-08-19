using AutoMapper;
using ECommerce.Data;
using Ecommerce.Dto.OrderDto;
using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public OrdersController(
    AppDbContext context,
    IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        // Get logged-in user's Identity ID
        var applicationUserId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (applicationUserId == null)
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }

        // Find Customer
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x =>
                x.ApplicationUserId == applicationUserId &&
                !x.IsDeleted);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found."
            });
        }

        // Find customer's cart with its products
        var cart = await _context.Carts
            .Include(x => x.CartItems)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x =>
                x.CustomerId == customer.Id);

        if (cart == null || !cart.CartItems.Any())
        {
            return BadRequest(new
            {
                message = "Cart is empty."
            });
        }

        // Check all products before creating the order
        foreach (var cartItem in cart.CartItems)
        {
            if (cartItem.Product.IsDeleted)
            {
                return BadRequest(new
                {
                    message = $"Product '{cartItem.Product.Name}' is no longer available."
                });
            }

            if (cartItem.Quantity > cartItem.Product.StockQuantity)
            {
                return BadRequest(new
                {
                    message = $"Insufficient stock for product '{cartItem.Product.Name}'."
                });
            }
        }

        // Create Order
        var order = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 0
        };

        _context.Orders.Add(order);

        // Create OrderDetails
        foreach (var cartItem in cart.CartItems)
        {
            var totalPrice =
                cartItem.Product.Price * cartItem.Quantity;

            var orderDetail = new OrderDetail
            {
                Order = order,
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                UnitPrice = cartItem.Product.Price,
                TotalPrice = totalPrice
            };

            order.OrderDetails.Add(orderDetail);

            // Reduce product stock
            cartItem.Product.StockQuantity -= cartItem.Quantity;

            // Add to order total
            order.TotalAmount += totalPrice;
        }

        // Remove all items from cart
        _context.CartItems.RemoveRange(cart.CartItems);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Order created successfully.",
            orderId = order.OrderId,
            totalAmount = order.TotalAmount
        });

    }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            // Get logged-in user's Identity ID
            var applicationUserId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (applicationUserId == null)
            {
                return Unauthorized(new
                {
                    message = "User is not authenticated."
                });
            }

            // Find Customer
            var customer = await _context.Customers
                .FirstOrDefaultAsync(x =>
                    x.ApplicationUserId == applicationUserId &&
                    !x.IsDeleted);

            if (customer == null)
            {
                return NotFound(new
                {
                    message = "Customer not found."
                });
            }

            // Find the order belonging to this customer
            var order = await _context.Orders
                .Include(x => x.OrderDetails)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x =>
                    x.OrderId == orderId &&
                    x.CustomerId == customer.Id);

            if (order == null)
            {
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            // Map entity to DTO
            var response = _mapper.Map<OrderResponseDto>(order);

            return Ok(response);
        }

    [HttpGet]
    public async Task<IActionResult> GetOrders(
    int page = 1,
    int pageSize = 10,
    string? search = null,
    string sortBy = "orderDate",
    string sortOrder = "desc")
    {
        // Validate pagination
        if (page < 1)
        {
            return BadRequest(new
            {
                message = "Page must be greater than 0."
            });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                message = "PageSize must be between 1 and 100."
            });
        }

        // Get logged-in user's Identity ID
        var applicationUserId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (applicationUserId == null)
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }

        // Find Customer
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x =>
                x.ApplicationUserId == applicationUserId &&
                !x.IsDeleted);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found."
            });
        }

        // Start query
        var query = _context.Orders
            .Include(x => x.OrderDetails)
                .ThenInclude(x => x.Product)
            .Where(x => x.CustomerId == customer.Id)
            .AsQueryable();

        // Searching
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.OrderDetails.Any(d =>
                    d.Product.Name.Contains(search)));
        }

        // Sorting
        query = sortBy.ToLower() switch
        {
            "orderid" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(x => x.OrderId)
                : query.OrderByDescending(x => x.OrderId),

            "orderdate" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(x => x.OrderDate)
                : query.OrderByDescending(x => x.OrderDate),

            "totalamount" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(x => x.TotalAmount)
                : query.OrderByDescending(x => x.TotalAmount),

            _ => query.OrderByDescending(x => x.OrderDate)
        };

        // Total records before pagination
        var totalRecords = await query.CountAsync();

        // Pagination
        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Map to DTOs
        var response = _mapper.Map<List<OrderResponseDto>>(orders);

        return Ok(new
        {
            totalRecords,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize),
            data = response
        });
    }
    [HttpPut("{orderId}/cancel")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        // Get logged-in user's Identity ID
        var applicationUserId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (applicationUserId == null)
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }

        // Find Customer
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x =>
                x.ApplicationUserId == applicationUserId &&
                !x.IsDeleted);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found."
            });
        }

        // Find customer's order with its details and products
        var order = await _context.Orders
            .Include(x => x.OrderDetails)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x =>
                x.OrderId == orderId &&
                x.CustomerId == customer.Id);

        if (order == null)
        {
            return NotFound(new
            {
                message = "Order not found."
            });
        }

        // Check order status
        if (order.Status == OrderStatus.Cancelled)
        {
            return BadRequest(new
            {
                message = "Order is already cancelled."
            });
        }

        if (order.Status == OrderStatus.Completed)
        {
            return BadRequest(new
            {
                message = "Completed orders cannot be cancelled."
            });
        }

        // Restore product stock
        foreach (var orderDetail in order.OrderDetails)
        {
            orderDetail.Product.StockQuantity += orderDetail.Quantity;
        }

        // Change order status
        order.Status = OrderStatus.Cancelled;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Order cancelled successfully."
        });
    }
}
