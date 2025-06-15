using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PromoTex.Data_Access;
using PromoTex.DTO;
using PromoTex.DTO.Cart;
using PromoTex.Models;
using System.Security.Claims;
namespace PromoTex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddProductToCart([FromBody] AddToCartDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (dto.Quantity <= 0)
                return BadRequest("Quantity must be greater than zero.");

            // Check if product exists
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                return NotFound("Product not found.");

            // Get or create cart for this user
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(cart);
            }

            // Check if product is already in the cart
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == dto.ProductId);
            if (cartItem != null)
            {
                // Update quantity
                cartItem.Quantity += dto.Quantity;
            }
            else
            {
                // Add new item
                cart.CartItems.Add(new CartItem
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product added to cart successfully" });
        }

        [HttpGet("View")]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return Ok(new { Items = new List<object>() }); // empty cart

            var result = cart.CartItems.Select(ci => new
            {
                ci.ProductId,
                ci.Product.Name,
                ci.Product.Price,
                ci.Quantity,
                ci.Product.ImageUrl
            });

            return Ok(result);
        }


        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveProductFromCart(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
                return NotFound("Cart Is Empty.");
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null)
                return NotFound("Product not found in cart.");
            cart.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Product removed from cart successfully" });
        }

    }
}

