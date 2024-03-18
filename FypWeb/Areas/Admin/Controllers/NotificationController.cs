using Microsoft.AspNetCore.Mvc;

namespace FypWeb.Areas.Admin.Controllers
{
    public class Notification : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
