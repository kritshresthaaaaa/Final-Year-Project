using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Utility;
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
        public Home(ApplicationDbContext context,UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View("Index", "_Customers");
        }
        #region API CALLS
        public class ProductWithSizes
        {
            public ProductDetail Product { get; set; }
            public List<string> Sizes { get; set; } = new List<string>();
        }
        public class SizeRequest
        {
            public int ProductId { get; set; }
            public string Size { get; set; }
        }

        [HttpGet]
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
                var employeeRelationId= currentUser.EmployeeRelationId;
       
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
                    ToEmployeeId= new Guid( relatedUser.Id),
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

