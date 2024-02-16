using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FypWeb.Areas.SalesEmployee.Controllers
{
    [Area("SalesEmployee")]
    [Authorize(Roles = SD.Role_Sales_Employee)]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
