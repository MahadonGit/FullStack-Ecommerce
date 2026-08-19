using System.Security.Claims;
using AutoMapper;
using ECommerce.Data;
using Ecommerce.Dto.AddressDto;
using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public AddressesController(
        AppDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // =========================================================
    // GET: api/Addresses
    // Get all addresses of the currently logged-in customer
    // =========================================================
    [HttpGet]
    public async Task<IActionResult> GetAddresses(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string sortBy = "addressLine",
        string sortOrder = "asc")
    {
        // -----------------------------------------------------
        // 1. Validate page
        // -----------------------------------------------------
        if (page < 1)
        {
            return BadRequest(new
            {
                message = "Page must be greater than or equal to 1."
            });
        }

        // -----------------------------------------------------
        // 2. Validate page size
        // -----------------------------------------------------
        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                message = "PageSize must be between 1 and 100."
            });
        }

        // -----------------------------------------------------
        // 3. Validate sort order
        // -----------------------------------------------------
        sortOrder = sortOrder.ToLower();

        if (sortOrder != "asc" && sortOrder != "desc")
        {
            return BadRequest(new
            {
                message = "Invalid sortOrder. Use 'asc' or 'desc'."
            });
        }

        // -----------------------------------------------------
        // 4. Get logged-in Identity user's ID
        // -----------------------------------------------------
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }

        // -----------------------------------------------------
        // 5. Find Customer belonging to logged-in user
        // -----------------------------------------------------
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x =>
                x.ApplicationUserId == userId &&
                !x.IsDeleted);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found."
            });
        }

        // -----------------------------------------------------
        // 6. Get customer's non-deleted addresses
        // -----------------------------------------------------
        var query = _context.Addresses
            .Where(x =>
                x.CustomerId == customer.Id &&
                !x.IsDeleted)
            .AsQueryable();

        // -----------------------------------------------------
        // 7. Searching
        // -----------------------------------------------------
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.AddressLine.Contains(search) ||
                x.City.Contains(search) ||
                x.State.Contains(search) ||
                x.Country.Contains(search) ||
                x.PostalCode.Contains(search));
        }

        // -----------------------------------------------------
        // 8. Sorting
        // -----------------------------------------------------
        sortBy = sortBy.ToLower();

        if (sortBy == "addressline")
        {
            query = sortOrder == "desc"
                ? query.OrderByDescending(x => x.AddressLine)
                : query.OrderBy(x => x.AddressLine);
        }
        else if (sortBy == "city")
        {
            query = sortOrder == "desc"
                ? query.OrderByDescending(x => x.City)
                : query.OrderBy(x => x.City);
        }
        else if (sortBy == "state")
        {
            query = sortOrder == "desc"
                ? query.OrderByDescending(x => x.State)
                : query.OrderBy(x => x.State);
        }
        else if (sortBy == "country")
        {
            query = sortOrder == "desc"
                ? query.OrderByDescending(x => x.Country)
                : query.OrderBy(x => x.Country);
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
                message =
                    "Invalid sortBy. Use 'addressLine', 'city', 'state', 'country', or 'createdDate'."
            });
        }

        // -----------------------------------------------------
        // 9. Count records BEFORE pagination
        // -----------------------------------------------------
        var totalRecords = await query.CountAsync();

        // -----------------------------------------------------
        // 10. Pagination
        // -----------------------------------------------------
        var addresses = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // -----------------------------------------------------
        // 11. Map Entity → DTO
        // -----------------------------------------------------
        var response = _mapper.Map<List<AddressResponseDto>>(addresses);

        // -----------------------------------------------------
        // 12. Return response
        // -----------------------------------------------------
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


    // =========================================================
    // POST: api/Addresses
    // Create address for logged-in customer
    // =========================================================
    [HttpPost]
    public async Task<IActionResult> CreateAddress(
        AddressCreateDto dto)
    {
        // -----------------------------------------------------
        // 1. Get logged-in Identity user's ID
        // -----------------------------------------------------
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }

        // -----------------------------------------------------
        // 2. Find Customer belonging to logged-in user
        // -----------------------------------------------------
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x =>
                x.ApplicationUserId == userId &&
                !x.IsDeleted);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found."
            });
        }

        // -----------------------------------------------------
        // 3. Map DTO → Address
        // -----------------------------------------------------
        var address = _mapper.Map<Address>(dto);

        // -----------------------------------------------------
        // 4. Connect Address to Customer
        //
        // IMPORTANT:
        // Customer.Id → Address.CustomerId
        //
        // NOT:
        // Customer.Id → Address.AddressId
        // -----------------------------------------------------
        address.CustomerId = customer.Id;

        // -----------------------------------------------------
        // 5. Add Address
        // -----------------------------------------------------
        _context.Addresses.Add(address);

        await _context.SaveChangesAsync();

        // -----------------------------------------------------
        // 6. Map Address → Response DTO
        // -----------------------------------------------------
        var response = _mapper.Map<AddressResponseDto>(address);

        // -----------------------------------------------------
        // 7. Return 201 Created
        // -----------------------------------------------------
        return CreatedAtAction(
            nameof(GetAddress),
            new { id = address.AddressId },
            response);
    }


    // =========================================================
    // GET: api/Addresses/{id}
    // Get one address belonging to logged-in customer
    // =========================================================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAddress(int id)
    {
        // -----------------------------------------------------
        // 1. Get logged-in Identity user's ID
        // -----------------------------------------------------
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }

        // -----------------------------------------------------
        // 2. Find Customer
        // -----------------------------------------------------
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x =>
                x.ApplicationUserId == userId &&
                !x.IsDeleted);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found."
            });
        }

        // -----------------------------------------------------
        // 3. Find Address
        //
        // AddressId = requested address
        // CustomerId = logged-in customer
        // IsDeleted = false
        // -----------------------------------------------------
        var address = await _context.Addresses
            .FirstOrDefaultAsync(x =>
                x.AddressId == id &&
                x.CustomerId == customer.Id &&
                !x.IsDeleted);

        if (address == null)
        {
            return NotFound(new
            {
                message = "Address not found."
            });
        }

        // -----------------------------------------------------
        // 4. Map Entity → DTO
        // -----------------------------------------------------
        var response = _mapper.Map<AddressResponseDto>(address);

        return Ok(response);
    }


    // =========================================================
    // PUT: api/Addresses/{id}
    // Update address belonging to logged-in customer
    // =========================================================
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAddress(
        int id,
        AddressUpdateDto dto)
    {
        // -----------------------------------------------------
        // 1. Get logged-in Identity user's ID
        // -----------------------------------------------------
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }

        // -----------------------------------------------------
        // 2. Find Customer
        // -----------------------------------------------------
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x =>
                x.ApplicationUserId == userId &&
                !x.IsDeleted);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found."
            });
        }

        // -----------------------------------------------------
        // 3. Find Address
        // -----------------------------------------------------
        var address = await _context.Addresses
            .FirstOrDefaultAsync(x =>
                x.AddressId == id &&
                x.CustomerId == customer.Id &&
                !x.IsDeleted);

        if (address == null)
        {
            return NotFound(new
            {
                message = "Address not found."
            });
        }

        // -----------------------------------------------------
        // 4. Map DTO → existing Address
        // -----------------------------------------------------
        _mapper.Map(dto, address);

        // -----------------------------------------------------
        // 5. Update audit information
        // -----------------------------------------------------
        address.UpdatedDate = DateTime.UtcNow;

        // -----------------------------------------------------
        // 6. Save changes
        // -----------------------------------------------------
        await _context.SaveChangesAsync();

        // -----------------------------------------------------
        // 7. Map Entity → DTO
        // -----------------------------------------------------
        var response = _mapper.Map<AddressResponseDto>(address);

        return Ok(response);
    }


    // =========================================================
    // DELETE: api/Addresses/{id}
    // Soft delete address
    // =========================================================
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        // -----------------------------------------------------
        // 1. Get logged-in Identity user's ID
        // -----------------------------------------------------
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }

        // -----------------------------------------------------
        // 2. Find Customer
        // -----------------------------------------------------
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x =>
                x.ApplicationUserId == userId &&
                !x.IsDeleted);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found."
            });
        }

        // -----------------------------------------------------
        // 3. Find Address belonging to customer
        // -----------------------------------------------------
        var address = await _context.Addresses
            .FirstOrDefaultAsync(x =>
                x.AddressId == id &&
                x.CustomerId == customer.Id &&
                !x.IsDeleted);

        if (address == null)
        {
            return NotFound(new
            {
                message = "Address not found."
            });
        }

        // -----------------------------------------------------
        // 4. Soft Delete
        // -----------------------------------------------------
        address.IsDeleted = true;
        address.DeletedDate = DateTime.UtcNow;
        address.DeletedBy = userId;

        // -----------------------------------------------------
        // 5. Save changes
        // -----------------------------------------------------
        await _context.SaveChangesAsync();

        // -----------------------------------------------------
        // 6. Return 204 No Content
        // -----------------------------------------------------
        return NoContent();
    }
}