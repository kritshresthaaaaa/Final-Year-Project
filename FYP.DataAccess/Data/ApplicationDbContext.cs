using Fyp.Models;
using Microsoft.EntityFrameworkCore;

namespace Fyp.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {
            
        }
        public DbSet<ProductDetail> Product { get; set; }
        public DbSet<CategoryDetail> Category { get; set; }
        public DbSet<BrandDetail> Brand { get; set; }
        public DbSet<EmployeeDetail> Employee { get; set; }       
    }
}
