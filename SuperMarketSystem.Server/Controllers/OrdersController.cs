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
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public OrdersController(ApplicationDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all orders.
        /// </summary>
        /// <returns>List of orders with pagination.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(new { message = "Page number and page size must be greater than 0." });
            }

            var totalRecords = await _context.Orders.CountAsync();
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginationResponse = new
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                Data = orders
            };

            return Ok(paginationResponse);
        }

        /// <summary>
        /// Get a specific order by ID.
        /// </summary>
        /// <param name="id">The ID of the order.</param>
        /// <returns>The requested order.</returns>
        /// <response code="200">Returns the order.</response>
        /// <response code="404">If the order is not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            return Ok(order);
        }
        /// <summary>
        /// Create a new order.
        /// </summary>
        /// <param name="order">The order details.</param>
        /// <param name="image">The image file associated with the order.</param>
        /// <returns>The newly created order.</returns>
        /// <response code="201">Order created successfully.</response>
        /// <response code="400">If the input is invalid.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder([FromForm] OrderModel order, IFormFile image)
        {
            if (image != null)
            {
                try
                {
                    // Define the directory to store images
                    var imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
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

                    // Set the image URL in the model
                    order.ImageUrl = $"/images/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "An error occurred while uploading the image.", details = ex.Message });
                }
            }

            Order model = new Order
            {
                OrderDate = order.OrderDate,
                CustomerId = order.CustomerId,
                TotalAmount = order.TotalAmount,
                ImageUrl = order.ImageUrl
            };

            _context.Orders.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = model.OrderId }, model);
        }

        /// <summary>
        /// Update an existing order.
        /// </summary>
        /// <param name="id">The ID of the order to update.</param>
        /// <param name="order">The updated order details.</param>
        /// <param name="image">The image file to update, if any.</param>
        /// <returns>No content.</returns>
        /// <response code="204">Update successful.</response>
        /// <response code="400">If the ID does not match or input is invalid.</response>
        /// <response code="404">If the order is not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrder(int id, [FromForm] OrderModel order, IFormFile image)
        {
            var existingOrder = await _context.Orders.FindAsync(id);
            if (existingOrder == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            if (image != null)
            {
                try
                {
                    // Define the directory to store images
                    var imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
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
                    existingOrder.ImageUrl = $"/images/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "An error occurred while uploading the image.", details = ex.Message });
                }
            }

            existingOrder.OrderDate = order.OrderDate;
            existingOrder.CustomerId = order.CustomerId;
            existingOrder.TotalAmount = order.TotalAmount;

            _context.Orders.Update(existingOrder);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        /// <summary>
        /// Delete an order by ID.
        /// </summary>
        /// <param name="id">The ID of the order to delete.</param>
        /// <returns>No content.</returns>
        /// <response code="204">Delete successful.</response>
        /// <response code="404">If the order is not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var existingOrder = await _context.Orders.FindAsync(id);
            if (existingOrder == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            _context.Orders.Remove(existingOrder);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class OrderModel
    {
        public DateTime OrderDate { get; set; }
        public int? CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string? ImageUrl { get; set; }
    }
}
