using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Models.ViewModels;
using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
  /*          var inventoryList = _context.Product
                .GroupBy(p => new { p.Name, p.Sizes, p.Price, p.Category.CategoryName })
                .Select(group => new InventoryViewModel
                {
                    ProductName = group.Key.Name,
                    Size = group.Key.Sizes,
                    Price = group.Key.Price,
                    Stock = group.Count(),
                    CategoryName = group.Key.CategoryName

                })
                .ToList();*/

            return View();
        }

        // GET: HomeController1/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: HomeController1/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: HomeController1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeController1/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: HomeController1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeController1/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }
        #region API CALLS
        public async Task<IActionResult> GetAll()
        {
            var inventoryList = await _context.Product
               .GroupBy(p => new { p.Name, p.Sizes, p.Price, p.Category.CategoryName })
               .Select(group => new InventoryViewModel
               {
                   ProductName = group.Key.Name,
                   Size = group.Key.Sizes,
                   Price = group.Key.Price,
                   Stock = group.Count(),
                   CategoryName = group.Key.CategoryName
               })
               .ToListAsync(); // Corrected to use await with ToListAsync

            return Json(new { data = inventoryList });
        }
        #endregion



        // POST: HomeController1/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
