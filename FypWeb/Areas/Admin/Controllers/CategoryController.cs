using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fyp.DataAccess.Data;
using Fyp.Models.ViewModels;
using Fyp.Models;

namespace FypWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var categoriesWithProductCount = await _context.Category
                .Select(c => new CategoryViewModel
                {
                   Category = c,
                    ProductCount = _context.Product.Count(p => p.CategoryID == c.CategoryID)
                })
                .ToListAsync();

            return View(categoriesWithProductCount);
        }



        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Category == null)
            {
                return NotFound();
            }

            var categoryDetail = await _context.Category
                .FirstOrDefaultAsync(m => m.CategoryID == id);
            if (categoryDetail == null)
            {
                return NotFound();
            }

            return View(categoryDetail);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryID,CategoryName")] CategoryDetail categoryDetail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(categoryDetail);
                TempData["success"] = "Category Added";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoryDetail);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Category == null)
            {
                return NotFound();
            }

            var categoryDetail = await _context.Category.FindAsync(id);
            if (categoryDetail == null)
            {
                return NotFound();
            }
            return View(categoryDetail);
        }

        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryID,CategoryName")] CategoryDetail categoryDetail)
        {
            if (id != categoryDetail.CategoryID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoryDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryDetailExists(categoryDetail.CategoryID))
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
            return View(categoryDetail);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Category == null)
            {
                return NotFound();
            }

            var categoryDetail = await _context.Category
                .FirstOrDefaultAsync(m => m.CategoryID == id);
            if (categoryDetail == null)
            {
                return NotFound();
            }

            return View(categoryDetail);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Category == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Category'  is null.");
            }
            var categoryDetail = await _context.Category.FindAsync(id);
            if (categoryDetail != null)
            {
                _context.Category.Remove(categoryDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryDetailExists(int id)
        {
            return _context.Category.Any(e => e.CategoryID == id);
        }
    }
}
