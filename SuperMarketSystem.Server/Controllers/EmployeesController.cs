using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperMarketSystem.Server.DATA;
using SuperMarketSystem.Server.Models;
using System.Threading.Tasks;

namespace SuperMarketSystem.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public EmployeesController(ApplicationDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all employees.
        /// </summary>
        /// <returns>List of employees with pagination.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEmployees([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(new { message = "Page number and page size must be greater than 0." });
            }

            var totalRecords = await _context.Employees.CountAsync();
            var employees = await _context.Employees
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginationResponse = new
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                Data = employees
            };

            return Ok(paginationResponse);
        }

        /// <summary>
        /// Get a specific employee by ID.
        /// </summary>
        /// <param name="id">The ID of the employee.</param>
        /// <returns>The requested employee.</returns>
        /// <response code="200">Returns the employee.</response>
        /// <response code="404">If the employee is not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound(new { message = "Employee not found." });
            }

            return Ok(employee);
        }

        /// <summary>
        /// Create a new employee.
        /// </summary>
        /// <param name="employee">The employee details.</param>
        /// <returns>The newly created employee.</returns>
        /// <response code="201">Employee created successfully.</response>
        /// <response code="400">If the input is invalid.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateEmployee([FromForm] Employeemodel employee, IFormFile image)
        {
            if (string.IsNullOrEmpty(employee.FirstName) || string.IsNullOrEmpty(employee.LastName))
            {
                return BadRequest(new { message = "First name and last name are required." });
            }

            if (image != null)
            {
                try
                {
                    // Define the directory to store the images
                    var imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "C:/Users/sahil/OneDrive/Desktop/Project/SuperMarketSystem/supermarketsystem.client/public/Images/Employee");
                    if (!Directory.Exists(imagesDirectory))
                    {
                        Directory.CreateDirectory(imagesDirectory);
                    }

                    // Generate a unique filename
                    var safeFileName = Path.GetFileName(image.FileName);
                    var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
                    var filePath = Path.Combine(imagesDirectory, uniqueFileName);

                    // Save the image file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    // Set the image URL
                    employee.ImageUrl = $"/Images/Employee/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "An error occurred while uploading the image.", details = ex.Message });
                }
            }

            Employee model = new Employee
            {
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Role = employee.Role,
                Salary = employee.Salary,
                ImageUrl = employee.ImageUrl
            };

            _context.Employees.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployee), new { id = model.EmployeeId }, model);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEmployee(int id, [FromForm] Employeemodel employee, IFormFile image)
        {
            var existingEmployee = await _context.Employees.FindAsync(id);
            if (existingEmployee == null)
            {
                return NotFound(new { message = "Employee not found." });
            }

            if (string.IsNullOrEmpty(employee.FirstName) || string.IsNullOrEmpty(employee.LastName))
            {
                return BadRequest(new { message = "First name and last name are required." });
            }

            existingEmployee.FirstName = employee.FirstName;
            existingEmployee.LastName = employee.LastName;
            existingEmployee.Role = employee.Role;
            existingEmployee.Salary = employee.Salary;

            if (image != null)
            {
                try
                {
                    // Define the directory to store the images
                    var imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "C:/Users/sahil/OneDrive/Desktop/Project/SuperMarketSystem/supermarketsystem.client/public/Images/Employee");
                    if (!Directory.Exists(imagesDirectory))
                    {
                        Directory.CreateDirectory(imagesDirectory);
                    }

                    // Generate a unique filename
                    var safeFileName = Path.GetFileName(image.FileName);
                    var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
                    var filePath = Path.Combine(imagesDirectory, uniqueFileName);

                    // Save the image file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    // Update the image URL
                    existingEmployee.ImageUrl = $"/Images/Employee/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "An error occurred while uploading the image.", details = ex.Message });
                }
            }

            _context.Employees.Update(existingEmployee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Delete an employee by ID.
        /// </summary>
        /// <param name="id">The ID of the employee to delete.</param>
        /// <returns>No content.</returns>
        /// <response code="204">Delete successful.</response>
        /// <response code="404">If the employee is not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var existingEmployee = await _context.Employees.FindAsync(id);
            if (existingEmployee == null)
            {
                return NotFound(new { message = "Employee not found." });
            }

            _context.Employees.Remove(existingEmployee);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class Employeemodel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public decimal Salary { get; set; }
        public string? ImageUrl { get; set; }
    }
}
