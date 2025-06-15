using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PromoTex.Data_Access;
using PromoTex.DTO.Order;
using PromoTex.Enums;
using PromoTex.Models;
using System.Security.Claims;

namespace PromoTex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]//must login to make order
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost]

        public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (dto.Products == null || !dto.Products.Any())
                return BadRequest("Order must contain at least one product.");

            var productIds = dto.Products.Select(p => p.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.Category)
                    .ThenInclude(c => c.Store)
                .ToListAsync();

            if (products.Count != productIds.Count)
                return BadRequest("Some products were not found.");

            //  Group by Store
            var groupedByStore = products
                .GroupBy(p => p.Category.Store)
                .ToList();

            var orders = new List<Order>();

            foreach (var group in groupedByStore)
            {
                var store = group.Key;
                var storeItems = dto.Products
                    .Where(p => group.Select(gp => gp.Id).Contains(p.ProductId))
                    .ToList();

                var orderItems = new List<OrderItem>();
                decimal totalPrice = 0;

                foreach (var item in storeItems)
                {
                    var product = group.First(p => p.Id == item.ProductId);

                    orderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,

                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    });

                    totalPrice += product.Price * item.Quantity;
                }

                var order = new Order
                {
                    BuyerId = userId,
                    StoreId = store.Id,
                    ShippingAddress = dto.ShippingAddress,
                    Items = orderItems,
                    TotalPrice = totalPrice,
                    TotalAmount = totalPrice,
                    Status = OrderStatus.Pending
                };

                orders.Add(order);
                _context.Orders.Add(order);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Order(s) placed successfully",
                Orders = orders.Select(o => new
                {
                    o.Id,
                    o.StoreId,
                    o.TotalPrice,
                    o.OrderDate
                })
            });
        }

        //get my ording History
        [HttpGet("my-orders")]

        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.Orders
                .Where(o => o.BuyerId == userId)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Store)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders.Select(order => new
            {
                order.Id,
                order.OrderDate,
                order.TotalPrice,
                order.Status,

                Store = new { order.Store.Name, order.Store.LogoUrl },
                Items = order.Items.Select(i => new
                {
                    i.ProductId,
                    i.Product.Name,
                    i.Quantity,
                    i.UnitPrice,
                    i.Product.ImageUrl,
                    Total = i.Quantity * i.UnitPrice
                })
            }));
        }






    }
}
