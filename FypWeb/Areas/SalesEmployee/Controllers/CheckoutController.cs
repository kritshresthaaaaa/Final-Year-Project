﻿using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Models.ViewModels;
using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace FypWeb.Areas.SalesEmployee.Controllers
{

    [Area("SalesEmployee")]
    [Authorize(Roles = SD.Role_Sales_Employee)]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;


        public CheckoutController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {

            return View();
        }
        /* [HttpPost]
         [Authorize]
         public async Task<IActionResult> ProceedCheckout(BillViewModel model)
         {
             var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
             var employee = await _context.ApplicationUser.FirstOrDefaultAsync(u => u.Id == employeeId);
             shoppingCart.ApplicationUserId = employeeId;
             _context.ShoppingCarts.Add(shoppingCart);
             _context.SaveChanges();
             return View();
         }*/


        public IActionResult GetProductDetailsByRFID()
        {

            // List of RFIDs
            string[] rfids = new[] { "123456", "123457" };
            // Assuming you have a DbContext to access your database
            // Fetch products matching any of the RFIDs in the list
            var products = _context.Product
                                   .Where(p => rfids.Contains(p.RFIDTag))
                                   .ToList();
            if (!products.Any())
            {
                return Json(new { success = false, message = "Product not found." });
            }
            // Map the products to your shopping cart model or any desired format
            var shoppingCartModels = products.Select(product => new
            {

                ProductId = product.Id,
                ProductName = product.Name,
                ProductPrice = product.Price,
                ProductRFID = product.RFIDTag,
                // Assuming Count = 1 for simplicity; adjust as necessary
                ProductCount = 1
            }).ToList();

            return Json(new { success = true, products = shoppingCartModels });

        }
        /*    [HttpPost]

            public async Task<IActionResult> OrderConfirmation([FromBody] OrderConfirmationVM model)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(); // Or any other appropriate response
                }
                var products = new List<CartConfimation>();
                foreach (var rfid in model.RfidTags)
                {
                    var product = await _context.Product
                .Include(p => p.Brand) // Assuming you have a navigation property named Brand
                .FirstOrDefaultAsync(p => p.RFIDTag == rfid);
                    if (product != null)
                    {
                        // Assuming ProductDetail logic here
                        products.Add(new CartConfimation {
                            ProductId = product.Id,
                            ProductName = product.Name, 
                            ProductQuantity = 1,
                            ProductBrand = product.Brand?.BrandName,
                            ProductSize = product.Sizes,                 
                            ProductRFID = rfid,
                            OrderTotal = GetPriceBasedOnQuantity(1, product.Price)

                        });
                    }
                }
                // Serialize the products list to JSON and store in session
                var productsJson = System.Text.Json.JsonSerializer.Serialize(products);
                HttpContext.Session.SetString("OrderProducts", productsJson);
                var redirectUrl = Url.Action("ConfirmOrder", "Checkout", new { Area = "SalesEmployee" });
                return Json(new { redirectUrl = redirectUrl });

            }*/
        [HttpPost]
        public async Task<IActionResult> OrderConfirmation([FromBody] OrderConfirmationVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(); // Or any other appropriate response
            }

            var today = DateTime.Today; // Today's date for discount validation
            var products = new List<CartConfimation>();

            foreach (var rfid in model.RfidTags)
            {
                var product = await _context.Product
                    .Include(p => p.Brand) // Include brand details
                    .FirstOrDefaultAsync(p => p.RFIDTag == rfid); // Query each product by its RFID

                if (product != null)
                {
                    // Query for an applicable discount
                    var discount = await _context.Discount
                        .Where(d => d.IsActive && today >= d.StartDate && today <= d.EndDate &&
                                    (d.CategoryID == null || d.CategoryID == product.CategoryID) &&
                                    (d.BrandID == null || d.BrandID == product.BrandID))
                        .OrderByDescending(d => d.Percentage)
                        .FirstOrDefaultAsync();

                    // Determine the price to use
                    double priceToUse = discount != null ? product.Price * (1 - (double)discount.Percentage / 100.0) : product.Price;

                    // Add product confirmation
                    products.Add(new CartConfimation
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        ProductQuantity = 1, // Assuming a quantity of 1 for simplicity
                        ProductBrand = product.Brand?.BrandName,
                        ProductSize = product.Sizes,
                        ProductRFID = rfid,
                        DiscountAmount = product.Price - priceToUse,
                        OrderTotal = 1 * priceToUse, // Use the determined price
                        ProductPrice = product.Price,
                    }); ;
                }
            }

            // Serialize the products list to JSON and store in session
            var productsJson = System.Text.Json.JsonSerializer.Serialize(products);
            HttpContext.Session.SetString("OrderProducts", productsJson);

            var redirectUrl = Url.Action("ConfirmOrder", "Checkout", new { Area = "SalesEmployee" });
            return Json(new { redirectUrl = redirectUrl });
        }





        /*   private double GetPriceBasedOnQuantity(int count, double price)
           {
               if (count < 1 && price < 0)
               {
                   BadRequest();
               }
               return count * price;

           }*/
        public IActionResult ConfirmOrder()
        {
            // Retrieve the products list from session
            var productsJson = HttpContext.Session.GetString("OrderProducts");
            if (string.IsNullOrEmpty(productsJson))
            {
                return RedirectToAction("Index"); // Or any other appropriate action
            }

            var products = System.Text.Json.JsonSerializer.Deserialize<List<CartConfimation>>(productsJson);

            // Calculate subtotal
            double total = 0;
            double discountedAmt = 0;
            double subTotal = 0;
            if (products != null)
            {
                foreach (var product in products)
                {
                    subTotal += product.ProductQuantity * product.ProductPrice;
                    total += product.OrderTotal * product.ProductQuantity;
                    discountedAmt += product.DiscountAmount * product.ProductQuantity;
                }
            }

            // Pass subtotal to the view
            ViewBag.Total = total;
            ViewBag.DiscountedAmt = discountedAmt;
            ViewBag.SubTotal = subTotal;

            // Continue as normal
            return View(products); // Assuming you're still passing the list of products to the view
        }
        [HttpPost]
        public async Task<IActionResult> ProcessOrder([FromBody] CustomerDetailsViewModel model)
        {
            // Check if model is null
            if (model == null)
            {
                return Json(new { success = false, message = "Customer details are missing." });
            }

            // Validate model properties
            if (string.IsNullOrWhiteSpace(model.CustomerName) ||
                string.IsNullOrWhiteSpace(model.CustomerEmail) ||
                string.IsNullOrWhiteSpace(model.CustomerPhone))
            {
                return Json(new { success = false, message = "Customer name, email, or phone is missing." });
            }
            var productsJson = HttpContext.Session.GetString("OrderProducts");
            if (string.IsNullOrEmpty(productsJson))
            {
                return Json(new { success = false, message = "Failed to retrieve product details from session." });
            }
            var products = JsonConvert.DeserializeObject<List<CartConfimation>>(productsJson);
            if (products == null || !products.Any())
            {
                return Json(new { success = false, message = "No products in the cart." });
            }
            var orderHeader = new OrderHeader
            {
                Id = Guid.NewGuid(),
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                CustomerPhone = model.CustomerPhone,
                OrderDate = DateTime.Now,
                OrderTotal = products.Sum(p => p.OrderTotal),
                PaymentStatus = SD.PaymentStatusPending
            };
            var orderDetailsList = new List<OrderDetail>();
            foreach (var product in products)
            {
                var orderDetail = new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    OrderHeaderId = orderHeader.Id,
                    ProductId = product.ProductId,
                    Quantity = product.ProductQuantity,
                    Price = product.OrderTotal 
                };
                orderDetailsList.Add(orderDetail);
            }
            _context.OrderHeaders.Add(orderHeader);
            _context.OrderDetails.AddRange(orderDetailsList);
            await _context.SaveChangesAsync();
            // Clear the session after order has been processed
            HttpContext.Session.Remove("OrderProducts");
            return Json(new { success = true, message = "Order processed successfully", orderId = orderHeader.Id });
        }




    }
}
