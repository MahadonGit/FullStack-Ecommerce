using AutoMapper;
using Ecommerce.Dto.CustomerDto;
using Ecommerce.Infrastructure.Constants;
using ECommerce.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CustomersController(
        AppDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/customers
    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetCustomers(
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

        var query = _context.Customers
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        // Searching
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Name.Contains(search));
        }

        // Sorting
        if (sortBy.ToLower() == "name")
        {
            sortOrder = sortOrder.ToLower();

            if (sortOrder != "asc" && sortOrder != "desc")
            {
                return BadRequest(new
                {
                    message = "Invalid sortOrder. Use 'asc' or 'desc'."
                });
            }
        }
        else if (sortBy.ToLower() == "createddate")
        {
            if (sortOrder.ToLower() == "desc")
            {
                query = query.OrderByDescending(x => x.CreatedDate);
            }
            else
            {
                query = query.OrderBy(x => x.CreatedDate);
            }
        }
        else
        {
            return BadRequest(new
            {
                message = "Invalid sortBy. Use 'name' or 'createdDate'."
            });
        }

        var totalRecords = await query.CountAsync();

        var customers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = _mapper.Map<List<CustomerResponseDto>>(customers);

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



    //GET: api/customers/{id}
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomer(int id)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found."
            });
        }

        var response = _mapper.Map<CustomerResponseDto>(customer);

        return Ok(response);
    }


    //update customer
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(
    int id,
    CustomerUpdateDto dto)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found."
            });
        }

        _mapper.Map(dto, customer);

        customer.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var response = _mapper.Map<CustomerResponseDto>(customer);

        return Ok(response);
    }

    //Soft delete customer
    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found."
            });
        }

        customer.IsDeleted = true;
        customer.DeletedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Customer deleted successfully."
        });
    }


}