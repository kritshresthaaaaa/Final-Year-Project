using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Models.ViewModels;
using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FypWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: HomeController1
        public ActionResult Index()
        {
            var inventoryList = _context.Product
                .GroupBy(p => new { p.Name, p.Sizes, p.Price, p.Category.CategoryName })
                .Select(group => new InventoryViewModel
                {
                    ProductName = group.Key.Name,
                    Size = group.Key.Sizes,
                    Price = group.Key.Price,
                    Stock = group.Count(),
                    CategoryName = group.Key.CategoryName

                })
                .ToList();

            return View(inventoryList);
        }


        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int stockThreshold = await GetUserStockThreshold();

            var inventoryList = await _context.Product
                .GroupBy(p => new { p.Name, p.Sizes, p.Price, p.Category.CategoryName, p.Brand.BrandName })
                .Select(group => new InventoryViewModel
                {
                    Id = group.Select(p => p.Id).FirstOrDefault(),
                    ProductName = group.Key.Name,
                    Size = group.Key.Sizes,
                    Price = group.Key.Price,
                    Stock = group.Count(),
                    CategoryName = group.Key.CategoryName,
                    BrandName = group.Key.BrandName,
                    // Determine stock status based on threshold
                    StockStatus = group.Count() <= stockThreshold ? "Low" : "In Stock"
                })
                .ToListAsync();

            return Json(new { data = inventoryList });
        }

        #endregion
        private async Task<int> GetUserStockThreshold()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get user ID from claims
            var applicationUser = await _context.ApplicationUser.FirstOrDefaultAsync(u => u.Id == userId);
            return applicationUser?.StockAlerter ?? 0; // Assuming 'StockAlerter' is an int property in your User model
        }
      
    }
}
