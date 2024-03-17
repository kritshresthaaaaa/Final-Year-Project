using Fyp.DataAccess.Data;
using Fyp.Models.ViewModels;
using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FypWeb.Areas.SalesEmployee.Controllers
{
    [Area("SalesEmployee")]
    [Authorize(Roles = SD.Role_Sales_Employee)]
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
                .Where(od => od.OrderHeaderId == id)
                .Include(od => od.Product) // Assuming you still want to include Product details
                .ToListAsync();

            if (!orderDetailsList.Any())
            {
                return NotFound("No order details found for this order.");
            }

            // Optionally, fetch the OrderHeader separately if needed for display
            var orderHeader = await _context.OrderHeaders.FirstOrDefaultAsync(oh => oh.Id == id);
            if (orderHeader == null)
            {
                return NotFound("Order not found.");
            }

            // Assuming you want to display both OrderHeader and its related OrderDetails
            // You might need a ViewModel to combine these if they're going to be displayed together
            var viewModel = new OrderViewModel
            {
                OrderHeader = orderHeader,
                OrderDetails = orderDetailsList
            };

            return View(viewModel);
        }

  
        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAllTransactions()
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
         
            var query = from orderHeader in _context.OrderHeaders
                        where orderHeader.ApplicationUserId == currentUserId 
                        join orderDetail in _context.OrderDetails on orderHeader.Id equals orderDetail.OrderHeaderId
                        join product in _context.Product on orderDetail.ProductId equals product.Id
                        select new
                        {
                            orderHeader.Id,
                            orderHeader.CustomerName,
                            orderHeader.CustomerEmail,
                            orderHeader.CustomerPhone,
                            orderHeader.OrderDate,
                            orderHeader.OrderTotal,
                            Detail = new { orderDetail.ProductId, ProductName = product.Name, orderDetail.Quantity, orderDetail.Price }
                        };

            var rawData = await query.ToListAsync();

            var groupedData = rawData
                .GroupBy(x => new { x.Id, x.CustomerName, x.CustomerEmail, x.CustomerPhone, x.OrderDate, x.OrderTotal })
                .Select(g => new
                {
                    g.Key.Id,
                    g.Key.CustomerName,
                    g.Key.CustomerEmail,
                    g.Key.CustomerPhone,
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
