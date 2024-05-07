using Fyp.DataAccess.Data;
using Fyp.Models.ViewModels;
using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FypWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(Guid id)
        {
            // Fetch OrderDetails related to the given OrderHeaderId
            var orderDetailsList = await _context.OrderDetails
                .Where(od => od.OrderHeaderId == id).ToListAsync();

            if (!orderDetailsList.Any())
            {
                return NotFound("No order details found for this order.");
            }

  
       
          
            var orderHeader = await _context.OrderHeaders
                .Include(oh => oh.ApplicationUser) // Include ApplicationUser navigation property
                .FirstOrDefaultAsync(oh => oh.Id == id);
            if (orderHeader == null)
            {
                return NotFound("Order not found.");
            }
            string fullName = orderHeader.ApplicationUser.FullName;
            // Create an instance of the OrderViewModel and populate it with the fetched data
            var viewModel = new OrderViewModel
            {
                OrderHeader = orderHeader,
                OrderDetails = orderDetailsList,
                ApplicationUserFullName = fullName
            };

            return View(viewModel);
        }



        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAllTransactions()
        {

            var query = from orderHeader in _context.OrderHeaders
                        join applicationUser in _context.Users on orderHeader.ApplicationUserId equals applicationUser.Id
                        join orderDetail in _context.OrderDetails on orderHeader.Id equals orderDetail.OrderHeaderId
                        select new
                        {
                            orderHeader.Id,
                            orderHeader.CustomerName,
                            orderHeader.CustomerEmail,
                            orderHeader.CustomerPhone,
                            orderHeader.OrderDate,
                            orderHeader.OrderTotal,
                            orderHeader.ApplicationUserId,
                            EmployeeEmail = applicationUser.UserName,
                            Detail = new { orderDetail.ProductId, orderDetail.ProductName, orderDetail.Quantity, orderDetail.Price, applicationUser.Id, applicationUser.UserName }
                        };

            var rawData = await query.ToListAsync();

            var groupedData = rawData
                .GroupBy(x => new { x.Id, x.CustomerName, x.CustomerEmail, x.CustomerPhone, x.OrderDate, x.OrderTotal, x.ApplicationUserId, x.EmployeeEmail })
                .Select(g => new
                {
                    g.Key.Id,
                    g.Key.CustomerName,
                    g.Key.CustomerEmail,
                    g.Key.CustomerPhone,
                    g.Key.ApplicationUserId,
                    g.Key.EmployeeEmail,
                    g.Key.OrderDate,
                    g.Key.OrderTotal,
         

                    OrderDetails = g.Select(x => x.Detail).ToList()
                })
                .ToList();

            return Json(new { data = groupedData });
        }
        #endregion
    }
}
