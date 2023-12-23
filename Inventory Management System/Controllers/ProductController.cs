using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Inventory_Management_System.Data;
using Inventory_Management_System.Models.Stock;
using Util;

namespace Inventory_Management_System.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private bool debug= false;
        private Device device;
        private RESTUtil util;
        private HexUtil utilities;
        private String address = "192.168.1.1";
        private readonly ILogger<ProductController> _logger;
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
            this.util = new RESTUtil(this.address, this.debug);
            this.device = this.util.parseDevice(false);
            this.utilities = new HexUtil();
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
                this.util.connect(this.device, false);

                string readMode = this.util.getReadMode(this.device, false);

                if (readMode != "AUTONOMOUS")
                {
                    this.util.setDeviceMode(this.device, "Autonomous", false);
                }

                // Make an inventory and collect all the EPCs
                this.util.startStopDevice(this.device, true, false);

                Thread.Sleep(2000);
                List<string> detectedEPCs = this.util.getSequentialInventory(this.device, true, false);
                Thread.Sleep(2000);

                this.util.startStopDevice(this.device, false, false);

                var result = new List<EpcScanResult>();

                // Iterate through detected EPCs
                foreach (string epc in detectedEPCs)
                {
                    // Generate a new EPC by changing the first bit
                    string newEPC = changeFirstBit(epc);

                    // Write the new EPC to the tag
                    this.util.CommissionTagOp(this.device, epc, newEPC, "", "", "", 1, false);

                    // Add the result to the list
                    result.Add(new EpcScanResult { OriginalEPC = epc, NewEPC = newEPC });
                }

                return new JsonResult(new ScanResponse { Results = result });
            }
            catch (Exception ex)
            {
                return new JsonResult(new ScanResponse { ErrorMessage = ex.Message });
            }
        }

        private string changeFirstBit(string epc)
        {
            // Change the first bit of the EPC
            string binEPC = this.utilities.HexStringToBinary(epc);
            string newBinEPC = binEPC[0] == '0' ? '1' + binEPC.Substring(1) : '0' + binEPC.Substring(1);
            return this.utilities.BinaryStringToHex(newBinEPC);
        }
        // GET: Product
        public async Task<IActionResult> Index()
        {
            return _context.Product!= null ?
                        View(await _context.Product.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Product'  is null.");
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
            return View();
        }

        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price")] ProductDetail productDetail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productDetail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(productDetail);
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
            return RedirectToAction(nameof(Index));
        }

        private bool ProductDetailExists(int id)
        {
            return (_context.Product?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
