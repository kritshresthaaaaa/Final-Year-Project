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
using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;

namespace FypWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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
            ViewBag.CategoryCount = _context.Category.Count();


            return View();
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
            var isCategoryExist = _context.Category.Any(c => c.CategoryName == categoryDetail.CategoryName);
            if (isCategoryExist)
            {
                ModelState.AddModelError("CategoryName", "Category already exist");
            }
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
        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryID,CategoryName")] CategoryDetail categoryDetail)
        {
            if (id != categoryDetail.CategoryID)
            {
                return NotFound();
            }

            // Check if the brand is associated with any products
            var associatedProductsCount = await _context.Product.CountAsync(p => p.CategoryID == id);
            if (associatedProductsCount > 0)
            {
                // If the brand is associated with products, prevent editing
                ModelState.AddModelError("", "Cannot edit the category because it is associated with products.");

                return View(categoryDetail);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoryDetail);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Category Detail Updated";
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


        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categoriesWithProductCount = await _context.Category
                .Select(category => new
                {
                    category.CategoryID,
                    category.CategoryName,
                    ProductCount = _context.Product.Count(p => p.CategoryID == category.CategoryID)
                })
                .ToListAsync();

            return Json(new { data = categoriesWithProductCount });
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Check if any products are associated with the brand being deleted
            var productsWithCategory = await _context.Product
                .AnyAsync(p => p.CategoryID == id);

            if (productsWithCategory)
            {
                // Prevent deletion if there are associated products
                return Json(new { success = false, message = "Cannot delete the category because it has associated products." });
            }

            // Proceed with deletion if no products are associated
            var categoryDetail = await _context.Category.FindAsync(id);
            if (categoryDetail == null)
            {
                return Json(new { success = false, message = "Category not found." });
            }

            _context.Category.Remove(categoryDetail);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Delete successful" });
        }

        /*     public async Task<IActionResult> GetAll()
             {
                 List<ApplicationUser> usersList = await _context.ApplicationUser.ToListAsync();
                 var userRoles = _context.UserRoles.ToList();
                 var roles = _context
                     .Roles.ToList();
                 foreach (var user in usersList)
                 {
                     var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                     user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
                 }
                 return Json(new { data = usersList }); ;
             }*/
        #endregion

        private bool CategoryDetailExists(int id)
        {
            return _context.Category.Any(e => e.CategoryID == id);
        }
    }
}
