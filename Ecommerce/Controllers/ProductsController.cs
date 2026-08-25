using AutoMapper;
using Ecommerce.Dto.ProductDto;
using Ecommerce.Infrastructure.Constants;
using Ecommerce.Models;
using ECommerce.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ProductsController(
        AppDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> CreateProduct(ProductCreateDto dto)
    {
        // Check if category exists
        var category = await _context.Categories
            .FirstOrDefaultAsync(x =>
                x.CategoryId == dto.CategoryId &&
                !x.IsDeleted);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category not found."
            });
        }

        // Map DTO to Product
        var product = _mapper.Map<Product>(dto);

        // Add product
        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        // Load category for response
        product.Category = category;

        // Map Product to Response DTO
        var response = _mapper.Map<ProductResponseDto>(product);

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.ProductId },
            response);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x =>
                x.ProductId == id &&
                !x.IsDeleted);

        if (product == null)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        var response = _mapper.Map<ProductResponseDto>(product);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetProducts(
    int page = 1,
    int pageSize = 10,
    string? search = null,
    int? categoryId = null,
    string sortBy = "name",
    string sortOrder = "asc")
    {
        // Validate page
        if (page < 1)
        {
            return BadRequest(new
            {
                message = "Page must be greater than or equal to 1."
            });
        }

        // Validate page size
        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                message = "PageSize must be between 1 and 100."
            });
        }

        // Validate sort order
        sortOrder = sortOrder.ToLower();

        if (sortOrder != "asc" && sortOrder != "desc")
        {
            return BadRequest(new
            {
                message = "Invalid sortOrder. Use 'asc' or 'desc'."
            });
        }

        // Start query
        var query = _context.Products
            .Include(x => x.Category)
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Name.Contains(search) ||
                (x.Description != null &&
                 x.Description.Contains(search)));
        }

        // Filter by category
        if (categoryId.HasValue)
        {
            query = query.Where(x =>
                x.CategoryId == categoryId.Value);
        }

        // Sorting
        sortBy = sortBy.ToLower();

        if (sortBy == "name")
        {
            query = sortOrder == "desc"
                ? query.OrderByDescending(x => x.Name)
                : query.OrderBy(x => x.Name);
        }
        else if (sortBy == "price")
        {
            query = sortOrder == "desc"
                ? query.OrderByDescending(x => x.Price)
                : query.OrderBy(x => x.Price);
        }
        else if (sortBy == "stockquantity")
        {
            query = sortOrder == "desc"
                ? query.OrderByDescending(x => x.StockQuantity)
                : query.OrderBy(x => x.StockQuantity);
        }
        else if (sortBy == "createddate")
        {
            query = sortOrder == "desc"
                ? query.OrderByDescending(x => x.CreatedDate)
                : query.OrderBy(x => x.CreatedDate);
        }
        else
        {
            return BadRequest(new
            {
                message = "Invalid sortBy. Use 'name', 'price', 'stockQuantity', or 'createdDate'."
            });
        }

        // Count records before pagination
        var totalRecords = await query.CountAsync();

        // Pagination
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Map to DTO
        var response = _mapper.Map<List<ProductResponseDto>>(products);

        return Ok(new
        {
            page,
            pageSize,
            search,
            categoryId,
            sortBy,
            sortOrder,
            totalRecords,
            totalPages = (int)Math.Ceiling(
                (double)totalRecords / pageSize),
            data = response
        });
    }


    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(
    int id,
    ProductUpdateDto dto)
    {
        // Find existing product
        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.ProductId == id &&
                !x.IsDeleted);

        if (product == null)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        // Check that the new category exists
        var category = await _context.Categories
            .FirstOrDefaultAsync(x =>
                x.CategoryId == dto.CategoryId &&
                !x.IsDeleted);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category not found."
            });
        }

        // Update existing product
        _mapper.Map(dto, product);

        await _context.SaveChangesAsync();

        // Load category for response
        product.Category = category;

        // Map to response DTO
        var response = _mapper.Map<ProductResponseDto>(product);

        return Ok(response);
    }
    //DELETE


    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.ProductId == id &&
                !x.IsDeleted);

        if (product == null)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        // Soft delete
        product.IsDeleted = true;
        product.DeletedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Product deleted successfully."
        });
    }
}