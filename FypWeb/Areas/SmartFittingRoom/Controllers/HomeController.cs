﻿using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Utility;
using FypWeb.Areas.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static FypWeb.Areas.SmartFittingRoom.Controllers.Home;

namespace FypWeb.Areas.SmartFittingRoom.Controllers
{
    [Area("SmartFittingRoom")]
    [Authorize(Roles = SD.Role_Customer_Handler)]
    public class Home : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private Reader _reader;
        public Home(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _reader = new Reader("192.168.1.1", false);
        }

        public IActionResult Index()
        {
            return View("Index", "_Customers");
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
        public class ProductWithSizes
        {
            public ProductDetail Product { get; set; }
            public HashSet<string> Sizes { get; set; }
        }

        public class SizeRequest
        {
            public int ProductId { get; set; }
            public string Size { get; set; }
        }

        /* [HttpGet]
         public async Task<IActionResult> GetProductsByRfidTagsWithSizes()
         {
             var productsDict = new Dictionary<string, ProductWithSizes>();

             try
             {
                 var rfids = new List<string> { "123456", "123457" };

                 foreach (var rfidTag in rfids)
                 {
                     var products = await _context.Product
                         .Include(p => p.SKU)
                         .Where(p => p.RFIDTag == rfidTag)
                         .ToListAsync();

                     foreach (var product in products)
                     {
                         // Assuming the base SKU is the part before the last '-' character
                         var baseSku = product.SKU.Code.Substring(0, product.SKU.Code.LastIndexOf('-'));

                         if (!productsDict.ContainsKey(baseSku))
                         {
                             productsDict[baseSku] = new ProductWithSizes { Product = product };
                         }

                         // Assuming size is the last character after the last '-' character
                         productsDict[baseSku].Sizes.Add(product.SKU.Code.Split('-').Last());
                     }
                 }

                 var responseData = productsDict.Select(kvp => new
                 {
                     BaseSKU = kvp.Key,
                     Product = kvp.Value.Product,
                     Sizes = kvp.Value.Sizes // Distinct not needed due to HashSet
                 });

                 return Json(new { data = responseData });
             }
             catch (Exception ex)
             {
                 return StatusCode(500, $"An error occurred: {ex.Message}");
             }
         }
 */

        [HttpGet]
        public async Task<IActionResult> GetProductsByRfidTagsWithSizes()
        {
            var rfidsTags = await ReadTags();
            var productsDict = new Dictionary<string, ProductWithSizes>();

            // List of RFIDs
            if (rfidsTags == null || rfidsTags.Count == 0)
            {
                return Json(new { success = false, message = "No RFID tags detected." });
            }

            try
            {
                foreach (var rfidTag in rfidsTags)
                {
                    var products = await _context.Product
                        .Include(p => p.SKU)
                        .Where(p => p.RFIDTag == rfidTag)
                        .ToListAsync();

                    foreach (var product in products)
                    {
                        var baseSku = product.SKU.Code.Substring(0, product.SKU.Code.LastIndexOf('-'));

                        if (!productsDict.ContainsKey(baseSku))
                        {
                            productsDict[baseSku] = new ProductWithSizes { Product = product, Sizes = new HashSet<string>() };
                        }

                        productsDict[baseSku].Sizes.Add(product.SKU.Code.Split('-').Last());
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return Json(new { success = false, message = "An error occurred while processing RFID tags.", error = ex.Message });
            }

            var responseData = productsDict.ToDictionary(
                kvp => kvp.Key,
                kvp => new { Product = kvp.Value.Product, Sizes = kvp.Value.Sizes });

            return Json(new { success = true, data = responseData });
        }


        /*
                [HttpGet]
                public async Task<IActionResult> GetProductDetailsWithAllSizes(string baseSku)
                {
                    // Fetch all products that have SKUs starting with the provided base SKU.
                    var productsWithBaseSku = await _context.Product
                        .Include(p => p.SKU)
                        .Where(p => p.SKU.Code.StartsWith(baseSku))
                        .ToListAsync();

                    // Assuming the size is the last segment after the last '-' in the SKU code
                    // and that the rest of the SKU code before the last '-' is the base SKU.
                    var sizes = productsWithBaseSku
                        .Select(p => p.SKU.Code.Split('-').Last())
                        .Distinct()
                        .ToList();

                    // Assuming we want to return the first product's details as a representative for the base SKU
                    var representativeProduct = productsWithBaseSku.FirstOrDefault();

                    if (representativeProduct == null)
                    {
                        return NotFound("Product not found.");
                    }

                    var responseData = new
                    {
                        BaseSKU = baseSku,
                        Product = new
                        {
                            representativeProduct.Id,
                            representativeProduct.Name,
                            representativeProduct.Description,
                            representativeProduct.Price,
                            representativeProduct.DiscountedPrice,
                            representativeProduct.BrandID,
                            representativeProduct.CategoryID,
                            SKUCode = representativeProduct.SKU.Code,
                            representativeProduct.ImageUrl,
                            representativeProduct.ColorCode
                        },
                        Sizes = sizes // List of all sizes for the base SKU
                    };

                    return Json(new { data = responseData });
                }*/

        public async Task<IActionResult> GetProductDetailsWithAllSizes(string baseSku)
        {
            // Fetch all products that have SKUs starting with the provided base SKU.
            var productsWithBaseSku = await _context.Product
                .Include(p => p.SKU)
                .Where(p => p.SKU.Code.StartsWith(baseSku))
                .ToListAsync();

            if (!productsWithBaseSku.Any())
            {
                return NotFound("No products found for the provided base SKU.");
            }

            // Assuming the size is the last segment after the last '-' in the SKU code.
            var sizes = productsWithBaseSku
                .Select(p => p.SKU.Code.Split('-').Last())
                .Distinct()
                .ToList();

            // Assuming we want to return the first product's details as a representative for the base SKU.
            var representativeProduct = productsWithBaseSku.FirstOrDefault();

            if (representativeProduct == null)
            {
                return NotFound("Product not found.");
            }

            // Fetch today's date for discount validation
            var today = DateTime.Today;

            // Query for applicable discount for the representative product
            var discount = _context.Discount
                .Where(d => d.IsActive && today >= d.StartDate && today <= d.EndDate &&
                            (d.CategoryID == null || d.CategoryID == representativeProduct.CategoryID) &&
                            (d.BrandID == null || d.BrandID == representativeProduct.BrandID))
                .OrderByDescending(d => d.Percentage)
                .FirstOrDefault();

            // Calculate the discounted price for the representative product
            var discountedPrice = representativeProduct.DiscountedPrice;
            if (discount != null)
            {
                discountedPrice = representativeProduct.Price * (1 - (double)discount.Percentage / 100.0);
            }

            var responseData = new
            {
                BaseSKU = baseSku,
                Product = new
                {
                    Id = representativeProduct.Id,
                    Name = representativeProduct.Name,
                    Description = representativeProduct.Description,
                    Price = representativeProduct.Price,
                    DiscountedPrice = discountedPrice,
                    BrandID = representativeProduct.BrandID,
                    CategoryID = representativeProduct.CategoryID,
                    SKUCode = representativeProduct.SKU.Code,
                    ImageUrl = representativeProduct.ImageUrl,
                    ColorCode = representativeProduct.ColorCode
                    // Add more product details as needed
                },
                Sizes = sizes // List of all sizes for the base SKU
            };

            return Json(new { data = responseData });
        }

        [HttpGet]
        public async Task<IActionResult> IsNotificationRead(int notificationId)
        {
            var notification = await _context.Notification.FindAsync(notificationId);
            if (notification == null)
            {
                return NotFound();
            }

            return Ok(new { isRead = notification.IsRead });
        }

        [HttpPost]
        public async Task<IActionResult> PostSizeRequest([FromBody] SizeRequest request)
        {
            if (request == null || request.ProductId <= 0 || string.IsNullOrWhiteSpace(request.Size))
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                // Here you would handle the request, e.g., saving it to the database
                // This is a placeholder for your logic

                // Example logic: Find the product by ID and then do something with the size
                var product = await _context.Product.FindAsync(request.ProductId);
                if (product == null)
                {
                    return NotFound($"Product with ID {request.ProductId} not found.");
                }
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var employeeId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                var currentUser = await _userManager.FindByIdAsync(employeeId);
                var employeeRelationId = currentUser.EmployeeRelationId;

                if (currentUser == null)
                {
                    return NotFound("Employee not found.");

                }
                // Now use the EmployeeRelationId to find another user who has the same EmployeeRelationId but a different user ID
                var relatedUser = await _context.Users
     .OfType<ApplicationUser>() // Ensure you're working with the extended ApplicationUser instances
     .FirstOrDefaultAsync(u => u.EmployeeRelationId == currentUser.EmployeeRelationId && u.Id != currentUser.Id);


                if (relatedUser == null)
                {
                    return NotFound("Related employee not found.");
                }

                Noti notification = new Noti
                {
                    FromRoomId = 1,
                    FromRoomName = "Smart Fitting Room",
                    ToEmployeeName = relatedUser.UserName,
                    ToEmployeeId = new Guid(relatedUser.Id),
                    NotiHeader = "Size Request",
                    NotiBody = $"Size request for product {product.Name} (ID: {product.Id}) with size {request.Size} received.",
                    IsRead = false,
                    Url = $"/products/details/{product.Id}",
                    CreatedDate = DateTime.Now,
                    Message = $"Request for size '{request.Size}'"
                };
                _context.Notification.Add(notification);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Size request processed successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }
    }




    #endregion
}
