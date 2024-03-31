
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Fyp.Models;
using Fyp.Utility;
using Fyp.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace FypWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            ViewBag.ProductsCount = _db.Product.Count();
            ViewBag.CategoriesCount = _db.Category.Count();
            ViewBag.BrandsCount = _db.Brand.Count();
            return View();
        }
        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetWeeklyShoppingStatus()
        {
            var startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Sunday);
            var endDate = startDate.AddDays(7);

            try
            {
                // Fetch data first
                var orders = await _db.OrderHeaders
                    .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate)
                    .ToListAsync();

                // Perform grouping and aggregation in memory
                var weeklyStatus = orders
                    .GroupBy(o => o.OrderDate.DayOfWeek)
                    .Select(group => new
                    {
                        DayOfWeek = group.Key.ToString(),
                        TotalSales = group.Sum(o => o.OrderTotal),
                        TotalQuantity = group.Sum(o => o.TotalQuantity)
                    })
                    .OrderBy(result => result.DayOfWeek)
                    .ToList();

                return Ok(weeklyStatus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        #endregion



    }
}