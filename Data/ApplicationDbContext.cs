using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<ProductIngredient> ProductIngredients { get; set; }
        public DbSet<UsageObject> UsageObjects { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<SideEffect> SideEffects { get; set; }
        public DbSet<ProductSideEffect> ProductSideEffects { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ProductIngredient - Many-to-Many
            modelBuilder.Entity<ProductIngredient>()
                .HasKey(pi => new { pi.ProductId, pi.IngredientId });

            modelBuilder.Entity<ProductIngredient>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductIngredients)
                .HasForeignKey(pi => pi.ProductId);

            modelBuilder.Entity<ProductIngredient>()
                .HasOne(pi => pi.Ingredient)
                .WithMany(i => i.ProductIngredients)
                .HasForeignKey(pi => pi.IngredientId);

            // ProductSideEffect - Many-to-Many
            modelBuilder.Entity<ProductSideEffect>()
                .HasKey(ps => new { ps.ProductId, ps.SideEffectId });

            modelBuilder.Entity<ProductSideEffect>()
                .HasOne(ps => ps.Product)
                .WithMany(p => p.ProductSideEffects)
                .HasForeignKey(ps => ps.ProductId);

            modelBuilder.Entity<ProductSideEffect>()
                .HasOne(ps => ps.SideEffect)
                .WithMany(s => s.ProductSideEffects)
                .HasForeignKey(ps => ps.SideEffectId);
        }
    }
}
