using Fyp.DataAccess.Data;
using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FypWeb.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = SD.Role_Employee)]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ProductController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }
        private double CalculateDiscountedPrice(double originalPrice, decimal discountPercentage)
        {
            return originalPrice * (1 - (double)discountPercentage / 100);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _db.Product == null)
            {
                return NotFound();
            }

            var today = DateTime.Today;
            var productDetail = await _db.Product
                .Include(p => p.SKU)
                .ThenInclude(sku => sku.Products)
                .Include(p => p.Brand) // Include the Brand
                .Include(p => p.Category) // Include the Category
                .Select(p => new
                {
                    Product = p,
                    Discount = _db.Discount
                        .Where(d => d.IsActive && d.StartDate <= today && today <= d.EndDate &&
                                    (d.CategoryID == null || d.CategoryID == p.CategoryID) &&
                                    (d.BrandID == null || d.BrandID == p.BrandID))
                        .OrderByDescending(d => d.Percentage)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(m => m.Product.Id == id);

            if (productDetail == null || productDetail.Product == null)
            {
                return NotFound();
            }

            // Map to ViewBag to pass additional data to the view
            ViewBag.TotalProductsInSKU = productDetail.Product.SKU?.Products?.Count() ?? 0;
            ViewBag.ProductColor = productDetail.Product.ColorCode;
            ViewBag.DiscountedPrice = productDetail.Discount != null ?
                CalculateDiscountedPrice(productDetail.Product.Price, productDetail.Discount.Percentage) :
                productDetail.Product.Price;
            ViewBag.IsActiveDiscount = productDetail.Discount != null;
            ViewBag.DiscountPercentage = productDetail.Discount?.Percentage;
            ViewBag.BrandName = productDetail.Product.Brand?.BrandName; // Assuming Brand has a Name property
            ViewBag.CategoryName = productDetail.Product.Category?.CategoryName; // Assuming Category has a Name property

            return View(productDetail.Product);
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

        #endregion
    }
}
