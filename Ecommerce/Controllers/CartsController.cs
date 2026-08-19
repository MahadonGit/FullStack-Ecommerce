using AutoMapper;

using ECommerce.Data;
using Ecommerce.Dto.CartDto;
using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CartsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CartsController(
        AppDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddToCart(AddToCartDto dto)
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

        // Find Customer belonging to logged-in user
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

        // Check Product
        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.ProductId == dto.ProductId &&
                !x.IsDeleted);

        if (product == null)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        // Check stock
        if (dto.Quantity > product.StockQuantity)
        {
            return BadRequest(new
            {
                message = "Requested quantity is greater than available stock."
            });
        }

        // Find customer's cart
        var cart = await _context.Carts
            .FirstOrDefaultAsync(x =>
                x.CustomerId == customer.Id);

        // Create cart if it doesn't exist
        if (cart == null)
        {
            cart = new Cart
            {
                CustomerId = customer.Id
            };

            _context.Carts.Add(cart);

            await _context.SaveChangesAsync();
        }

        // Check if product already exists in cart
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(x =>
                x.CartId == cart.CartId &&
                x.ProductId == dto.ProductId);

        if (cartItem != null)
        {
            // Calculate new quantity
            var newQuantity = cartItem.Quantity + dto.Quantity;

            if (newQuantity > product.StockQuantity)
            {
                return BadRequest(new
                {
                    message = "Total quantity exceeds available stock."
                });
            }

            cartItem.Quantity = newQuantity;
        }
        else
        {
            // Add new product to cart
            cartItem = new CartItem
            {
                CartId = cart.CartId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };

            _context.CartItems.Add(cartItem);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Product added to cart successfully."
        });
    }

        [HttpGet]
        public async Task<IActionResult> GetCart()
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

            // Get customer's cart with products
            var cart = await _context.Carts
                .Include(x => x.CartItems)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x =>
                    x.CustomerId == customer.Id);

            if (cart == null)
            {
                return NotFound(new
                {
                    message = "Cart is empty."
                });
            }

            // Only include products that haven't been soft deleted
            var activeCartItems = cart.CartItems
                .Where(x => !x.Product.IsDeleted)
                .ToList();

            // Map CartItems
            var items = _mapper.Map<List<CartItemResponseDto>>(
                activeCartItems);

            // Calculate total quantity
            var totalItems = activeCartItems.Sum(x => x.Quantity);

            // Calculate total amount
            var totalAmount = activeCartItems.Sum(x =>
                x.Product.Price * x.Quantity);

            var response = new CartResponseDto
            {
                CartId = cart.CartId,
                CustomerId = cart.CustomerId,
                TotalItems = totalItems,
                TotalAmount = totalAmount,
                Items = items
            };

            return Ok(response);
        }

    [HttpPut("items/{cartItemId}")]
    public async Task<IActionResult> UpdateCartItem(
    int cartItemId,
    UpdateCartItemDto dto)
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

        // Find the cart item belonging to this customer
        var cartItem = await _context.CartItems
            .Include(x => x.Cart)
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x =>
                x.CartItemId == cartItemId &&
                x.Cart.CustomerId == customer.Id);

        if (cartItem == null)
        {
            return NotFound(new
            {
                message = "Cart item not found."
            });
        }

        // Check if product is still active
        if (cartItem.Product.IsDeleted)
        {
            return BadRequest(new
            {
                message = "This product is no longer available."
            });
        }

        // Check stock
        if (dto.Quantity > cartItem.Product.StockQuantity)
        {
            return BadRequest(new
            {
                message = "Requested quantity is greater than available stock."
            });
        }

        // Update quantity
        cartItem.Quantity = dto.Quantity;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Cart item quantity updated successfully."
        });
    }



    [HttpDelete("items/{cartItemId}")]
    public async Task<IActionResult> RemoveCartItem(int cartItemId)
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

        // Find cart item belonging to this customer
        var cartItem = await _context.CartItems
            .Include(x => x.Cart)
            .FirstOrDefaultAsync(x =>
                x.CartItemId == cartItemId &&
                x.Cart.CustomerId == customer.Id);

        if (cartItem == null)
        {
            return NotFound(new
            {
                message = "Cart item not found."
            });
        }

        // Remove cart item
        _context.CartItems.Remove(cartItem);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Cart item removed successfully."
        });
    }



    [HttpDelete]
    public async Task<IActionResult> ClearCart()
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

        // Find customer's cart
        var cart = await _context.Carts
            .Include(x => x.CartItems)
            .FirstOrDefaultAsync(x =>
                x.CustomerId == customer.Id);

        if (cart == null)
        {
            return NotFound(new
            {
                message = "Cart not found."
            });
        }

        // Check if cart is already empty
        if (!cart.CartItems.Any())
        {
            return BadRequest(new
            {
                message = "Cart is already empty."
            });
        }

        // Remove all cart items
        _context.CartItems.RemoveRange(cart.CartItems);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Cart cleared successfully."
        });
    }

}
