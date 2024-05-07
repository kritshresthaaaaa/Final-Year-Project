
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Fyp.Models;
using Fyp.Utility;
using Fyp.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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

        public async Task<IActionResult> Index()
        {
            // Count employees with role "employee" using a more efficient approach
            var employeeCount = await _db.UserRoles.CountAsync(ur => ur.RoleId == _db.Roles.FirstOrDefault(r => r.Name == SD.Role_Employee ).Id);
            var totalItemsSold = await _db.OrderDetails.Where(od => _db.OrderHeaders.Any(oh => oh.Id == od.OrderHeaderId)).SumAsync(od => od.Quantity);
            ViewBag.ProductsCount = _db.Product.Count();
            ViewBag.CategoriesCount = _db.Category.Count();
            ViewBag.BrandsCount = _db.Brand.Count();
            ViewBag.EmployeesCount = employeeCount;
            ViewBag.RevenueCount = _db.OrderHeaders.Sum(o => o.OrderTotal);
            ViewBag.SoldCount = totalItemsSold;
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
        [HttpGet]
        public async Task<IActionResult> GetYearlyShoppingStatus(int year)
        {
            try
            {
                var orders = await _db.OrderHeaders
                    .Where(o => o.OrderDate.Year == year)
                    .ToListAsync();

                var yearlyStatus = orders
                    .GroupBy(o => o.OrderDate.Month)
                    .Select(group => new
                    {
                        Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(group.Key), // Convert month number to name
                        TotalSales = group.Sum(o => o.OrderTotal),
                        TotalQuantity = group.Sum(o => o.TotalQuantity)
                    })
                    .OrderBy(result => DateTime.ParseExact(result.Month, "MMMM", CultureInfo.CurrentCulture)) // Order by month name properly
                    .ToList();

                // Optionally, ensure all months are represented, even if there are no sales
                var allMonths = Enumerable.Range(1, 12).Select(month => new
                {
                    Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                    TotalSales = yearlyStatus.FirstOrDefault(m => m.Month == CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month))?.TotalSales ?? 0,
                    TotalQuantity = yearlyStatus.FirstOrDefault(m => m.Month == CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month))?.TotalQuantity ?? 0
                });

                return Ok(allMonths);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMonthlyShoppingStatus(int year, int month)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var allDays = Enumerable.Range(1, daysInMonth).Select(day => new DateTime(year, month, day));

            try
            {
                var salesData = await _db.OrderHeaders
                    .Where(o => o.OrderDate.Year == year && o.OrderDate.Month == month)
                    .GroupBy(o => o.OrderDate.Day)
                    .Select(group => new
                    {
                        Day = group.Key,
                        TotalSales = group.Sum(o => o.OrderTotal)
                    })
                    .ToListAsync();

                var dailySales = from day in allDays
                                 join sale in salesData on day.Day equals sale.Day into salesFromDay
                                 from sale in salesFromDay.DefaultIfEmpty()
                                 select new
                                 {
                                     Day = day.Day,
                                     TotalSales = sale != null ? sale.TotalSales : 0
                                 };

                return Ok(dailySales);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion



    }
}