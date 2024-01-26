using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Util;

namespace FypWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        IWebHostEnvironment hostEnvironment;
        private readonly ApplicationDbContext _context;
        private bool debug = false;
        private Device device;
        private RESTUtil util;
        private HexUtil utilities;
        private string address = "192.168.1.1";
        private readonly ILogger<ProductController> _logger;
        public ProductController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            util = new RESTUtil(address, debug);
            device = util.parseDevice(false);
            utilities = new HexUtil();
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

        public JsonResult ReadAndWriteEPC()
        {
            try
            {
                // Connect to the device
                util.connect(device, false);

                string readMode = util.getReadMode(device, false);

                if (readMode != "AUTONOMOUS")
                {
                    util.setDeviceMode(device, "Autonomous", false);
                }

                // Make an inventory and collect all the EPCs
                util.startStopDevice(device, true, false);

                Thread.Sleep(2000);
                List<string> detectedEPCs = util.getSequentialInventory(device, true, false);
                Thread.Sleep(2000);

                util.startStopDevice(device, false, false);

                var result = new List<EpcScanResult>();

                // Iterate through detected EPCs
                foreach (string epc in detectedEPCs)
                {
                    // Simply add the original EPC to the result
                    result.Add(new EpcScanResult { OriginalEPC = epc, NewEPC = epc });
                }

                return new JsonResult(new ScanResponse { Results = result });
            }
            catch (Exception ex)
            {
                return new JsonResult(new ScanResponse { ErrorMessage = ex.Message });
            }
        }
        public JsonResult readTagFromDatabase()
        {
            try
            {
                util.connect(device, false);

                string readMode = util.getReadMode(device, false);

                if (readMode != "AUTONOMOUS")
                {
                    util.setDeviceMode(device, "Autonomous", false);
                }

                // Make an inventory and collect all the EPCs
                util.startStopDevice(device, true, false);

                Thread.Sleep(2000);
                List<string> detectedEPCs = util.getSequentialInventory(device, true, false);
                Thread.Sleep(2000);

                util.startStopDevice(device, false, false);

                var result = new List<EpcScanResult>();

                var products = new List<ProductDetail>();
                foreach (var epcResult in result)
                {
                    var product = _context.Product.FirstOrDefault(p => p.RFIDTag == epcResult.OriginalEPC);
                    if (product != null)
                    {
                        products.Add(product);
                    }
                }

                return Json(new { Products = products });
            }
            catch (Exception ex)
            {
                return Json(new { ErrorMessage = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult refreshProductList()
        {
            var response = ReadAndWriteEPC();
            // You may need to convert the response to a format suitable for your view
            return response;
        }

        /*       private string changeFirstBit(string epc)
               {
                   // Change the first bit of the EPC
                   string binEPC = this.utilities.HexStringToBinary(epc);
                   string newBinEPC = binEPC[0] == '0' ? '1' + binEPC.Substring(1) : '0' + binEPC.Substring(1);
                   return this.utilities.BinaryStringToHex(newBinEPC);
               }*/
        // GET: Product
        public async Task<IActionResult> Index()
        {
            // Fetch the products from your database and map them to ProductDetailViewModel
            var products = await _context.Product
                .Select(p => new ProductDetailViewModel
                {
                    Product = p
                    // Include other properties as needed
                })
                .ToListAsync();

            return View(products);
        }

        public IActionResult Scan()
        {
            return View("Scan");
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Product == null)
            {
                return NotFound();
            }

            var productDetail = await _context.Product
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productDetail == null)
            {
                return NotFound();
            }

            return View(productDetail);
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
        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductDetailViewModel productViewModel, IFormFile? file)
        {

            productViewModel.Product.Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(productViewModel.Product.Name.ToLower());
            string wwwRootPath = hostEnvironment.WebRootPath;
            if (IsEpcUnique(productViewModel.Product.RFIDTag))
            {
                if (file != null)
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

                _context.Add(productViewModel.Product);
                _context.SaveChanges();
                TempData["success"] = "Product has been added successfully.";
                return RedirectToAction(nameof(Index));

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
            return View(productDetail);
        }

        // POST: Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Product == null)
            {
                return NotFound();
            }

            var productDetail = await _context.Product
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productDetail == null)
            {
                return NotFound();
            }

            return View(productDetail);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Product == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Product'  is null.");
            }
            var productDetail = await _context.Product.FindAsync(id);
            if (productDetail != null)
            {
                _context.Product.Remove(productDetail);
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Product has been deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool ProductDetailExists(int id)
        {
            return (_context.Product?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
