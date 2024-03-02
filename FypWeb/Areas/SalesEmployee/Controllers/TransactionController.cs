using Fyp.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FypWeb.Areas.SalesEmployee.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
            
        }
        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var brandsWithProductCount = await _context.Brand
                .Select(brand => new
                {
                    brand.BrandID,
                    brand.BrandName,
                    ProductCount = _context.Product.Count(p => p.BrandID == brand.BrandID)
                })
                .ToListAsync();

            return Json(new { data = brandsWithProductCount });
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var brandDetail = await _context.Brand.FindAsync(id);
            if (brandDetail != null)
            {
                _context.Brand.Remove(brandDetail);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Delete successful" });
            }
            return Json(new { success = false, message = "Delete failed" });
        }
        #endregion
    }
}
