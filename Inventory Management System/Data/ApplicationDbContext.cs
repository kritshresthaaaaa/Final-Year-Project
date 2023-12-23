using Inventory_Management_System.Models.Stock;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_System.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {
            
        }
        public DbSet<ProductDetail> Product { get; set; }
        public DbSet<CategoryDetail> Category { get; set; }
        public DbSet<BrandDetail> Brand { get; set; }

    }
}
