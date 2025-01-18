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
    public class OrderItemsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public OrderItemsController(ApplicationDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all order items.
        /// </summary>
        /// <returns>List of order items with pagination.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderItems([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(new { message = "Page number and page size must be greater than 0." });
            }

            var totalRecords = await _context.OrderItems.CountAsync();
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginationResponse = new
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                Data = orderItems
            };

            return Ok(paginationResponse);
        }

        /// <summary>
        /// Get a specific order item by ID.
        /// </summary>
        /// <param name="id">The ID of the order item.</param>
        /// <returns>The requested order item.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderItem(int id)
        {
            var orderItem = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .FirstOrDefaultAsync(oi => oi.OrderItemId == id);

            if (orderItem == null)
            {
                return NotFound(new { message = "Order item not found." });
            }

            return Ok(orderItem);
        }

        /// <summary>
        /// Create a new order item.
        /// </summary>
        /// <param name="orderItemModel">The order item details.</param>
        /// <returns>The newly created order item.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrderItem([FromForm] OrderItemModel orderItemModel, IFormFile image)
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
                    orderItemModel.ImageUrl = $"/images/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "An error occurred while uploading the image.", details = ex.Message });
                }
            }

            OrderItem orderItem = new OrderItem
            {
                OrderId = orderItemModel.OrderId,
                ProductId = orderItemModel.ProductId,
                Quantity = orderItemModel.Quantity,
                Subtotal = orderItemModel.Subtotal,
                ImageUrl = orderItemModel.ImageUrl
            };

            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrderItem), new { id = orderItem.OrderItemId }, orderItem);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderItem(int id, [FromForm] OrderItemModel orderItemModel, IFormFile image)
        {
            var existingOrderItem = await _context.OrderItems.FindAsync(id);
            if (existingOrderItem == null)
            {
                return NotFound(new { message = "Order item not found." });
            }

            existingOrderItem.OrderId = orderItemModel.OrderId;
            existingOrderItem.ProductId = orderItemModel.ProductId;
            existingOrderItem.Quantity = orderItemModel.Quantity;
            existingOrderItem.Subtotal = orderItemModel.Subtotal;

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
                    existingOrderItem.ImageUrl = $"/images/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "An error occurred while uploading the image.", details = ex.Message });
                }
            }

            _context.OrderItems.Update(existingOrderItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        /// <summary>
        /// Delete an order item by ID.
        /// </summary>
        /// <param name="id">The ID of the order item to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOrderItem(int id)
        {
            var existingOrderItem = await _context.OrderItems.FindAsync(id);
            if (existingOrderItem == null)
            {
                return NotFound(new { message = "Order item not found." });
            }

            _context.OrderItems.Remove(existingOrderItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class OrderItemModel
    {
        public int? OrderId { get; set; }
        public int? ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public string? ImageUrl { get; set; }
    }
}
