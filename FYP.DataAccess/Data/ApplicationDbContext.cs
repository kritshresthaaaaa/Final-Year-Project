using Fyp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Fyp.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {


        }
        public DbSet<ProductDetail> Product { get; set; }

        public DbSet<CategoryDetail> Category { get; set; }
        public DbSet<BrandDetail> Brand { get; set; }
        public DbSet<EmployeeDetail> Employee { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProductDetail>().HasData(

                new ProductDetail
                {
                    Id = 1,
                    Name = "Product 1",
                    Description = "Product 1 Description",
                    Price = 100,
                    BrandID = 2,
                    CategoryID = 6,
                    RFIDTag = "123456",
                    Sizes = "S",
                    ImageUrl = ""

                },
                new ProductDetail
                {
                    Id = 2,
                    Name = "Product 2",
                    Description = "Product 2 Description",
                    Price = 200,
                    BrandID = 2,
                    CategoryID = 6,
                    RFIDTag = "123457",
                    Sizes = "M",
                    ImageUrl = ""
                }
                );

        }
    }
}
