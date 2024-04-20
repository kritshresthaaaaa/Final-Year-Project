using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;

namespace FypWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class BrandController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BrandController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Brand
        public async Task<IActionResult> Index()
        {
            ViewBag.BrandCount = _context.Brand.Count();
            return View();
        }
        public async Task<IActionResult> GetItemCountByCreationDate()
        {
            var itemCountByDate = await _context.Brand
                                   .GroupBy(b => b.CreationDate.Date)
                                   .Select(group => new
                                   {
                                       CreationDate = group.Key,
                                       Count = group.Count()
                                   })
                                   .ToListAsync();

            return Json(itemCountByDate);
        }
        // GET: Brand/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Brand == null)
            {
                return NotFound();
            }

            var brandDetail = await _context.Brand
                .FirstOrDefaultAsync(m => m.BrandID == id);
            if (brandDetail == null)
            {
                return NotFound();
            }

            return View(brandDetail);
        }

        // GET: Brand/Create
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BrandID,BrandName")] BrandDetail brandDetail)
        {
            var isBrandExist = _context.Brand.Any(c => c.BrandName == brandDetail.BrandName);
            if (isBrandExist)
            {
                ModelState.AddModelError("BrandName", "Brand already exist");
            }
            if (ModelState.IsValid)
            {
                brandDetail.CreationDate = DateTime.Now;
                _context.Add(brandDetail);
                TempData["success"] = "Brand Added";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(brandDetail);
        }

        // GET: Brand/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Brand == null)
            {
                return NotFound();
            }

            var brandDetail = await _context.Brand.FindAsync(id);
            if (brandDetail == null)
            {
                return NotFound();
            }

            return View(brandDetail);
        }

        // POST: Brand/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BrandID,BrandName")] BrandDetail brandDetail)
        {
            if (id != brandDetail.BrandID)
            {
                return NotFound();
            }

            // Check if the brand is associated with any products
            var associatedProductsCount = await _context.Product.CountAsync(p => p.BrandID == id);
            if (associatedProductsCount > 0)
            {
                // If the brand is associated with products, prevent editing
                ModelState.AddModelError("", "Cannot edit the brand because it is associated with products.");
         
                return View(brandDetail);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(brandDetail);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Brand Detail Updated";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrandDetailExists(brandDetail.BrandID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(brandDetail);
        }

        // GET: Brand/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Brand == null)
            {
                return NotFound();
            }

            var brandDetail = await _context.Brand
                .FirstOrDefaultAsync(m => m.BrandID == id);
            if (brandDetail == null)
            {
                return NotFound();
            }

            return View(brandDetail);
        }

        // POST: Brand/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Brand == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Brand'  is null.");
            }
            var brandDetail = await _context.Brand.FindAsync(id);
            if (brandDetail != null)
            {
                _context.Brand.Remove(brandDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
            // Check if any products are associated with the brand being deleted
            var productsWithBrand = await _context.Product
                .AnyAsync(p => p.BrandID == id);

            if (productsWithBrand)
            {
                // Prevent deletion if there are associated products
                return Json(new { success = false, message = "Cannot delete the brand because it has associated products." });
            }

            // Proceed with deletion if no products are associated
            var brandDetail = await _context.Brand.FindAsync(id);
            if (brandDetail == null)
            {
                return Json(new { success = false, message = "Brand not found." });
            }

            _context.Brand.Remove(brandDetail);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Delete successful" });
        }

        #endregion
        private bool BrandDetailExists(int id)
        {
            return _context.Brand.Any(e => e.BrandID == id);
        }
        
    }
}
