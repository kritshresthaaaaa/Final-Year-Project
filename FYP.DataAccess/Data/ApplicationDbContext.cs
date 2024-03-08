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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductRecommendation>()
                .HasKey(pr => new { pr.ProductId, pr.RecommendedProductId });

            modelBuilder.Entity<ProductRecommendation>()
                .HasOne(pr => pr.Product)
                .WithMany(p => p.RecommendedProducts) // Assuming ProductDetail has a collection of ProductRecommendation
                .HasForeignKey(pr => pr.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // If you decide to add a navigation property for reverse navigation from RecommendedProduct back to its recommendations, you would adjust this:
            modelBuilder.Entity<ProductRecommendation>()
                .HasOne(pr => pr.RecommendedProduct)
                .WithMany() // Replace with .WithMany(r => r.ReverseNavigationProperty) if applicable
                .HasForeignKey(pr => pr.RecommendedProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure decimal precision for DiscountDetail entity
            modelBuilder.Entity<DiscountDetail>()
                .Property(p => p.Percentage)
                .HasPrecision(18, 2); // Adjust precision and scale as needed

            // Since your ProductDetail seeding includes a reference to SKU, 
            // you need to ensure the SKUDetail entity is correctly set up and seeded as well.
            // This assumes you have a separate SKUDetail entity that your ProductDetail references through SKU.
            // Ensure you adjust your model definitions and seeding logic if SKUDetail is not set up as assumed.

            // Seed SKUDetails (assuming SKUDetail entity exists and is correctly related to ProductDetail)




            modelBuilder.Entity<SKUDetail>().HasData(
                new SKUDetail { SKUID = 1, Code = "GUC-MC-BLA-S" },
                new SKUDetail { SKUID = 2, Code = "NIK-WC-FLO-M" },
                new SKUDetail { SKUID = 3, Code = "NIK-WC-PRO-M" },
                new SKUDetail { SKUID = 4, Code = "NIK-WC-PRO-XL" }
            );


            modelBuilder.Entity<ProductDetail>().HasData(
                new ProductDetail
                {
                    Id = 1,
                    Name = "Black Tshirt",
                    Description = "Product 1 Description",
                    Price = 100,
                    BrandID = 1,
                    CategoryID = 1,
                    RFIDTag = "123456",
                    Sizes = "S",
                    ImageUrl = "",
                    SKUID = 1 // Reference to the seeded SKUDetail,

                },
                new ProductDetail
                {
                    Id = 2,
                    Name = "Florence Tshirt",
                    Description = "Product 2 Description",
                    Price = 200,
                    BrandID = 2,
                    CategoryID = 2,
                    RFIDTag = "123457",
                    Sizes = "M",
                    ImageUrl = "",
                    SKUID = 2 // Reference to the seeded SKUDetail
                },
                    new ProductDetail
                    {
                        Id = 3,
                        Name = "Product 3",
                        Description = "Product 3 Description",
                        Price = 200,
                        BrandID = 2,
                        CategoryID = 2,
                        RFIDTag = "123450",
                        Sizes = "M",
                        ImageUrl = "",
                        SKUID = 2 // Reference to the seeded SKUDetail
                    },
                        new ProductDetail
                        {
                            Id = 4,
                            Name = "Product 4",
                            Description = "Product 4 Description",
                            Price = 200,
                            BrandID = 2,
                            CategoryID = 2,
                            RFIDTag = "123488",
                            Sizes = "XL",
                            ImageUrl = "",
                            SKUID = 2 // Reference to the seeded SKUDetail
                        },
                          new ProductDetail
                          {
                              Id = 5,
                              Name = "Product 5",
                              Description = "Product 5 Description",
                              Price = 200,
                              BrandID = 2,
                              CategoryID = 2,
                              RFIDTag = "123498",
                              Sizes = "XL",
                              ImageUrl = "",
                              SKUID = 2 // Reference to the seeded SKUDetail
                          },
                            new ProductDetail
                            {
                                Id = 6,
                                Name = "Product 6",
                                Description = "Product 6 Description",
                                Price = 200,
                                BrandID = 2,
                                CategoryID = 2,
                                RFIDTag = "123490",
                                Sizes = "XL",
                                ImageUrl = "",
                                SKUID = 2 // Reference to the seeded SKUDetail
                            },
                                new ProductDetail
                                {
                                    Id = 7,
                                    Name = "Product 7",
                                    Description = "Product 7 Description",
                                    Price = 200,
                                    BrandID = 2,
                                    CategoryID = 2,
                                    RFIDTag = "123496",
                                    Sizes = "XL",
                                    ImageUrl = "",
                                    SKUID = 2 // Reference to the seeded SKUDetail
                                },
                                        new ProductDetail
                                        {
                                            Id = 8,
                                            Name = "Black Tshirt",
                                            Description = "black t prodyc 1 ",
                                            Price = 100,
                                            BrandID = 1,
                                            CategoryID = 1,
                                            RFIDTag = "12312312",
                                            Sizes = "S",
                                            ImageUrl = "",
                                            SKUID = 1 // Reference to the seeded SKUDetail,

                                        }, new ProductDetail
                                        {
                                            Id = 9,
                                            Name = "Black Tshirt",
                                            Description = "black t prodyc 1 ",
                                            Price = 100,
                                            BrandID = 1,
                                            CategoryID = 1,
                                            RFIDTag = "12312412",
                                            Sizes = "S",
                                            ImageUrl = "",
                                            SKUID = 1 // Reference to the seeded SKUDetail,

                                        }


            );
        }


        /*protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProductDetail>().HasData(

                new ProductDetail
                {
                    Id = 1,
                    Name = "Black Tshirt",
                    Description = "Product 1 Description",
                    Price = 100,
                    BrandID = 2,
                    CategoryID = 6,
                    RFIDTag = "123456",
                    Sizes = "S",
                    ImageUrl = "",
                    SKU = "GUC-MC-BLA-S"

                },
                new ProductDetail
                {
                    Id = 2,
                    Name = "Florence Tshirt",
                    Description = "Product 2 Description",
                    Price = 200,
                    BrandID = 2,
                    CategoryID = 6,
                    RFIDTag = "123457",
                    Sizes = "M",
                    ImageUrl = "",
                    SKU = "GUC-MC-FLO-M"
                }
                );

        }*/
    }
}
