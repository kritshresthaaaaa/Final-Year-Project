using Microsoft.AspNetCore.Mvc;

namespace FypWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
