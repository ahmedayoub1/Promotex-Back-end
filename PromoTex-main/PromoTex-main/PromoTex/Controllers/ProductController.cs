using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PromoTex.Data_Access;
using PromoTex.Models;
using PromoTex.DTO.Product;
using System.Security.Claims;

namespace PromoTex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<string?> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            var folderPath = Path.Combine("wwwroot", "Images", "Products");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return Path.Combine("Images", "Products", fileName).Replace("\\", "/");
        }

        [HttpPost("products")]
        [Authorize(Roles = "Seller")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddProduct([FromForm] CreateProduct dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var store = await _context.Stores
                .Include(s => s.ApplicationUsers)
                .FirstOrDefaultAsync(s => s.Name.ToLower() == dto.StoreName.ToLower() && s.ApplicationUsers.Any(u => u.Id == userId));

            if (store == null)
                return NotFound("Store not found or unauthorized.");

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.CategoryName.ToLower() && c.StoreId == store.Id);

            if (category == null)
                return NotFound("Category not found in this store.");

            var imageUrl = await SaveImageAsync(dto.ImageUrl);

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                ImageUrl = imageUrl,
                Brand = dto.Brand,
                Quantity = dto.Quantity,
                IsAvailable = dto.Quantity > 0,
                CategoryId = category.Id,
                StoreId = store.Id,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Colors = dto.Colors?.Select(c => new ProductColor { Color = c }).ToList(),
                Sizes = dto.Sizes?.Select(s => new ProductSize { Size = s }).ToList()
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.ImageUrl,
                product.Brand,
                Colors = product.Colors.Select(c => c.Color),
                Sizes = product.Sizes.Select(s => s.Size),
                product.Quantity,
                product.IsAvailable,
                CategoryId = category.Id,
                CategoryName = category.Name,
                StoreId = store.Id,
                StoreName = store.Name
            });
        }

        [HttpPut("products/{productId}")]
        [Authorize(Roles = "Seller")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromForm] UpdateProductDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var product = await _context.Products
                .Include(p => p.Store).ThenInclude(s => s.ApplicationUsers)
                .Include(p => p.Colors)
                .Include(p => p.Sizes)
                .FirstOrDefaultAsync(p => p.Id == productId && p.Store.ApplicationUsers.Any(u => u.Id == userId));

            if (product == null)
                return NotFound("Product not found or unauthorized.");

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.CategoryName.ToLower() && c.StoreId == product.StoreId);

            if (category == null)
                return NotFound("Category not found in this store.");

            var imageUrl = await SaveImageAsync(dto.ImageUrl);

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.ImageUrl = imageUrl ?? product.ImageUrl;
            product.Brand = dto.Brand;
            product.Quantity = dto.Quantity;
            product.IsAvailable = dto.Quantity > 0;
            product.CategoryId = category.Id;
            product.UpdatedAt = DateTime.UtcNow;

            _context.ProductColors.RemoveRange(product.Colors);
            _context.ProductSizes.RemoveRange(product.Sizes);
            await _context.SaveChangesAsync();

            product.Colors = dto.Colors?.Distinct().Select(c => new ProductColor { Color = c }).ToList() ?? new();
            product.Sizes = dto.Sizes?.Distinct().Select(s => new ProductSize { Size = s }).ToList() ?? new();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.ImageUrl,
                product.Brand,
                Colors = product.Colors.Select(c => c.Color),
                Sizes = product.Sizes.Select(s => s.Size),
                product.Quantity,
                product.IsAvailable,
                CategoryId = category.Id,
                CategoryName = category.Name,
                StoreId = product.StoreId,
                StoreName = product.Store.Name
            });
        }

        [HttpGet("products/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProduct(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Store)
                .Include(p => p.Sizes)
                .Include(p => p.Colors)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return NotFound("Product not found.");

            return Ok(new
            {
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.ImageUrl,
                product.Brand,
                Colors = product.Colors.Select(c => c.Color),
                Sizes = product.Sizes.Select(s => s.Size),
                product.Quantity,
                product.IsAvailable,
                CategoryId = product.Category.Id,
                CategoryName = product.Category.Name,
                StoreId = product.Store.Id,
                StoreName = product.Store.Name
            });
        }

        [HttpGet("products")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Store)
                .Include(p => p.Sizes)
                .Include(p => p.Colors)
                .ToListAsync();

            if (!products.Any())
                return NotFound("No products found.");

            return Ok(products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.ImageUrl,
                p.Brand,
                Colors = p.Colors.Select(c => c.Color),
                Sizes = p.Sizes.Select(s => s.Size),
                p.Quantity,
                p.IsAvailable,
                CategoryId = p.Category.Id,
                CategoryName = p.Category.Name,
                StoreId = p.Store.Id,
                StoreName = p.Store.Name
            }));
        }

        [HttpDelete("products/{productId}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var product = await _context.Products
                .Include(p => p.Store).ThenInclude(s => s.ApplicationUsers)
                .FirstOrDefaultAsync(p => p.Id == productId && p.Store.ApplicationUsers.Any(u => u.Id == userId));

            if (product == null)
                return NotFound("Product not found or unauthorized.");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product deleted successfully." });
        }
        // Get products by category name
        [HttpGet("products/by-category/{categoryName}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductsByCategoryName(string categoryName)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Store)
                .Include(p => p.Sizes)
                .Include(p => p.Colors)
                .Where(p => p.Category.Name.ToLower() == categoryName.ToLower())
                .ToListAsync();

            if (!products.Any())
                return NotFound($"No products found under category '{categoryName}'.");

            return Ok(products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.ImageUrl,
                p.Brand,
                Colors = p.Colors.Select(c => c.Color),
                Sizes = p.Sizes.Select(s => s.Size),
                p.Quantity,
                p.IsAvailable,
                CategoryId = p.Category.Id,
                CategoryName = p.Category.Name,
                StoreId = p.Store.Id,
                StoreName = p.Store.Name
            }));
        }
        //search by product name 
        [HttpGet("products/search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchProductsByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Search term cannot be empty.");

            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Store)
                .Include(p => p.Sizes)
                .Include(p => p.Colors)
                .Where(p => p.Name.ToLower().Contains(name.ToLower()))
                .ToListAsync();

            if (!products.Any())
                return NotFound($"No products found with name containing '{name}'.");

            return Ok(products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.ImageUrl,
                p.Brand,
                Colors = p.Colors.Select(c => c.Color),
                Sizes = p.Sizes.Select(s => s.Size),
                p.Quantity,
                p.IsAvailable,
                CategoryId = p.Category.Id,
                CategoryName = p.Category.Name,
                StoreId = p.Store.Id,
                StoreName = p.Store.Name
            }));
        }
    }
}
