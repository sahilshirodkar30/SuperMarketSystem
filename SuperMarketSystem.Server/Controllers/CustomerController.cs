using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperMarketSystem.Server.DATA;
using SuperMarketSystem.Server.Models;

namespace SuperMarketSystem.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public CustomerController(ApplicationDBContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Get all categories.
        /// </summary>
        /// <returns>List of categories.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(new { message = "Page number and page size must be greater than 0." });
            }

            var totalRecords = await _context.Categories.CountAsync();
            var customers = await _context.Customers
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginationResponse = new
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                Data = customers
            };

            return Ok(paginationResponse);
        }
        /// <summary>
        /// Get a specific customers by ID.
        /// </summary>
        /// <param name="id">The ID of the customers.</param>
        /// <returns>The requested category.</returns>
        /// <response code="200">Returns the customers.</response>
        /// <response code="404">If the customers is not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomers(int id)
        {
            var customers = await _context.Customers.FindAsync(id);

            if (customers == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            return Ok(customers);
        }

        /// <summary>
        /// Create a new customers.
        /// </summary>
        /// <param name="customers">The customers details.</param>
        /// <returns>The newly created customers.</returns>
        /// <response code="201">customers created successfully.</response>
        /// <response code="400">If the input is invalid.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCategory([FromBody] Categorymodel category)
        {
            Category model = new Category
            {
                Name = category.Name
            };

            _context.Categories.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        /// <summary>
        /// Update an existing category.
        /// </summary>
        /// <param name="id">The ID of the category to update.</param>
        /// <param name="category">The updated category details.</param>
        /// <returns>No content.</returns>
        /// <response code="204">Update successful.</response>
        /// <response code="400">If the ID does not match or input is invalid.</response>
        /// <response code="404">If the category is not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCustomers(int id, [FromBody] Customermodel customers)
        {
            var existingClass = await _context.Customers.FindAsync(id);
            if (existingClass == null) return NotFound();

            existingClass.FirstName = customers.FirstName;
            existingClass.LastName = customers.LastName;
            existingClass.Email = customers.Email;
            existingClass.Phone = customers.Phone;

            _context.Customers.Update(existingClass);
            await _context.SaveChangesAsync();

            return Ok(existingClass);
        }

        /// <summary>
        /// Delete a category by ID.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <returns>No content.</returns>
        /// <response code="204">Delete successful.</response>
        /// <response code="404">If the category is not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var existingClass = await _context.Customers.FindAsync(id);
            if (existingClass == null) return NotFound();

            _context.Customers.Remove(existingClass);
            await _context.SaveChangesAsync();
            return NoContent();

        }
    }
    public class Customermodel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
        public string Phone { get; set; }

    }
}
