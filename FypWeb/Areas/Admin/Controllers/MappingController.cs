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
                p.SKU.Code,
                // Include other properties as needed, e.g., ImageUrl, Price
            }).ToListAsync();

            return Json(products);
        }
        [HttpPost]
        public async Task<IActionResult> SubmitSelectedProducts([FromBody] SelectedProductsViewModel submissionData)
        {
            if (submissionData == null || (submissionData.SelectedProductIds == null && submissionData.DeselectedProductIds == null))
            {
                return Json(new { success = false, message = "Invalid submission data." });
            }
            submissionData.DeselectedProductIds ??= new List<int>();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Fetch all products for the given SKU.
                    var skuProducts = await _context.Product
                                                     .Include(p => p.RecommendedProducts) // Ensure you load the recommended products.
                                                     .Where(p => p.SKU.Code == submissionData.Sku)
                                                     .ToListAsync();

                    if (!skuProducts.Any())
                    {
                        return Json(new { success = false, message = "No products found with the provided SKU." });
                    }

                    foreach (var product in skuProducts)
                    {
                        // Handle new recommendations.
                        foreach (var selectedId in submissionData.SelectedProductIds.Except(product.RecommendedProducts.Select(rp => rp.RecommendedProductId)))
                        {
                            product.RecommendedProducts.Add(new ProductRecommendation { ProductId = product.Id, RecommendedProductId = selectedId });
                        }

                        // Handle deselected recommendations.
                        var recommendationsToRemove = product.RecommendedProducts
                            .Where(rp => submissionData.DeselectedProductIds.Contains(rp.RecommendedProductId))
                            .ToList();

                        foreach (var recommendation in recommendationsToRemove)
                        {
                            product.RecommendedProducts.Remove(recommendation);
                            _context.ProductRecommendations.Remove(recommendation); // Also remove from the context to ensure deletion.
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "Recommendations updated successfully." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
                }
            }
        }

        #region API CALLS
        [HttpGet]
        public async Task<JsonResult> GetProductsByCategory(int categoryId, string sku)
        {
            // Fetch IDs of recommended products based on the provided SKU
            var recommendedProductIds = await _context.ProductRecommendations
                                                       .Where(pr => pr.Product.SKU.Code == sku)
                                                       .Select(pr => pr.RecommendedProductId)
                                                       .ToListAsync();

            // Fetch all products that belong to the specified category but exclude those with the provided SKU
            var products = await _context.Product
                                          .Where(p => p.CategoryID == categoryId && p.SKU.Code != sku)
                                          .Select(p => new
                                          {
                                              p.Id,
                                              p.Name,
                                              p.ImageUrl,
                                              p.Price,
                                              p.SKU.Code,
                                              p.ColorCode,
                                              IsRecommended = recommendedProductIds.Contains(p.Id)
                                          })
                                          .ToListAsync();
            return Json(products);
        }

        [HttpGet]
        public async Task<JsonResult> GetProductsByBrand(int brandId, string sku)
        {
            // Fetch IDs of recommended products based on the provided SKU
            var recommendedProductIds = await _context.ProductRecommendations
                                                       .Where(pr => pr.Product.SKU.Code == sku)
                                                       .Select(pr => pr.RecommendedProductId)
                                                       .ToListAsync();

            // Fetch all products that belong to the specified brand but exclude those with the provided SKU
            var products = await _context.Product
                                          .Where(p => p.BrandID == brandId && p.SKU.Code != sku)
                                          .Select(p => new
                                          {
                                              p.Id,
                                              p.Name,
                                              p.ImageUrl,
                                              p.Price,
                                              p.ColorCode,
                                              IsRecommended = recommendedProductIds.Contains(p.Id) // Determine if the product is recommended
                                          })
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
                        p.ColorCode,

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
