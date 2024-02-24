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
            List<CategoryDetail> objCategoryList = _db.Category.ToList();
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
            List<SKUDetail> objSKUList = _db.SKU.ToList();
            IEnumerable<SelectListItem> SKUList = _db.SKU.Select(i => new SelectListItem
            {
                Text = i.Code,
                Value = i.SKUID.ToString()
            });
            DiscountVM discountVM = new DiscountVM()
            {
                Discount = new DiscountDetail(),
                CategoryList = CategoryList,
                BrandList = BrandList,
                SKUList = SKUList

            };

            return View(discountVM);
        }
        /*  [HttpPost]
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
          }*/
        [HttpPost]
        public async Task<IActionResult> Create(DiscountVM model)
        {

            model.CategoryList = _db.Category.Select(i => new SelectListItem
            {
                Text = i.CategoryName,
                Value = i.CategoryID.ToString()
            }).ToList();

            model.BrandList = _db.Brand.Select(i => new SelectListItem
            {
                Text = i.BrandName,
                Value = i.BrandID.ToString()
            }).ToList();
            model.SKUList = _db.SKU.Select(i => new SelectListItem
            {
                Text = i.Code,
                Value = i.SKUID.ToString()
            }).ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool existsOverlap = await _db.Discount.AnyAsync(d =>
         d.IsActive &&
         ((model.SelectedCategoryId.HasValue && d.CategoryID == model.SelectedCategoryId) ||
          (model.SelectedBrandId.HasValue && d.BrandID == model.SelectedBrandId) ||
          (model.SelectedSKUID.HasValue && d.SKUID == model.SelectedSKUID)) && // Check for SKU overlap
         (d.StartDate <= model.Discount.EndDate && d.EndDate >= model.Discount.StartDate));
            if (existsOverlap)
            {
                ViewBag.ErrorMessage = "An overlapping discount for the selected category/brand and date range already exists.";

                return View(model);
            }

            var discountDetail = new DiscountDetail
            {
                Name = model.Discount.Name,
                Percentage = model.Discount.Percentage,
                StartDate = DateTime.Today,
                EndDate = model.Discount.EndDate,
                IsActive = model.Discount.IsActive,
                CategoryID = model.SelectedCategoryId,
                BrandID = model.SelectedBrandId,
                SKUID = model.SelectedSKUID
            };

            _db.Discount.Add(discountDetail);
            await _db.SaveChangesAsync();

            // Assuming you want to apply the discount to products based on CategoryID, BrandID, or SKUID
            var productsToUpdate = new List<ProductDetail>();
            if (model.SelectedCategoryId.HasValue)
            {
                productsToUpdate = await _db.Product.Where(p => p.CategoryID == model.SelectedCategoryId).ToListAsync();
            }
            else if (model.SelectedBrandId.HasValue)
            {
                productsToUpdate = await _db.Product.Where(p => p.BrandID == model.SelectedBrandId).ToListAsync();
            }
            else if (model.SelectedSKUID.HasValue)
            {
                productsToUpdate = await _db.Product.Where(p => p.SKUID == model.SelectedSKUID).ToListAsync();
            }
            
            // Calculate and set the discounted price
            foreach (var product in productsToUpdate)
            {          
                product.DiscountedPrice = product.Price * (1 - (double)model.Discount.Percentage / 100);
            }


            try
            {
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while saving the discount. Please try again.");
                return View(model);
            }
        }
        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAllDiscounts()
        {
            var discounts = await _db.Discount.Select(d => new
            {
                d.Id,
                d.Name,
                d.Percentage,
                d.StartDate,
                d.EndDate,
                d.IsActive,
                CategoryName = d.CategoryID.HasValue ? d.Category.CategoryName : null,
                BrandName = d.BrandID.HasValue ? d.Brand.BrandName : null,
                SKUCode = d.SKUID.HasValue ? d.SKU.Code : null
            }).ToListAsync();
            return Json(new { data = discounts });
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var discountDetail = await _db.Discount.FindAsync(id);
            if (discountDetail != null)
            {
                _db.Discount.Remove(discountDetail);
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Delete successful" });
            }
            return Json(new { success = false, message = "Delete failed" });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus([FromBody] ToggleStatusModel model)
        {
            // Find the discount by ID
            var discount = await _db.Discount.FindAsync(model.Id);
            if (discount == null)
            {
                return Json(new { success = false, message = "Discount not found." });
            }

            // Toggle the active status
            discount.IsActive = model.IsActive;

            // Save changes to the database
            try
            {
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = $"Discount has been {(model.IsActive ? "activated" : "deactivated")}." });
            }
            catch (Exception ex)
            {
                // Log the exception, if logging is set up
                // Log.Error(ex, "Failed to toggle discount status.");
                return Json(new { success = false, message = "An error occurred while updating the discount status." });
            }
        }


        public class ToggleStatusModel
        {
            public int Id { get; set; }
            public bool IsActive { get; set; }
        }

        #endregion
    }
}