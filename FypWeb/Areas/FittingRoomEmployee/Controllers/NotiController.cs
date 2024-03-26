using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Utility;
using FypWeb.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FypWeb.Areas.FittingRoomEmployee.Controllers
{
    [Area("FittingRoomEmployee")]
    [Authorize(Roles = SD.Role_Fitting_Employee)]
    public class Home : Controller
    {
        INotiService _notiService = null;
        List<Noti> _oNotifications = new List<Noti>();
        private readonly ApplicationDbContext context;
        
        public Home(INotiService notiService, ApplicationDbContext _context)
        {
            _notiService = notiService;
            context = _context;
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


        [HttpPost]
        public async Task<IActionResult> SetNotificationRead(int id)
        {
            // Find the notification by ID
            var notification = await context.Notification.FindAsync(id);
            if (notification == null)
            {
                // If not found, return an error response
                TempData["Error"] = "Notification could not be found.";
                return Json(new { success = false, message = "Notification not found." });
            }

            // Check if the notification is already marked as read
            if (notification.IsRead)
            {
                // If already read, return a success response indicating no changes were needed
                return Json(new { success = true, message = "Already" });
            }

            // Update the IsRead property to true only if it was false
            notification.IsRead = true;

            // Save changes to the database
            await context.SaveChangesAsync();
            TempData["Success"] = "Notification marked as read successfully.";

            // Return a success response
            return Json(new { success = true });
        }

        #endregion
    }
}
