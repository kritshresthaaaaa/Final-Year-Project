using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Utility;
using FypWeb.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FypWeb.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = SD.Role_Employee)]
    public class Notification : Controller
    {
        INotiService _notiService = null;
        List<Noti> _oNotifications = new List<Noti>();
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> _userManager;

        public Notification(INotiService notiService, ApplicationDbContext _context, UserManager<ApplicationUser> userManager)
        {
            _notiService = notiService;
            context = _context;
            _userManager = userManager;
        }
        public IActionResult AllNotifications()
        {
            return View("Index");
        }
        #region API CALLS
        [HttpGet]
        public async Task<JsonResult> GetAllNotifications(bool bIsGetOnlyUnread = false)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var employeeId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (employeeId == null)
            {
                return Json(new { error = "User not found." });
            }

            var currentUser = await _userManager.FindByIdAsync(employeeId);
            if (currentUser == null)
            {
                return Json(new { error = "User not found." });
            }

            Guid nToEmployeeId = new Guid(employeeId);

            // Assuming GetNotifications is correctly implemented to be async and returning Task<List<Noti>>
            var _oNotifications = await _notiService.GetNotifications(nToEmployeeId, bIsGetOnlyUnread);

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
