﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Models.ViewModels;
using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Util;

namespace FypWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        IWebHostEnvironment hostEnvironment;
        private readonly ApplicationDbContext _context;


        private readonly ILogger<ProductController> _logger;
        private readonly Reader _reader;
        public ProductController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;

            this.hostEnvironment = hostEnvironment;
        }



        public class EpcScanResult
        {
            public string OriginalEPC { get; set; }
            public string NewEPC { get; set; }
        }

        public class ScanResponse
        {
            public List<EpcScanResult> Results { get; set; }
            public string ErrorMessage { get; set; }
        }

        public async Task<JsonResult> ReadAndWriteEPC()
        {
            try
            {
                var reader = new Reader("192.168.1.1", false);
                reader.ConnectToDevice();
                reader.SetDeviceReadMode();
                reader.StartDevice();
                var detectedEPCs = reader.GetDetectedEPCs();
                await Task.Delay(2000);
                reader.StopDevice();
                var result = new List<EpcScanResult>();

                foreach (string epc in detectedEPCs)
                {

                    result.Add(new EpcScanResult { OriginalEPC = epc, NewEPC = epc });
                }

                return new JsonResult(new ScanResponse { Results = result });
            }
            catch (Exception ex)
            {
                return new JsonResult(new ScanResponse { ErrorMessage = ex.Message });
            }

        }

        public async Task<JsonResult> ReadTagFromDatabase()
        {
            var reader = new Reader("192.168.1.1", false); // Configure your Reader
            reader.ConnectToDevice();
            reader.SetDeviceReadMode();
            reader.StartDevice();
            var detectedEPCs = reader.GetDetectedEPCs();
            await Task.Delay(2000); // Consider using await Task.Delay(2000); for async
            reader.StopDevice();

            var inventoryList = new List<FetchTagsDataViewModel>();
            foreach (var epc in detectedEPCs)
            {
                var productEntity = await _context.Product
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.RFIDTag == epc);

                if (productEntity != null)
                {
                    var inventoryItem = new FetchTagsDataViewModel
                    {
                        Id = productEntity.Id,
                        ProductName = productEntity.Name,
                        Size = productEntity.Sizes,
                        Price = productEntity.Price,
                        CategoryName = productEntity.Category.CategoryName
                    };
                    inventoryList.Add(inventoryItem);
                }
            }

            return Json(inventoryList);
        }
        public async Task<IActionResult> Index()
        {
            var count = _context.Product.Count();
            ViewBag.productCount = count;
            var totalSales = _context.OrderHeaders.Sum(o => o.OrderTotal);
            ViewBag.totalSales = totalSales;
            return View();
        }
        // GET: Product
        /*    public async Task<IActionResult> Index()
            {
                var today = DateTime.Today;
                // Fetch products with their categories
                var products = await _context.Product
                    .Include(p => p.Category) // Eagerly load Category data
                    .ToListAsync();

                // Map to ProductDetailViewModel
                var productViewModels = products.Select(p => new ProductDetailViewModel
                {
                    Product = p
                }).ToList();
                ViewBag.productCount = productViewModels.Count();

                return View(productViewModels);
            }*/
        /*        public async Task<IActionResult> Index()
                {
                    var today = DateTime.Today;
                    var productsWithDiscounts = await _context.Product
                        .Include(p => p.Category)
                        .Select(p => new
                        {
                            Product = p,
                            Discount = _context.Discount
                                .Where(d => d.IsActive && d.StartDate<=today && today <= d.EndDate &&
                                            (d.CategoryID == null || d.CategoryID == p.CategoryID) &&
                                            (d.BrandID == null || d.BrandID == p.BrandID))
                                .OrderByDescending(d => d.Percentage) // Assuming you want the highest discount if multiple apply
                                .FirstOrDefault()
                        })
                        .ToListAsync();

                    var productViewModels = productsWithDiscounts.Select(pd => new ProductDetailViewModel
                    {
                        Product = pd.Product,
                        DiscountStartDate = pd.Discount?.StartDate,
                        DiscountEndDate = pd.Discount?.EndDate,               
                    }).ToList();

                    ViewBag.productCount = productViewModels.Count;

                    return View(productViewModels);
                }
        */



        public IActionResult Scan()
        {
            return View("Scan");
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var today = DateTime.Today;
            var productDetail = await _context.Product
                .Include(p => p.SKU)
                .ThenInclude(sku => sku.Products)
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Select(p => new
                {
                    Product = p,
                    Discount = _context.Discount
                        .Where(d => d.IsActive && d.StartDate <= today && today <= d.EndDate &&
                                    ((d.CategoryID == null && d.BrandID == null && d.SKUID == null) || // Discount applies to all products
                                    (d.CategoryID == null || d.CategoryID == p.CategoryID) &&
                                    (d.BrandID == null || d.BrandID == p.BrandID) &&
                                    (d.SKUID == null || d.SKUID == p.SKUID)))
                        .OrderByDescending(d => d.Percentage) // Assuming you want the highest discount if multiple apply
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(m => m.Product.Id == id);

            if (productDetail == null || productDetail.Product == null)
            {
                return NotFound();
            }

            // Map to ViewBag to pass additional data to the view
            ViewBag.TotalProductsInSKU = productDetail.Product.SKU?.Products?.Count() ?? 0;
            ViewBag.ProductColor = productDetail.Product.ColorCode;
            ViewBag.DiscountedPrice = productDetail.Discount != null ?
                CalculateDiscountedPrice(productDetail.Product.Price, productDetail.Discount.Percentage) :
                productDetail.Product.Price;
            ViewBag.IsActiveDiscount = productDetail.Discount != null;
            ViewBag.DiscountPercentage = productDetail.Discount?.Percentage;
            ViewBag.BrandName = productDetail.Product.Brand?.BrandName;
            ViewBag.CategoryName = productDetail.Product.Category?.CategoryName;

            return View(productDetail.Product);
        }


        private double CalculateDiscountedPrice(double originalPrice, decimal discountPercentage)
        {
            return originalPrice * (1 - (double)discountPercentage / 100);
        }

        // GET: Product/Create
        public IActionResult Create()
        {

            ViewBag.getCategory = _context.Category.ToList();
            ViewBag.getBrand = _context.Brand.ToList();
            return View();
        }
        private bool IsEpcUnique(string epc)
        {
            return !_context.Product.Any(p => p.RFIDTag == epc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductDetailViewModel productViewModel, IFormFile? file)
        {
            const int minWidth = 1000;
            const int minHeight = 1000;
            productViewModel.Product.Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(productViewModel.Product.Name.ToLower());
            string wwwRootPath = hostEnvironment.WebRootPath;

            if (IsEpcUnique(productViewModel.Product.RFIDTag))
            {
                if (file != null)
                {
                    using (var image = Image.FromStream(file.OpenReadStream()))
                    {
                        if (image.Width < minWidth || image.Height < minHeight)
                        {
                            ModelState.AddModelError("ImageFile", $"The image must be at least {minWidth} x {minHeight} pixels in size.");
                        }
                    }
                    if (!ModelState.IsValid)
                    {
                        // If the ModelState is invalid, print out the errors to the console
                        foreach (var modelStateKey in ModelState.Keys)
                        {
                            var value = ModelState[modelStateKey];
                            foreach (var error in value.Errors)
                            {
                                // Use the Console to print the error messages
                                Console.WriteLine("Error in {0}: {1}", modelStateKey, error.ErrorMessage);
                            }
                        }
                    }
                    if (ModelState.IsValid)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = Path.Combine(wwwRootPath, "images", "product");
                        Directory.CreateDirectory(productPath);
                        string filePath = Path.Combine(productPath, fileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        productViewModel.Product.ImageUrl = "/images/product/" + fileName;
                    }
                }
                if (!ModelState.IsValid)
                {
                    var allErrors = ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));

                    // Print errors to the console
                    foreach (var error in allErrors)
                    {
                        Console.WriteLine("Validation error: {0}", error);
                    }

                    // Populate ViewBag for Category and Brand again if ModelState is not valid
                    ViewBag.getCategory = _context.Category.ToList();
                    ViewBag.getBrand = _context.Brand.ToList();

                    // Return to the view to display the errors
                    return View(productViewModel);
                }


                if (ModelState.IsValid)
                {
                    var existingSku = _context.SKU.FirstOrDefault(sku => sku.Code == productViewModel.SKU);
                    if (existingSku == null)
                    {
                        // If not, create a new SKUDetail entry
                        existingSku = new SKUDetail { Code = productViewModel.SKU };
                        _context.SKU.Add(existingSku);
                        _context.SaveChanges(); // Save changes to obtain the SKUID
                    }
                    // Assign the SKUID to the ProductDetail
                    productViewModel.Product.SKUID = existingSku.SKUID;

                    _context.Add(productViewModel.Product);
                    _context.SaveChanges();
                    TempData["success"] = "Product has been added successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                ViewBag.EpcError = "This EPC is already assigned to another product.";
            }

            // Populate ViewBag for Category and Brand if ModelState is not valid
            ViewBag.getCategory = _context.Category.ToList();
            ViewBag.getBrand = _context.Brand.ToList();
            return View(productViewModel);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Product == null)
            {
                return NotFound();
            }

            var productDetail = await _context.Product.FindAsync(id);
            if (productDetail == null)
            {
                return NotFound();
            }

            var viewModel = new ProductDetailViewModel
            {
                Product = productDetail,
            };

            // Ensure ViewBag items are populated for the dropdowns
            ViewBag.getCategory = _context.Category.ToList();
            ViewBag.getBrand = _context.Brand.ToList();


            return View(viewModel); // Use the same view for Create and Edit
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price")] ProductDetail productDetail)
        {
            if (id != productDetail.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductDetailExists(productDetail.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["success"] = "Product has been edited successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(productDetail);
        }

        private bool ProductDetailExists(int id)
        {
            return (_context.Product?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var today = DateTime.Today;
            var productsWithDiscounts = await _context.Product
                .Include(p => p.Category)
                .Include(p => p.SKU) // Include SKU information
                .Select(p => new
                {
                    Product = p,
                    Discount = _context.Discount
                        .Where(d => d.IsActive && d.StartDate <= today && today <= d.EndDate &&
                                    ((d.CategoryID == null && d.BrandID == null && d.SKUID == null) || // Discount applies to all products
                                    (d.CategoryID == null || d.CategoryID == p.CategoryID) &&
                                    (d.BrandID == null || d.BrandID == p.BrandID) &&
                                    (d.SKUID == null || d.SKUID == p.SKUID)))
                        .OrderByDescending(d => d.Percentage) // Assuming you want the highest discount if multiple apply
                        .FirstOrDefault()
                })
                .ToListAsync();

            var productViewModels = productsWithDiscounts.Select(pd => new
            {
                Id = pd.Product.Id,
                Name = pd.Product.Name,
                Category = pd.Product.Category.CategoryName,
                SKU = pd.Product.SKU != null ? pd.Product.SKU.Code : "", // SKU code
                Price = pd.Product.Price,
                DiscountedPrice = pd.Discount != null ? CalculateDiscountedPrice(pd.Product.Price, pd.Discount.Percentage) : pd.Product.Price,
                IsActiveDiscount = pd.Discount != null,
                DiscountStartDate = pd.Discount?.StartDate,
                DiscountEndDate = pd.Discount?.EndDate
            }).ToList();

            return Json(new { data = productViewModels });
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var productDetail = await _context.Product.FindAsync(id);
            if (productDetail != null)
            {
                // Check if the product is recommended for other products
                var isRecommendedProduct = _context.ProductRecommendations.Any(pr => pr.RecommendedProductId == id);

                // If the product is recommended for other products, remove its associations as a recommended product
                if (isRecommendedProduct)
                {
                    var recommendingProducts = _context.ProductRecommendations
                                                   .Where(pr => pr.RecommendedProductId == id)
                                                   .ToList();

                    if (recommendingProducts.Any())
                    {
                        _context.ProductRecommendations.RemoveRange(recommendingProducts);
                        await _context.SaveChangesAsync();
                    }
                }

                // Check if there are any recommended products associated with the product
                var recommendedProducts = _context.ProductRecommendations
                                            .Where(pr => pr.ProductId == id)
                                            .ToList();

                if (recommendedProducts.Any())
                {
                    // Remove the associations with recommended products
                    _context.ProductRecommendations.RemoveRange(recommendedProducts);
                    await _context.SaveChangesAsync();
                }

                // If there's an image URL, delete the image file from the server
                if (!string.IsNullOrEmpty(productDetail.ImageUrl))
                {
                    string filePath = Path.Combine(hostEnvironment.WebRootPath, productDetail.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Remove the product itself
                _context.Product.Remove(productDetail);
                try
                {
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Product deleted successfully." });
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Error while deleting. " + ex.Message;
                    return Json(new { success = false, message = "Error while deleting. " + ex.Message });
                }
            }
            else
            {
                TempData["error"] = "Error while deleting. Product not found.";
                // If the product doesn't exist, return a failure response
                return Json(new { success = false, message = "Error while deleting. Product not found." });
            }
        }



        #endregion

    }
}
