using HttpVerbsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HttpVerbsApi.DbContexts
{
    public class ProductDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("ProductDb");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
