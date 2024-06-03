using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Utility;
using FypWeb.Areas.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FypWeb.Areas.Recommendation.Controllers
{
    [Area("Recommendation")]
    [Authorize(Roles = SD.Role_Customer_Handler)]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private Reader _reader;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
            _reader = new Reader("192.168.1.1", false);
        }
      /*  [Authorize]*/
        public IActionResult Index()
        {
            return View("Index", "_Customers");
        }
      
        public IActionResult TrailRoom()
        {
            return View("Room", "_Customers");
        }
        public async Task<List<string>> ReadTags()
        {
            var reader = new Reader("192.168.1.1", false); // Configure your Reader
            reader.ConnectToDevice();
            reader.SetDeviceReadMode();
            reader.StartDevice();
            var detectedEPCs = reader.GetDetectedEPCs();
            await Task.Delay(2000); // Consider using await Task.Delay(2000); for async operation

            // Filter detected EPCs to only include those present in the database
            var validEPCs = new List<string>();
            foreach (var epc in detectedEPCs)
            {
                // Check if the EPC exists in the database
                var product = await _context.Product.FirstOrDefaultAsync(p => p.RFIDTag == epc);
                if (product != null)
                {
                    validEPCs.Add(epc);
                }
            }

            return validEPCs;
        }
        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAllRecommendedItems()
        {
            try
            {
                var rfids = await ReadTags(); // Get RFID tags
                if (rfids == null || rfids.Count == 0)
                {
                    return Json(new { success = false, message = "No RFID tags detected." });
                }

                var recommendedProductsDict = new Dictionary<string, Dictionary<string, List<ProductDetail>>>();

                // Iterate over each RFID tag in the list
                foreach (var rfid in rfids)
                {
                    // Query the product table to retrieve the product based on the RFID tag
                    var product = await _context.Product
                        .Include(p => p.RecommendedProducts) // Include recommended products
                        .ThenInclude(rp => rp.RecommendedProduct) // Include recommended product details
                        .FirstOrDefaultAsync(p => p.RFIDTag == rfid);

                    if (product != null)
                    {
                        // Initialize dictionary entry for the current RFID tag
                        recommendedProductsDict[rfid] = new Dictionary<string, List<ProductDetail>>
                {
                    { "product", new List<ProductDetail>() },
                    { "recommended", new List<ProductDetail>() }
                };

                        // Fetch today's date for discount validation
                        var today = DateTime.Today;

                        // Query for applicable discount for the main product
                        var mainProductDiscount = await _context.Discount
                            .Where(d => d.IsActive && today >= d.StartDate && today <= d.EndDate &&
                                        (d.CategoryID == null || d.CategoryID == product.CategoryID) &&
                                        (d.BrandID == null || d.BrandID == product.BrandID))
                            .OrderByDescending(d => d.Percentage)
                            .FirstOrDefaultAsync();

                        // Calculate the discounted price for the main product
                        var mainProductDiscountedPrice = product.Price;
                        if (mainProductDiscount != null)
                        {
                            mainProductDiscountedPrice = product.Price * (1 - (double)mainProductDiscount.Percentage / 100.0);
                        }

                        // Add the main product to the dictionary under the corresponding RFID tag with discount details
                        recommendedProductsDict[rfid]["product"].Add(new ProductDetail
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Description = product.Description,
                            Price = product.Price,
                            DiscountedPrice = mainProductDiscountedPrice,
                            BrandID = product.BrandID,
                            CategoryID = product.CategoryID,
                            SKUID = product.SKUID,
                            SKU = product.SKU, // Assuming SKU is not null
                            Brand = product.Brand, // Assuming Brand is not null
                            Category = product.Category, // Assuming Category is not null
                            RFIDTag = product.RFIDTag,
                            Sizes = product.Sizes,
                            ImageUrl = product.ImageUrl,
                            ColorCode = product.ColorCode,
                            RecommendedProducts = product.RecommendedProducts
                        });

                        // Check if there are recommended products associated with the main product
                        if (product.RecommendedProducts != null && product.RecommendedProducts.Any())
                        {
                            // Fetch recommended products' details
                            var recommendedProducts = await _context.ProductRecommendations
                                .Include(pr => pr.RecommendedProduct) // Include the recommended product details
                                .Where(rp => rp.ProductId == product.Id)
                                .Select(rp => rp.RecommendedProduct) // Select only the recommended product details
                                .ToListAsync();

                            // Add the recommended products to the dictionary under the corresponding RFID tag
                            foreach (var recommendedProduct in recommendedProducts)
                            {
                                // Query for applicable discount for the recommended product
                                var discount = await _context.Discount
                                    .Where(d => d.IsActive && today >= d.StartDate && today <= d.EndDate &&
                                                (d.CategoryID == null || d.CategoryID == recommendedProduct.CategoryID) &&
                                                (d.BrandID == null || d.BrandID == recommendedProduct.BrandID))
                                    .OrderByDescending(d => d.Percentage)
                                    .FirstOrDefaultAsync();

                                // Calculate the discounted price for the recommended product
                                var discountedPrice = recommendedProduct.Price;
                                if (discount != null)
                                {
                                    discountedPrice = recommendedProduct.Price * (1 - (double)discount.Percentage / 100.0);
                                }

                                // Add recommended product details with discount information to the dictionary
                                recommendedProductsDict[rfid]["recommended"].Add(new ProductDetail
                                {
                                    Id = recommendedProduct.Id,
                                    Name = recommendedProduct.Name,
                                    Description = recommendedProduct.Description,
                                    Price = recommendedProduct.Price,
                                    DiscountedPrice = discountedPrice,
                                    BrandID = recommendedProduct.BrandID,
                                    CategoryID = recommendedProduct.CategoryID,
                                    SKUID = recommendedProduct.SKUID,
                                    SKU = recommendedProduct.SKU, // Assuming SKU is not null
                                    Brand = recommendedProduct.Brand, // Assuming Brand is not null
                                    Category = recommendedProduct.Category, // Assuming Category is not null
                                    RFIDTag = recommendedProduct.RFIDTag,
                                    Sizes = recommendedProduct.Sizes,
                                    ImageUrl = recommendedProduct.ImageUrl,
                                    ColorCode = recommendedProduct.ColorCode,
                                    RecommendedProducts = recommendedProduct.RecommendedProducts
                                });
                            }
                        }
                    }
                }

                // Return the dictionary containing product and recommended product details for each RFID tag
                return Json(new { success = true, data = recommendedProductsDict });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a JSON error response
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        /* public async Task<IActionResult> GetAllRecommendedItems()
         {
             // Define the RFID tags in a dictionary
             Dictionary<string, Dictionary<string, List<ProductDetail>>> recommendedProductsDict = new Dictionary<string, Dictionary<string, List<ProductDetail>>>
     {
         {
             "123456", new Dictionary<string, List<ProductDetail>>
             {
                 { "product", new List<ProductDetail>() },
                 { "recommended", new List<ProductDetail>() }
             }
         },
         {
             "12312412", new Dictionary<string, List<ProductDetail>>
             {
                 { "product", new List<ProductDetail>() },
                 { "recommended", new List<ProductDetail>() }
             }
         }
     };

             try
             {
                 // Iterate over each RFID tag in the dictionary
                 foreach (var kvp in recommendedProductsDict)
                 {
                     string rfidTag = kvp.Key;

                     // Query the product table to retrieve the product based on the RFID tag
                     var product = await _context.Product
                         .Include(p => p.RecommendedProducts) // Include recommended products
                         .ThenInclude(rp => rp.RecommendedProduct) // Include recommended product details
                         .FirstOrDefaultAsync(p => p.RFIDTag == rfidTag);

                     if (product != null)
                     {
                         // Add the main product to the dictionary under the corresponding RFID tag
                         recommendedProductsDict[rfidTag]["product"].Add(product);

                         // Check if there are recommended products associated with the main product
                         if (product.RecommendedProducts != null && product.RecommendedProducts.Any())
                         {
                             // Fetch recommended products' details
                             var recommendedProducts = await _context.ProductRecommendations
                                 .Include(pr => pr.RecommendedProduct) // Include the recommended product details
                                 .Where(rp => rp.ProductId == product.Id)
                                 .Select(rp => rp.RecommendedProduct) // Select only the recommended product details
                                 .ToListAsync();

                             // Add the recommended products to the dictionary under the corresponding RFID tag
                             recommendedProductsDict[rfidTag]["recommended"].AddRange(recommendedProducts);
                         }
                     }
                 }

                 // Return the dictionary containing product and recommended product details for each RFID tag
                 return Json(new { data= recommendedProductsDict });
               *//*  return Json(new { data = brandsWithProductCount });*//*
             }
             catch (Exception ex)
             {
                 // Handle any exceptions
                 return StatusCode(500, $"An error occurred: {ex.Message}");
             }
         }*/

        #endregion



    }
}
