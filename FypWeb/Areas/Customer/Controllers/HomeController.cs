using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FypWeb.Areas.Customer.Controllers
{
 
    [Area("Customer")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View("Index","_Customers");
        }
    }
}
