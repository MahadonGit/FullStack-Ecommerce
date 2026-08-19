using AutoMapper;

using ECommerce.Data;
using Ecommerce.Dto.FeedbackDto;
using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FeedbackController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public FeedbackController(
        AppDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> CreateFeedback(
        CreateFeedbackDto dto)
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

        // Check if customer already reviewed this product
        var existingFeedback = await _context.Feedbacks
            .FirstOrDefaultAsync(x =>
                x.CustomerId == customer.Id &&
                x.ProductId == dto.ProductId &&
                !x.IsDeleted);

        if (existingFeedback != null)
        {
            return BadRequest(new
            {
                message = "You have already submitted feedback for this product."
            });
        }

        // Map DTO to entity
        var feedback = _mapper.Map<Feedback>(dto);

        // Set values controlled by the server
        feedback.CustomerId = customer.Id;
        feedback.FeedbackDate = DateTime.UtcNow;

        _context.Feedbacks.Add(feedback);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Feedback submitted successfully.",
            feedbackId = feedback.FeedbackId
        });

    }
    [HttpGet("product/{productId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductFeedback(
int productId,
int page = 1,
int pageSize = 10,
string? search = null,
string sortBy = "feedbackDate",
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

        // Check if product exists
        var productExists = await _context.Products
            .AnyAsync(x =>
                x.ProductId == productId &&
                !x.IsDeleted);

        if (!productExists)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        // Start query
        var query = _context.Feedbacks
            .Include(x => x.Customer)
            .Include(x => x.Product)
            .Where(x =>
                x.ProductId == productId &&
                !x.IsDeleted)
            .AsQueryable();

        // Searching
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Comment.Contains(search) ||
                x.Customer.Name.Contains(search));
        }

        // Sorting
        query = sortBy.ToLower() switch
        {
            "rating" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(x => x.Rating)
                : query.OrderByDescending(x => x.Rating),

            "customername" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(x => x.Customer.Name)
                : query.OrderByDescending(x => x.Customer.Name),

            "feedbackdate" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(x => x.FeedbackDate)
                : query.OrderByDescending(x => x.FeedbackDate),

            _ => query.OrderByDescending(x => x.FeedbackDate)
        };

        // Total records before pagination
        var totalRecords = await query.CountAsync();

        // Pagination
        var feedback = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Map to DTO
        var response = _mapper.Map<List<FeedbackResponseDto>>(feedback);

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

    [HttpPut("{feedbackId}")]
    public async Task<IActionResult> UpdateFeedback(
    int feedbackId,
    UpdateFeedbackDto dto)
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

        // Find feedback belonging to this customer
        var feedback = await _context.Feedbacks
            .FirstOrDefaultAsync(x =>
                x.FeedbackId == feedbackId &&
                x.CustomerId == customer.Id &&
                !x.IsDeleted);

        if (feedback == null)
        {
            return NotFound(new
            {
                message = "Feedback not found."
            });
        }

        // Map updated values
        _mapper.Map(dto, feedback);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Feedback updated successfully."
        });
    }

        [HttpDelete("{feedbackId}")]
        public async Task<IActionResult> DeleteFeedback(int feedbackId)
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

            // Find feedback belonging to this customer
            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(x =>
                    x.FeedbackId == feedbackId &&
                    x.CustomerId == customer.Id &&
                    !x.IsDeleted);

            if (feedback == null)
            {
                return NotFound(new
                {
                    message = "Feedback not found."
                });
            }

            // Soft delete
            feedback.IsDeleted = true;
            feedback.DeletedDate = DateTime.UtcNow;
            feedback.DeletedBy = applicationUserId;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Feedback deleted successfully."
            });
        }
    }
