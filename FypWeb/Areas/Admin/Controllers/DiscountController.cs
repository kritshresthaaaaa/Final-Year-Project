using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Models.ViewModels;
using Fyp.Utility;
using FypWeb.Services;
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
        private readonly IDiscountService _discountService;
        public DiscountController(ApplicationDbContext context, IDiscountService discountService)
        {

            _db = context;
            _discountService = discountService;
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
            var today = DateTime.Today;
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
            // Check if the start date is equal to the end date
            if (today == model.Discount.EndDate.Date)
            {
                ViewBag.Error = "The start date and end date cannot be the same.";

                return View(model);
            }
            bool existsOverlap = await _db.Discount.AnyAsync(d =>
         d.IsActive &&
         ((model.SelectedCategoryId.HasValue && d.CategoryID == model.SelectedCategoryId) ||
          (model.SelectedBrandId.HasValue && d.BrandID == model.SelectedBrandId) ||
          (model.SelectedSKUID.HasValue && d.SKUID == model.SelectedSKUID)) && // Check for SKU overlap
         (d.StartDate < model.Discount.EndDate && d.EndDate > model.Discount.StartDate));
            if (existsOverlap)
            {
                ViewBag.ErrorMessage = "An overlapping discount for the selected category/brand and date range already exists.";

                return View(model);
            }

            var discountDetail = new DiscountDetail
            {
                Name = model.Discount.Name,
                Percentage = model.Discount.Percentage,
                StartDate = today,
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
                // Only update products with the specific SKUID
                productsToUpdate = await _db.Product.Where(p => p.SKUID == model.SelectedSKUID).ToListAsync();
            }

            foreach (var product in productsToUpdate)
            {
                if (product.SKUID == model.SelectedSKUID) // Ensure only products with the specific SKUID are affected
                {
                    double discountedPrice = product.Price * (1 - (double)model.Discount.Percentage / 100);
                    product.DiscountedPrice = Math.Floor(discountedPrice * 100) / 100; // Floor the number to two decimal places without rounding up
                }
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
        /*   [HttpPost]
           public async Task<IActionResult> UpdateExpiredDiscounts()
           {
               var today = DateTime.Today;

               // Query to find discounts that have expired but are still marked as active.
               var expiredDiscounts = await _db.Discount
                   .Where(d => d.EndDate < today && d.IsActive)
                   .ToListAsync();

               foreach (var discount in expiredDiscounts)
               {
                   discount.IsActive = false;
               }

               if (expiredDiscounts.Any())
               {
                   _db.Discount.UpdateRange(expiredDiscounts);
                   await _db.SaveChangesAsync();
               }

               return Ok(new { message = "Expired discounts updated successfully." });
           }*/
        [HttpPost]
        public async Task<IActionResult> UpdateExpiredDiscounts()
        {
            try
            {
                await _discountService.UpdateExpiredDiscountsAsync();
                return Ok(new { message = "Expired discounts updated successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "An error occurred while updating expired discounts.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllDiscounts()
        {
            var today = DateTime.Today;

            var discounts = await _db.Discount
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.Percentage,
                    StartDate = d.StartDate.ToString("yyyy-MM-dd"), // Assuming StartDate is not nullable
                    EndDate = d.EndDate != null ? d.EndDate.ToString("yyyy-MM-dd") : null, // Handling nullable EndDate
                    d.IsActive,
                    CategoryName = d.CategoryID != null ? d.Category.CategoryName : null,
                    BrandName = d.BrandID != null ? d.Brand.BrandName : null,
                    SKUCode = d.SKUID != null ? d.SKU.Code : null
                })
                .ToListAsync();

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
            if(model.IsActive && discount.EndDate < DateTime.Now)
            {
                return Json(new { success = false, message = "Cannot activate an expired discount." });
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