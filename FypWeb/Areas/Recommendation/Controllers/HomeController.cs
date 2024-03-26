using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Utility;
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

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
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
        #region API CALLS
      
        [HttpGet]
        public async Task<IActionResult> GetAllRecommendedItems()
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
              /*  return Json(new { data = brandsWithProductCount });*/
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        #endregion



    }
}
