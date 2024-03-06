using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Models.ViewModels;
using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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
        [HttpGet]
        public async Task<IActionResult> Map(string sku)
        {
            ViewBag.SKU = sku;
            if (string.IsNullOrWhiteSpace(sku))
            {
                return BadRequest("SKU is required.");
            }

            var categories = _context.Category.Select(i => new SelectListItem
            {
                Text = i.CategoryName,
                Value = i.CategoryID.ToString()
            }).ToList();

            var brands = _context.Brand.Select(i => new SelectListItem
            {
                Text = i.BrandName,
                Value = i.BrandID.ToString()
            }).ToList();

            var allProducts = await _context.Product.Select(p => new ProductDetailViewModel
            {
                Product = p, // Adjust this based on your ProductDetailViewModel structure
               
                SKU = p.SKU.Code,
                // Map other properties as needed
            }).ToListAsync();

            var skuProducts = await _context.Product
                .Where(p => p.SKU.Code == sku)
                .Select(p => new RecommendationVM
                {
                    Product = p, // Adjust this based on your RecommendationVM structure
                    SKU = sku,
                    // Populate other fields as necessary
                })
                .ToListAsync();

            var viewModel = new MapVM
            {
                SkuProducts = skuProducts,
                Categories = categories,
                Brands = brands,
                AllProducts = allProducts
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(int? categoryId, int? brandId)
        {
            var productsQuery = _context.Product.AsQueryable();

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryID == categoryId.Value);
            }
            else if (brandId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.BrandID == brandId.Value);
            }

            var products = await productsQuery.Select(p => new
            {
                p.Id,
                p.Name,
                p.Category.CategoryName,
                p.Brand.BrandName,
                p.ImageUrl,
                // Include other properties as needed, e.g., ImageUrl, Price
            }).ToListAsync();

            return Json(products);
        }
        [HttpPost]
        public async Task<IActionResult> SubmitSelectedProducts([FromBody] SelectedProductsViewModel submissionData)
        {
            if (submissionData == null || submissionData.SelectedProductIds == null || !submissionData.SelectedProductIds.Any())
            {
                return Json(new { success = false, message = "No products selected." });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var skuProducts = await _context.Product
                                                     .Where(p => p.SKU.Code == submissionData.Sku)
                                                     .ToListAsync();

                    if (!skuProducts.Any())
                    {
                        return Json(new { success = false, message = "No products found with the provided SKU." });
                    }

                    foreach (var skuProduct in skuProducts)
                    {
                        foreach (var recommendedProductId in submissionData.SelectedProductIds)
                        {
                            if (skuProduct.Id != recommendedProductId)
                            {
                                var existingRecommendation = await _context.ProductRecommendations
                                                                            .FirstOrDefaultAsync(pr => pr.ProductId == skuProduct.Id && pr.RecommendedProductId == recommendedProductId);

                                if (existingRecommendation == null)
                                {
                                    var recommendation = new ProductRecommendation
                                    {
                                        ProductId = skuProduct.Id,
                                        RecommendedProductId = recommendedProductId
                                    };

                                    _context.ProductRecommendations.Add(recommendation);
                                }
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "Recommended products added successfully." });
                }
                catch (Exception ex)
                {
                    // Log the exception (adjust logging based on your logging framework/configuration)
                    Console.WriteLine(ex.Message);

                    // Rollback transaction if there was an error
                    await transaction.RollbackAsync();

                    return Json(new { success = false, message = "An error occurred while processing your request." });
                }
            }
        }



        #region API CALLS
        [HttpGet]
        public async Task<JsonResult> GetProductsByCategory(int categoryId)
        {
            var products = await _context.Product
                                          .Where(p => p.CategoryID == categoryId)
                                          .Select(p => new { p.Id, p.Name, p.ImageUrl, p.Price })
                                          .ToListAsync();
            return Json(products);
        }

        [HttpGet]
        public async Task<JsonResult> GetProductsByBrand(int brandId)
        {
            var products = await _context.Product
                                          .Where(p => p.BrandID == brandId)
                                          .Select(p => new { p.Id, p.Name, p.ImageUrl, p.Price })
                                          .ToListAsync();
            return Json(products);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var productWithCategoryandBrand = await _context.Product
                .GroupBy(product => new { product.SKU.Code })
                .Select(group => new
                {
                    SKUCode = group.Key,
                    Products = group.Select(p => new
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

        [HttpGet]
        public async Task<IActionResult> GetCategory()
        {
            var category = await _context.Category.Select(c => new
            {
                c.CategoryID,
                c.CategoryName
            }).ToListAsync();
            return Json(new { data = category });
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDetail>>> GetCategories()
        {
            var categories = await _context.Category
                                           .Select(c => new { c.CategoryID, c.CategoryName })
                                           .ToListAsync();
            return Json(categories);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BrandDetail>>> GetBrands()
        {
            var brands = await _context.Brand
                                       .Select(b => new { b.BrandID, b.BrandName })
                                       .ToListAsync();
            return Json(brands);
        }

        #endregion
    }

}
