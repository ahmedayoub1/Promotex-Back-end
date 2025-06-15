using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PromoTex.Models;

namespace PromoTex.Data_Access
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContext) : base(dbContext)
        {

        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ContactRequest> ContactRequests { get; set; }
        public DbSet<PasswordResetOTP> passwordResetOTPs { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<SellerRequests> SellerRequests { get; set; }
        public DbSet<ProductColor> ProductColors { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<RoleChangeRequest> RoleChangeRequests { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
           .Property(o => o.Status)
           .HasConversion<string>();

            modelBuilder.Entity<Store>()
            .HasMany(s => s.ApplicationUsers)
            .WithMany(u => u.Stores);


            // Product → User
            modelBuilder.Entity<Product>()
                .HasOne(p => p.User)
                .WithMany() // or .WithMany(u => u.Products) if added in ApplicationUser
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Product → Store
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Store)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.StoreId)
                .OnDelete(DeleteBehavior.NoAction);

            // Cart → User
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany() // or .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Order → User
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Buyer)
                .WithMany() // or .WithMany(u => u.Orders)
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Order → Store
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Store)
                .WithMany() // or s.Orders
                .HasForeignKey(o => o.StoreId)
                .OnDelete(DeleteBehavior.NoAction);

            // ContactRequest → User
            modelBuilder.Entity<ContactRequest>()
                .HasOne(c => c.User)
                .WithMany() // or u.ContactRequests
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Product → ProductColors
            modelBuilder.Entity<ProductColor>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.Colors)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product → ProductSizes
            modelBuilder.Entity<ProductSize>()
                .HasOne(ps => ps.Product)
                .WithMany(p => p.Sizes)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
