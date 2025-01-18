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
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ProductsController(ApplicationDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all products.
        /// </summary>
        /// <returns>List of products with pagination.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(new { message = "Page number and page size must be greater than 0." });
            }

            var totalRecords = await _context.Products.CountAsync();
            var products = await _context.Products
                .Include(p => p.Category)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginationResponse = new
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                Data = products
            };

            return Ok(paginationResponse);
        }

        /// <summary>
        /// Get a specific product by ID.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <returns>The requested product.</returns>
        /// <response code="200">Returns the product.</response>
        /// <response code="404">If the product is not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            return Ok(product);
        }

        /// <summary>
        /// Create a new product.
        /// </summary>
        /// <param name="product">The product details.</param>
        /// <param name="image">The image file for the product.</param>
        /// <returns>The newly created product.</returns>
        /// <response code="201">Product created successfully.</response>
        /// <response code="400">If the input is invalid.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProduct([FromForm] ProductModel product, IFormFile image)
        {
            if (image != null)
            {
                try
                {
                    // Define the directory to store images
                    var imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "C:/Users/sahil/OneDrive/Desktop/Project/SuperMarketSystem/supermarketsystem.client/public/Images/Products");
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

                    // Set the image URL in the product model
                    product.ImageUrl = $"/images/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "An error occurred while uploading the image.", details = ex.Message });
                }
            }

            Product newProduct = new Product
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
               // CategoryId = product.CategoryId,
                ImageUrl = product.ImageUrl
            };

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = newProduct.ProductId }, newProduct);
        }

        /// <summary>
        /// Update an existing product.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="product">The updated product details.</param>
        /// <param name="image">The image file to update, if any.</param>
        /// <returns>No content.</returns>
        /// <response code="204">Update successful.</response>
        /// <response code="400">If the ID does not match or input is invalid.</response>
        /// <response code="404">If the product is not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductModel product, IFormFile image)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            if (image != null)
            {
                try
                {
                    // Define the directory to store images
                    var imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "C:/Users/sahil/OneDrive/Desktop/Project/SuperMarketSystem/supermarketsystem.client/public/Images/Products");
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
                    existingProduct.ImageUrl = $"/images/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "An error occurred while uploading the image.", details = ex.Message });
                }
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.StockQuantity = product.StockQuantity;
           // existingProduct.CategoryId = product.CategoryId;

            _context.Products.Update(existingProduct);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Delete a product by ID.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <returns>No content.</returns>
        /// <response code="204">Delete successful.</response>
        /// <response code="404">If the product is not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            _context.Products.Remove(existingProduct);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class ProductModel
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        //public int? CategoryId { get; set; }
        public string? ImageUrl { get; set; }
    }
}
