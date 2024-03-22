using Fyp.Models;
using Fyp.Utility;
using FypWeb.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FypWeb.Areas.FittingRoomEmployee.Controllers
{
    [Area("FittingRoomEmployee")]
    [Authorize(Roles = SD.Role_Fitting_Employee)]
    public class Home : Controller
    {
        INotiService _notiService = null;
        List<Noti> _oNotifications = new List<Noti>();
        public Home(INotiService notiService)
        {
            _notiService = notiService;
        }
        public IActionResult AllNotifications()
        {
            return View("Index");
        }
        #region API CALLS
        [HttpGet]
        public JsonResult GetAllNotifications(bool bIsGetOnlyUnread = false)
        {
            Guid nToEmployeeId = new Guid("757243c1-cbdd-43c5-a2da-605a6f1ba32e");
            _oNotifications = new List<Noti>();
            _oNotifications = _notiService.GetNotifications(nToEmployeeId, bIsGetOnlyUnread);
            return Json(new { data = _oNotifications });
        }
        #endregion
    }
}
