﻿using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Models.ViewModels;
using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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
        [Authorize]
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


        /*      public IActionResult GetProductDetailsByRFID()
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

              }*/
        [Authorize]
        public IActionResult GetProductDetailsByRFID()
        {
            // List of RFIDs
            string[] rfids = new[] {  "123457", "123450" };
            var today = DateTime.Today; // Today's date for discount validation

            // Fetch products matching any of the RFIDs in the list
            var products = _context.Product
                                   .Where(p => rfids.Contains(p.RFIDTag))
                                   .Include(p => p.Brand) // Include brand details
                                   .ToList();

            if (!products.Any())
            {
                return Json(new { success = false, message = "Product not found." });
            }

            var shoppingCartModels = products.Select(product => {
                // Query for an applicable discount
                var discount = _context.Discount
                    .Where(d => d.IsActive && today >= d.StartDate && today <= d.EndDate &&
                                (d.CategoryID == null || d.CategoryID == product.CategoryID) &&
                                (d.BrandID == null || d.BrandID == product.BrandID))
                    .OrderByDescending(d => d.Percentage)
                    .FirstOrDefault();

                // Determine the price to use
                double priceToUse = discount != null ? product.Price * (1 - (double)discount.Percentage / 100.0) : product.Price;

                return new
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductPrice = product.Price,
                    ProductRFID = product.RFIDTag,
                    ProductCount = 1, // Assuming Count = 1 for simplicity; adjust as necessary
                    DiscountAmount = product.Price - priceToUse,
                    OrderTotal = 1 * priceToUse, // Use the determined price
                    ProductBrand = product.Brand?.BrandName,
                    // Add more product details as needed
                };
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
        [Authorize]
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
        [Authorize]
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
            var transactionId = Guid.NewGuid().ToString();
            ViewBag.TransactionId = transactionId;
            string messageHash = $"total_amount={total},transaction_uuid={transactionId},product_code=EPAYTEST";
            ViewBag.MessageHash = messageHash;
            var signature = GenerateSignature(messageHash, "8gBm/:&EnhH.1/q");
            ViewBag.Signature = signature;
            ViewBag.SecretKey = "8gBm/:&EnhH.1/q";
            ViewBag.MerchantId = "EPAYTEST";
            ViewBag.ServiceCharge = 0;
            ViewBag.ProductDeliveryCharge = 0;
            // Store the signature in session
            HttpContext.Session.SetString("Signature", signature);



            // Continue as normal
            return View(products); // Assuming you're still passing the list of products to the view
        }
        [Authorize]
        private string GenerateSignature(string message, string secretKey)
        {
            var encoding = new ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secretKey);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }
        [Authorize]
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
                PaymentStatus = SD.PaymentStatusPending,
                TotalQuantity = products.Sum(p => p.ProductQuantity)
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
        [Authorize]
        [HttpPost]
        public IActionResult StoreCustomerDetailsInSession([FromBody] CustomerDetailsViewModel model)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var employeeId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            model.ApplicationUserId = employeeId;


            if (model == null)
            {
                return Json(new { success = false, message = "Customer details are missing." });
            }
            


            if (
                string.IsNullOrWhiteSpace(model.ApplicationUserId) ||
                string.IsNullOrWhiteSpace(model.CustomerName) ||
                string.IsNullOrWhiteSpace(model.CustomerEmail) ||
                string.IsNullOrWhiteSpace(model.CustomerPhone))
            {
                return Json(new { success = false, message = "Customer name, email, or phone is missing." });
            }
            var serializedModel = JsonConvert.SerializeObject(model);

            HttpContext.Session.SetString("CustomerDetails", serializedModel);
            return Json(new { success = true });
        }

        [Authorize]
        public async Task<IActionResult> VerifyEsewa(string data)
        {

            if (!string.IsNullOrEmpty(data))
            {
                var base64EncodedBytes = System.Convert.FromBase64String(data);
                var decodedString = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                var json = JObject.Parse(decodedString);
                var status = json["status"].ToString();
                var totalAmountValue = decimal.Parse(json["total_amount"].ToString());
                var totalAmount = totalAmountValue % 1 == 0 ? ((int)totalAmountValue).ToString() : totalAmountValue.ToString();
                var transactionCode = json["transaction_code"].ToString();            
           var signdFieldNames = json["signed_field_names"].ToString();
                var transactionUuid = json["transaction_uuid"].ToString();
                var productCode = json["product_code"].ToString();
                string messageHash = $"total_amount={totalAmount},transaction_uuid={transactionUuid},product_code={productCode}";
              
            /*    var messsage = "transaction_code=" + transactionCode + ",status=" + status + ",total_amount=" + totalAmount + ",transaction_uuid=" + transactionUuid + ",product_code=" + productCode + ",signed_field_names=" + signdFieldNames;*/
               /* var message = "transaction_code={transactionCode},status={status},total_amount={totalAmount},transaction_uuid={transactionUuid},product_code={productCode},signed_field_names={signdFieldNames}";*/

                var signature = GenerateSignature(messageHash, "8gBm/:&EnhH.1/q");
                var serverGeneratedSignature = HttpContext.Session.GetString("Signature");

                if (status == "COMPLETE" && serverGeneratedSignature == signature)
                {
                    // Retrieve customer details from session
                    var customerDetailsJson = HttpContext.Session.GetString("CustomerDetails");
                    if (!string.IsNullOrEmpty(customerDetailsJson))
                    {
                        // Deserialize the JSON string back into CustomerDetails object
                        var customerDetails = JsonConvert.DeserializeObject<CustomerDetailsViewModel>(customerDetailsJson);


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
                            CustomerName = customerDetails.CustomerName,
                            CustomerEmail = customerDetails.CustomerEmail,
                            CustomerPhone = customerDetails.CustomerPhone,
                            OrderDate = DateTime.Now,
                            OrderTotal = products.Sum(p => p.OrderTotal),
                            PaymentStatus = SD.PaymentStatusApproved,
                            PaymentDate = DateTime.Now,
                            PaymentIntentId = transactionCode,
                            ApplicationUserId = customerDetails.ApplicationUserId,
                            TotalQuantity = products.Sum(p => p.ProductQuantity)

                        };
                        _context.OrderHeaders.Add(orderHeader);
                        await _context.SaveChangesAsync();
                        var orderDetailsList = new List<OrderDetail>();
                        foreach (var product in products)
                        {
                            var productEntity = await _context.Product.FirstOrDefaultAsync(p => p.Id == product.ProductId);
                            if (productEntity !=null)
                            {
                                // Remove product entity since the purchase is complete
                               /* _context.Product.Remove(productEntity);*/
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

                        }
                        try
                        {
                           
                            _context.OrderDetails.AddRange(orderDetailsList);
                            await _context.SaveChangesAsync();

                        }
                        catch (Exception ex)
                        {
                            return Json(new { success = false, message = "Failed to process order." });
                        }
                        
                      

                        // Clear the session after order has been processed
                        HttpContext.Session.Remove("OrderProducts");
                        HttpContext.Session.Remove("Signature");
                        HttpContext.Session.Remove("CustomerDetails");
                        TempData["success"] = "Payment Successfully Completed & Customer Details Saved.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["error"] = "No Customer Details were found to save.";
                        HttpContext.Session.Remove("OrderProducts");
                        HttpContext.Session.Remove("Signature");
                        HttpContext.Session.Remove("CustomerDetails");
                        // Handle case where customer details are not found in session
                        return RedirectToAction("GetProductDetailsByRFID");
                    }
                }
                else
                {

                    TempData["error"] = "Payment verification failed. Try Again!";
                    HttpContext.Session.Remove("OrderProducts");
                    HttpContext.Session.Remove("Signature");
                    HttpContext.Session.Remove("CustomerDetails");
                    // Handle payment verification failure
                    return RedirectToAction("GetProductDetailsByRFID");
                }
            }
            else
            {
                TempData["error"] = "No data was received from eSewa. Try Again!";
                // Handle the case where data is null or empty
                return RedirectToAction("ConfirmOrder");
            }
            }



    }
}
