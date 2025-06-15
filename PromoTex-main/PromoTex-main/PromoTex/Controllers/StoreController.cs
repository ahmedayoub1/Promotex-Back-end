using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PromoTex.Data_Access;
using PromoTex.Models;
using System.Security.Claims;
using PromoTex.DTO.Store;
using Microsoft.EntityFrameworkCore;

namespace PromoTex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class StoreController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StoreController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> CreateStore([FromForm] StoreDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingStore = await _context.Stores
                .FirstOrDefaultAsync(s => s.Name == dto.Name);

            if (existingStore != null)
                return Conflict("Store with this name already exists.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");

            string logoUrl = null;
            if (dto.LogoUrl != null && dto.LogoUrl.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Stores");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.LogoUrl.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.LogoUrl.CopyToAsync(fileStream);
                }

                logoUrl = $"/Images/Stores/{uniqueFileName}";
            }

            var store = new Store
            {
                Name = dto.Name,
                Description = dto.Description,
                PhoneNumber = dto.PhoneNumber,
                LogoUrl = logoUrl, // set the saved image url here
                Address = dto.Address,
                ApplicationUsers = new List<ApplicationUser> { user }
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                store.Id,
                store.Name,
                store.Description,
                store.PhoneNumber,
                store.LogoUrl,
                store.Address
            });
        }



        [HttpPut("stores/{storeName}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> UpdateStore(string storeName, [FromForm] StoreDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            var store = await _context.Stores
                .Include(s => s.ApplicationUsers)
                .FirstOrDefaultAsync(s => s.Name == storeName && s.ApplicationUsers.Any(u => u.Id == userId));

            if (store == null)
                return NotFound("Store not found or unauthorized");


            string logoUrl = null;
            if (dto.LogoUrl != null && dto.LogoUrl.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Stores");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.LogoUrl.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.LogoUrl.CopyToAsync(fileStream);
                }

                logoUrl = $"/Images/Stores/{uniqueFileName}";
            }
            store.Name = dto.Name;
            store.Description = dto.Description;
            store.PhoneNumber = dto.PhoneNumber;
            store.Address = dto.Address;
            store.LogoUrl = logoUrl;

            await _context.SaveChangesAsync();
            return Ok(new
            {
                store.Id,
                store.Name,
                store.Description,
                store.PhoneNumber,
                store.LogoUrl,
                store.Address
            });


        }


        [HttpDelete("stores/{storeName}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> DeleteStore(string storeName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            
            var store = await _context.Stores
                .Include(s => s.ApplicationUsers)
                .FirstOrDefaultAsync(s => s.Name == storeName && s.ApplicationUsers.Any(u => u.Id == userId));

            if (store == null)
                return NotFound("Store not found or unauthorized");

            _context.Stores.Remove(store);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Store deleted successfully" });
        }




        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllStores()
        {
            var stores = await _context.Stores
                .Include(s => s.Categories)
                    .ThenInclude(c => c.Products)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.PhoneNumber,
                    s.LogoUrl,
                    s.Address,
                    Categories = s.Categories.Select(c => new
                    {
                        c.Id,
                        c.Name,
                        Products = c.Products.Select(p => new
                        {
                            p.Id,
                            p.Name,
                            p.Description,
                            p.Price,
                            p.ImageUrl
                        })
                    }),
                    ProductCount = s.Categories.SelectMany(c => c.Products).Count()
                })
                .ToListAsync();

            return Ok(stores);
        }
        //get all Stores to the Same OWNER
        [HttpGet("my-stores")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> GetMyStores()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var stores = await _context.Stores
                .Include(s => s.Categories) // Optional: include categories
                .Include(s => s.Products)   // Optional: include products
                .Where(s => s.ApplicationUsers.Any(u => u.Id == userId))
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.PhoneNumber,
                    s.LogoUrl,
                    s.Address,
                    CategoryCount = s.Categories.Count,
                    ProductCount = s.Products.Count
                })
                .ToListAsync();

            return Ok(stores);
        }

        //Get Sorte by Name
        [HttpGet("{storeName}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStoreByName(string storeName)
        {
            var store = await _context.Stores
                .Include(s => s.Categories)
                    .ThenInclude(c => c.Products)
                .FirstOrDefaultAsync(s => s.Name == storeName);
            if (store == null)
                return NotFound("Store not found");
            return Ok(new
            {

                store.Name,
                store.Description,
                store.PhoneNumber,
                store.LogoUrl,
                store.Address,
                Categories = store.Categories.Select(c => new
                {

                    c.Name,
                    Products = c.Products.Select(p => new
                    {

                        p.Name,
                        p.Description,
                        p.Price,
                        p.ImageUrl
                    })
                }),
                ProductCount = store.Categories.SelectMany(c => c.Products).Count()
            });

        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchStores([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Search keyword is required.");

            var stores = await _context.Stores
                .Where(s => s.Name.Contains(name))
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.PhoneNumber,
                    s.LogoUrl,
                    s.Address
                })
                .ToListAsync();

            return Ok(stores);
        }




    }
}

