﻿using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Models.ViewModels;
using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FypWeb.Areas.Admin;
using Microsoft.IdentityModel.Tokens;

namespace FypWeb.Areas.Employee.Controllers
{

    [Area("Employee")]
    [Authorize(Roles = SD.Role_Sales_Employee)]
    public class Checkout : Controller
    {
        private readonly ApplicationDbContext _context;


        public Checkout(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {

            return View();
        }
        public async Task<List<string>> ReadTags()
        {
            var reader = new Reader("192.168.1.1", false); // Configure your Reader
            reader.ConnectToDevice();
            reader.SetDeviceReadMode();
            reader.StartDevice();
            var detectedEPCs = reader.GetDetectedEPCs();
            await Task.Delay(2000); // Consider using await Task.Delay(2000); for async operation

            // Filter detected EPCs to only include those present in the database
            var validEPCs = new List<string>();
            foreach (var epc in detectedEPCs)
            {
                // Check if the EPC exists in the database
                var product = await _context.Product.FirstOrDefaultAsync(p => p.RFIDTag == epc);
                if (product != null)
                {
                    validEPCs.Add(epc);
                }
            }

            return validEPCs;
        }


        public async Task<IActionResult> GetProductDetailsByRFID()
        {
            var rfids = await ReadTags();
            // List of RFIDs
            if (rfids == null || rfids.Count == 0)
            {


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

                var shoppingCartModels = products.Select(product =>
                {
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
            else
            {
                return Json(new { success = false, message = "No RFID tags detected." });
            }
        }
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
                            if (productEntity != null)
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
                            var email = customerDetails.CustomerEmail;
                            if (email != null)
                            {
                                // Construct the email subject dynamically
                                var subject = $"Order Confirmation - Order #{orderHeader.Id}";
                                var body = "Your order has been successfully processed. Details: ";
                                foreach (var orderDetail in orderDetailsList)
                                {
                                    var product = await _context.Product.FirstOrDefaultAsync(p => p.Id == orderDetail.ProductId);
                                    if (product != null)
                                    {
                                        // Add product information to the subject
                                        body += $" | {product.Name} (Qty: {orderDetail.Quantity})";
                                    }
                                }
                                // Add additional information to the subject
                                subject += $" | Total Amount: ${orderHeader.OrderTotal}";

                                // Send the email with the constructed subject
                                await SendEmailAsync(email, subject, body);
                            }
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


        private async Task<bool> SendEmailAsync(string email, string subject, string body)
        {
            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            message.From = new MailAddress("np03cs4s220079@heraldcollege.edu.np");
            message.To.Add(email);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;
            smtp.Port = 587;
            smtp.Host = "smtp.gmail.com";
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("np03cs4s220079@heraldcollege.edu.np", "eizdbtlqjhheslfd");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            try
            {
                await smtp.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }


        }
    }
}
