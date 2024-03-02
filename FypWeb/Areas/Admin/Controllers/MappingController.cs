using Fyp.DataAccess.Data;
using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FypWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class MappingController : Controller
    {
        private readonly ApplicationDbContext _context;
        public MappingController(ApplicationDbContext context)
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
            var productWithCategoryandBrand = await _context.Product
                .GroupBy(product => new { product.SKU.Code })
                .Select(group => new
                {
                    SKUCode= group.Key,
                    Products= group.Select(p => new
                    {   
                        p.Id,
                        p.Name,
                        p.Category.CategoryName,
                        p.Brand.BrandName,
                        
                    }).ToList()
                   
                 
                })
                .ToListAsync();

            return Json(new { data = productWithCategoryandBrand });
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
