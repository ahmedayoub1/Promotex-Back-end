using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PromoTex.Data_Access;
using PromoTex.Models;
using System.Security.Claims;
using PromoTex.DTO.Category;

namespace PromoTex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost("stores/{storeName}/categories")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> CreateCategory(string storeName, [FromBody] CreateCategoryDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var store = await _context.Stores
                .Include(s => s.ApplicationUsers)
                .FirstOrDefaultAsync(s => s.Name == storeName && s.ApplicationUsers.Any(u => u.Id == userId));

            if (store == null)
                return NotFound("Store not found or unauthorized");


            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == dto.Name && c.StoreId == store.Id);

            if (existingCategory != null)
                return Conflict("Category with this name already exists in this store.");

            var category = new Category
            {
                Name = dto.Name,
                StoreId = store.Id
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                category.Id,
                category.Name,
                category.StoreId,
                StoreName = category.Store.Name
            });
        }

        [HttpPut("stores/{storeName}/categories/{categoryName}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> UpdateCategory(string storeName, string categoryName, [FromBody] UpdateCategoryDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var category = await _context.Categories
                .Include(c => c.Store)
                .FirstOrDefaultAsync(c => c.Name == categoryName && c.Store.Name == storeName && c.Store.ApplicationUsers.Any(u => u.Id == userId));

            if (category == null)
                return NotFound("Category not found or unauthorized");

            category.Name = dto.Name;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                category.Id,
                category.Name,
                category.StoreId,
                StoreName = category.Store.Name
            });
        }




        [HttpDelete("categories/by-name")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> DeleteCategoryByStoreAndCategoryName([FromQuery] string storeName, [FromQuery] string categoryName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(storeName) || string.IsNullOrWhiteSpace(categoryName))
                return BadRequest("Store name and category name are required.");

            var category = await _context.Categories
                .Include(c => c.Store)
                .ThenInclude(s => s.ApplicationUsers)
                .FirstOrDefaultAsync(c =>
                    c.Name.ToLower() == categoryName.ToLower() &&
                    c.Store.Name.ToLower() == storeName.ToLower() &&
                    c.Store.ApplicationUsers.Any(u => u.Id == userId));

            if (category == null)
                return NotFound("Category not found or unauthorized");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Category '{categoryName}' from store '{storeName}' deleted successfully." });
        }



        [HttpGet("by-name/{storeName}/categories")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> GetStoreCategoriesByName(string storeName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Find the store by name and verify ownership
            var store = await _context.Stores
                .Include(s => s.ApplicationUsers)
                .FirstOrDefaultAsync(s =>
                    s.Name.ToLower() == storeName.ToLower() &&
                    s.ApplicationUsers.Any(u => u.Id == userId));

            if (store == null)
                return NotFound("Store not found or unauthorized");

            // Get categories for the store
            var categories = await _context.Categories
                .Where(c => c.StoreId == store.Id)
                .Select(c => new
                {
                    c.Id,
                    c.Name
                })
                .ToListAsync();

            return Ok(categories);
        }




    }
}
