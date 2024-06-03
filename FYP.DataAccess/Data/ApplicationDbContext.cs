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

        public DbSet<ProductRecommendation> ProductRecommendations { get; set; }
        public DbSet<CategoryDetail> Category { get; set; }
        public DbSet<BrandDetail> Brand { get; set; }
        public DbSet<EmployeeDetail> Employee { get; set; }
        public DbSet<SKUDetail> SKU { get; set; }
        public DbSet<DiscountDetail> Discount { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Noti> Notification { get; set; }
        public DbSet<SoldRFIDTags> SoldRFIDTags { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductRecommendation>()
      .HasKey(pr => new { pr.ProductId, pr.RecommendedProductId });

            // Configure the relationship and foreign key for the product that makes the recommendation
            modelBuilder.Entity<ProductRecommendation>()
                .HasOne(pr => pr.Product)
                .WithMany(p => p.RecommendedProducts)
                .HasForeignKey(pr => pr.ProductId)
                .OnDelete(DeleteBehavior.Restrict);  // Adjust the delete behavior as needed

            // Configure the relationship and foreign key for the product being recommended
            modelBuilder.Entity<ProductRecommendation>()
                .HasOne(pr => pr.RecommendedProduct)
                .WithMany(p => p.ProductRecommendations)
                .HasForeignKey(pr => pr.RecommendedProductId)
                .OnDelete(DeleteBehavior.Restrict);  // Adjust the delete behavior as needed


            modelBuilder.Entity<DiscountDetail>()
                .Property(p => p.Percentage)
                .HasPrecision(18, 2); 

           
        }

   
    }
}
