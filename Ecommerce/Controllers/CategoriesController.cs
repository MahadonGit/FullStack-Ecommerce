using AutoMapper;

using ECommerce.Data;
using Ecommerce.Dto.CategoryDto;
using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CategoriesController(
        AppDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories(
    int page = 1,
    int pageSize = 10,
    string? search = null,
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
        var query = _context.Categories
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        // Searching
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Name.Contains(search) ||
                (x.Description != null &&
                 x.Description.Contains(search)));
        }

        // Sorting
        sortBy = sortBy.ToLower();

        if (sortBy == "name")
        {
            query = sortOrder == "desc"
                ? query.OrderByDescending(x => x.Name)
                : query.OrderBy(x => x.Name);
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
                message = "Invalid sortBy. Use 'name' or 'createdDate'."
            });
        }

        // Count records after search, before pagination
        var totalRecords = await query.CountAsync();

        // Pagination
        var categories = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Entity → DTO
        var response = _mapper.Map<List<CategoryResponseDto>>(categories);

        return Ok(new
        {
            page,
            pageSize,
            search,
            sortBy,
            sortOrder,
            totalRecords,
            totalPages = (int)Math.Ceiling(
                (double)totalRecords / pageSize),
            data = response
        });
    }



    [HttpPost]
    public async Task<IActionResult> CreateCategory(
        CategoryCreateDto dto)
    {
        // Check if category with same name already exists
        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(x =>
                x.Name.ToLower() == dto.Name.ToLower() &&
                !x.IsDeleted);

        if (existingCategory != null)
        {
            return Conflict(new
            {
                message = "Category with this name already exists."
            });
        }

        // DTO → Entity
        var category = _mapper.Map<Category>(dto);

        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        // Entity → Response DTO
        var response = _mapper.Map<CategoryResponseDto>(category);

        return CreatedAtAction(
            nameof(GetCategory),
            new { id = category.CategoryId },
            response);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(x =>
                x.CategoryId == id &&
                !x.IsDeleted);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category not found."
            });
        }

        var response = _mapper.Map<CategoryResponseDto>(category);

        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(
    int id,
    CategoryUpdateDto dto)
    {
        // Find the category
        var category = await _context.Categories
            .FirstOrDefaultAsync(x =>
                x.CategoryId == id &&
                !x.IsDeleted);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category not found."
            });
        }

        // Check for duplicate category name
        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(x =>
                x.CategoryId != id &&
                x.Name.ToLower() == dto.Name.ToLower() &&
                !x.IsDeleted);

        if (existingCategory != null)
        {
            return Conflict(new
            {
                message = "Category with this name already exists."
            });
        }

        // Update existing category using AutoMapper
        _mapper.Map(dto, category);

        await _context.SaveChangesAsync();

        // Entity → Response DTO
        var response = _mapper.Map<CategoryResponseDto>(category);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(x =>
                x.CategoryId == id &&
                !x.IsDeleted);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category not found."
            });
        }

        category.IsDeleted = true;
        category.DeletedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Category deleted successfully."
        });
    } 
}