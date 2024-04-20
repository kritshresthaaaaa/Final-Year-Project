using Fyp.DataAccess.Data;
using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace FypWeb.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = SD.Role_Employee)]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var employeeIdStr = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid employeeId;

            if (!Guid.TryParse(employeeIdStr, out employeeId))
            {
                return BadRequest("Invalid user ID.");
            }

            // Total Sales Amount by the Employee
            var totalSales = await _db.OrderHeaders
                .Where(o => o.ApplicationUserId == employeeIdStr)
                .SumAsync(o => o.OrderTotal);

            // Total Number of Transactions by the Employee
            var totalTransactions = await _db.OrderHeaders
                .CountAsync(o => o.ApplicationUserId == employeeIdStr);

            // Assuming there's an OrderDetails table that includes a Quantity for each item in an order
            // and assuming there's a relationship between OrderHeaders and OrderDetails
            var totalItemsSold = await _db.OrderDetails
                .Where(od => _db.OrderHeaders.Any(oh => oh.Id == od.OrderHeaderId && oh.ApplicationUserId == employeeIdStr))
                .SumAsync(od => od.Quantity);

            // Pass the data to the view via ViewBag
            ViewBag.TotalSales = totalSales;
            ViewBag.TotalTransactions = totalTransactions;
            ViewBag.TotalItemsSold = totalItemsSold;

            return View();
        }
        private double CalculateDiscountedPrice(double originalPrice, decimal discountPercentage)
        {
            return originalPrice * (1 - (double)discountPercentage / 100);
        }
        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var today = DateTime.Today;
            var productsWithDiscounts = await _db.Product
                .Include(p => p.Category)
                .Select(p => new
                {
                    Product = p,
                    Discount = _db.Discount
                        .Where(d => d.IsActive && d.StartDate <= today && today <= d.EndDate &&
                                    (d.CategoryID == null || d.CategoryID == p.CategoryID) &&
                                    (d.BrandID == null || d.BrandID == p.BrandID))
                        .OrderByDescending(d => d.Percentage) // Assuming you want the highest discount if multiple apply
                        .FirstOrDefault()
                })
                .ToListAsync();

            var productViewModels = productsWithDiscounts.Select(pd => new
            {
                Id = pd.Product.Id,
                Name = pd.Product.Name,
                Category = pd.Product.Category.CategoryName,
                Price = pd.Product.Price,
                DiscountedPrice = pd.Discount != null ? CalculateDiscountedPrice(pd.Product.Price, pd.Discount.Percentage) : pd.Product.Price,
                IsActiveDiscount = pd.Discount != null,
                DiscountStartDate = pd.Discount?.StartDate,
                DiscountEndDate = pd.Discount?.EndDate
            }).ToList();

            return Json(new { data = productViewModels });
        }


        [HttpGet]
        public async Task<IActionResult> GetWeeklyShoppingStatus()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var employeeIdStr = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;    
            var startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Sunday);
            var endDate = startDate.AddDays(7);

            try
            {
                // Fetch data first
                var orders = await _db.OrderHeaders
             .Where(o => o.ApplicationUserId == employeeIdStr && o.OrderDate >= startDate && o.OrderDate < endDate)
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
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var employeeIdStr = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
    
            try
            {
                var orders = await _db.OrderHeaders
                      .Where(o => o.ApplicationUserId == employeeIdStr && o.OrderDate.Year == year)
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
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var employeeIdStr = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var allDays = Enumerable.Range(1, daysInMonth).Select(day => new DateTime(year, month, day));

            try
            {
                var salesData = await _db.OrderHeaders
                          .Where(o => o.ApplicationUserId == employeeIdStr && o.OrderDate.Year == year && o.OrderDate.Month == month)
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
