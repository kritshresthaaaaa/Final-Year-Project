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
            if (submissionData == null || submissionData.SelectedProductIds == null || submissionData.DeselectedProductIds == null)
            {
                return Json(new { success = false, message = "Invalid submission data." });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var products = await _context.Product
                           .Include(p => p.RecommendedProducts)
                           .Include(p => p.ProductRecommendations)
                           .Where(p => p.SKU.Code == submissionData.Sku)
                           .ToListAsync();

                    if (!products.Any())
                    {
                        return Json(new { success = false, message = "No products found with the provided SKU." });
                    }

                    foreach (var product in products)
                    {
                        foreach (var selectedId in submissionData.SelectedProductIds)
                        {
                            if (product.Id != selectedId)
                            {
                                if (!product.RecommendedProducts.Any(rp => rp.RecommendedProductId == selectedId))
                                {
                                    var newRecommendation = new ProductRecommendation { ProductId = product.Id, RecommendedProductId = selectedId };
                                    _context.ProductRecommendations.Add(newRecommendation);
                                    Console.WriteLine($"Adding forward recommendation from {product.Id} to {selectedId}");
                                }

                                var reverseProduct = products.FirstOrDefault(p => p.Id == selectedId);
                                if (reverseProduct != null && !reverseProduct.RecommendedProducts.Any(rp => rp.RecommendedProductId == product.Id))
                                {
                                    reverseProduct.RecommendedProducts.Add(new ProductRecommendation { ProductId = selectedId, RecommendedProductId = product.Id });
                                    Console.WriteLine($"Adding reverse recommendation from {selectedId} to {product.Id}");
                                }
                            }
                        }

                        var recommendationsToRemove = product.RecommendedProducts
                            .Where(rp => submissionData.DeselectedProductIds.Contains(rp.RecommendedProductId))
                            .ToList();

                        _context.ProductRecommendations.RemoveRange(recommendationsToRemove);
                        Console.WriteLine($"Removing recommendations for product {product.Id}");
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "Recommendations updated successfully." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
                }
            }
        }


        /*        [HttpPost]
                public async Task<IActionResult> SubmitSelectedProducts([FromBody] SelectedProductsViewModel submissionData)
                {
                    if (submissionData == null || submissionData.SelectedProductIds == null || submissionData.DeselectedProductIds == null)
                    {
                        return Json(new { success = false, message = "Invalid submission data." });
                    }

                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var products = await _context.Product
                                   .Include(p => p.RecommendedProducts)
                                   .Include(p => p.ProductRecommendations)
                                   .ToListAsync();  // Temporarily remove SKU filter for debugging

                            if (!products.Any())
                            {
                                return Json(new { success = false, message = "No products found with the provided SKU." });
                            }

                            // Handling new recommendations and ensuring reverse mapping
                                foreach (var product in products)
                            {
                                foreach (var selectedId in submissionData.SelectedProductIds)
                                {
                                    if (product.Id != selectedId && !product.RecommendedProducts.Any(rp => rp.RecommendedProductId == selectedId))
                                    {
                                        var newRecommendation = new ProductRecommendation { ProductId = product.Id, RecommendedProductId = selectedId };
                                        _context.ProductRecommendations.Add(newRecommendation);
                                    }

                                    // Handle reverse recommendation
                                    var reverseProduct = products.FirstOrDefault(p => p.Id == selectedId);
                                    if (reverseProduct != null && product.Id != selectedId && !reverseProduct.RecommendedProducts.Any(rp => rp.RecommendedProductId == product.Id))
                                    {
                                        reverseProduct.RecommendedProducts.Add(new ProductRecommendation { ProductId = selectedId, RecommendedProductId = product.Id });
                                    }
                                }

                                // Removing deselected recommendations and their reverses
                                var recommendationsToRemove = product.RecommendedProducts
                                    .Where(rp => submissionData.DeselectedProductIds.Contains(rp.RecommendedProductId))
                                    .ToList();

                                _context.ProductRecommendations.RemoveRange(recommendationsToRemove);
                                foreach (var recommendation in recommendationsToRemove)
                                {
                                    // Find and remove the reverse recommendations
                                    var reverseRecommendations = _context.ProductRecommendations
                                        .Where(rp => rp.ProductId == recommendation.RecommendedProductId && rp.RecommendedProductId == product.Id)
                                        .ToList();
                                    _context.ProductRecommendations.RemoveRange(reverseRecommendations);
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
        */



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
