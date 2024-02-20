using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Models.ViewModels;
using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FypWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class DiscountController : Controller
    {
        private readonly ApplicationDbContext _db;
        public DiscountController(ApplicationDbContext context)
        {
            
            _db = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create()
        {
            List<CategoryDetail>objCategoryList= _db.Category.ToList();
            IEnumerable<SelectListItem> CategoryList = _db.Category.Select(i => new SelectListItem
            {
                Text = i.CategoryName,
                Value = i.CategoryID.ToString()
            });
            List<BrandDetail> objBrandList = _db.Brand.ToList();
            IEnumerable<SelectListItem> BrandList = _db.Brand.Select(i => new SelectListItem
            {
                Text = i.BrandName,
                Value = i.BrandID.ToString()
            });
            DiscountVM discountVM = new DiscountVM()
            {
                Discount = new DiscountDetail(),
                CategoryList = CategoryList,
              
            };
         
            return View(discountVM);
        }
        [HttpPost]
        public async Task<IActionResult> Create(DiscountVM model)
        {
            // Populate CategoryList and BrandList no matter what
            model.CategoryList = _db.Category.Select(i => new SelectListItem
            {
                Text = i.CategoryName,
                Value = i.CategoryID.ToString()
            });
            model.BrandList = _db.Brand.Select(i => new SelectListItem
            {
                Text = i.BrandName,
                Value = i.BrandID.ToString()
            });
            if (!ModelState.IsValid)
            {          

                // Return to the view if validation failed to show error messages
                return View(model);
            }
            // Directly creating and saving the DiscountDetail entity without model state validation
            var discountDetail = new DiscountDetail
            {
                Name = model.Discount.Name,
                Percentage = model.Discount.Percentage,
               
                StartDate = model.Discount.StartDate,
                EndDate = model.Discount.EndDate,
                IsActive = model.Discount.IsActive
            };

            // Set foreign key properties based on DiscountFor selection
            switch (model.DiscountFor)
            {
                case "Categories":
                    discountDetail.CategoryID = model.SelectedCategoryId;
                    break;
                case "Brands":
                    discountDetail.BrandID = model.SelectedBrandId;
                    break;
                    // Optionally handle "Products" case
            }

            // Add the new DiscountDetail entity to the DbSet
            _db.Discount.Add(discountDetail);

            try
            {
            
                await _db.SaveChangesAsync();
          
                if (model.SelectedCategoryId.HasValue)
                {
                    var productsInCategory = await _db.Product
                        .Where(p => p.CategoryID == model.SelectedCategoryId.Value)
                        .ToListAsync();

                    foreach (var product in productsInCategory)
                    {
                        product.Price *= (1 - Convert.ToDouble(model.Discount.Percentage) / 100.0);
                    }
                }
             
                if (model.SelectedBrandId.HasValue)
                {
                    var productsInBrand = await _db.Product
                        .Where(p => p.BrandID == model.SelectedBrandId.Value)
                        .ToListAsync();

                    foreach (var product in productsInBrand)
                    {
                        product.Price *= (1 - Convert.ToDouble(model.Discount.Percentage) / 100.0);
                    }
                }
                // Save the product price changes to the database
                if (model.SelectedCategoryId.HasValue || model.SelectedBrandId.HasValue)
                {
                    await _db.SaveChangesAsync();
                }

                // Redirect upon successful save
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while saving the discount. Please try again.");
                return View(model);
            }
        }



        #region API CALLS
        public IActionResult GetAllBrands()
        {
           return Json(new { data = _db.Brand.ToList() });
        }
        public IActionResult GetAllCategories()
        {
            return Json(new { data = _db.Category.ToList() });
        }
        #endregion
    }
}
