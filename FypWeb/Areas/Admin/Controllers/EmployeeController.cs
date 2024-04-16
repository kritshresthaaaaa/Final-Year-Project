using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Models.ViewModels;
using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FypWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class EmployeeController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public EmployeeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }

        // GET: Employee
        public IActionResult Index()
        {
            // Retrieve all users using UserManager
            var users = _userManager.Users.ToList();

            return View();
        }

        public IActionResult RoleManagement(string userId)
        {
            string RoleID = _context.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;
            RoleManagementVM roleManagementVM = new RoleManagementVM()
            {
                ApplicationUser = _context.ApplicationUser.FirstOrDefault(u => u.Id == userId),
                RoleList = _context.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id
                }),
                UserList = _context.ApplicationUser.Select(i => new SelectListItem
                {
                    Text = i.FullName,
                    Value = i.Id
                }),
            };
            roleManagementVM.ApplicationUser.Role = _context.Roles.FirstOrDefault(u => u.Id == RoleID).Name;
          
          
            return View(roleManagementVM);
        }
        [HttpPost]
        public  IActionResult RoleManagement(RoleManagementVM roleManagementVM)
        {
            string RoleID = _context.UserRoles.FirstOrDefault(u => u.UserId == roleManagementVM.ApplicationUser.Id).RoleId;
            string oldRole = _context.Roles.FirstOrDefault(u => u.Id == RoleID).Name;
            string check = roleManagementVM.ApplicationUser.Role;
            string newRole= _context.Roles.FirstOrDefault(u => u.Id == check).Name;
            if (!(roleManagementVM.ApplicationUser.Role == oldRole))
            {
                ApplicationUser applicationUser = _context.ApplicationUser.FirstOrDefault(u => u.Id == roleManagementVM.ApplicationUser.Id);
                _context.SaveChanges();
                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                 _userManager.AddToRoleAsync(applicationUser, newRole).GetAwaiter().GetResult();
            }


            return RedirectToAction("Index");
        }


        // GET: Employee/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Employee == null)
            {
                return NotFound();
            }

            var employeeDetail = await _context.Employee
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employeeDetail == null)
            {
                return NotFound();
            }

            return View(employeeDetail);
        }

        // GET: Employee/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employee/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel employeeViewDetail, IFormFile? file)
        {
            employeeViewDetail.Employee.FullName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(employeeViewDetail.Employee.FullName.ToLower());
            // Check for duplicate email, password, and phone entries
            var isEmailExist = await _context.Employee.AnyAsync(e => e.Email == employeeViewDetail.Employee.Email);
            var isPasswordExist = await _context.Employee.AnyAsync(e => e.Password == employeeViewDetail.Employee.Password);
            var isPhoneExist = await _context.Employee.AnyAsync(e => e.Phone == employeeViewDetail.Employee.Phone);
            var isFullNameExist = await _context.Employee.AnyAsync(e => e.FullName == employeeViewDetail.Employee.FullName);

            if (isFullNameExist)
            {
                ModelState.AddModelError("Name", "A user with this name already exists");
            }
            if (isEmailExist)
            {
                ModelState.AddModelError("Email", "A user with this email already exists");
            }
            if (isPasswordExist)
            {
                ModelState.AddModelError("Password", "A user with this password already exists");
            }
            if (isPhoneExist)
            {
                ModelState.AddModelError("Phone", "A user with this phone already exists");
            }

            if (ModelState.IsValid)
            {
                employeeViewDetail.Employee.RegistrationDate = DateTime.Now;
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string employeePath = Path.Combine(wwwRootPath, "images", "employee");
                    Directory.CreateDirectory(employeePath);

                    string filePath = Path.Combine(employeePath, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    employeeViewDetail.Employee.ImageUrl = "/images/employee/" + fileName; // Use forward slashes for URLs
                }

                _context.Add(employeeViewDetail.Employee);
                TempData["success"] = "Employee Added";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(employeeViewDetail);
        }


        // GET: Employee/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Employee == null)
            {
                return NotFound();
            }

            var employeeDetail = await _context.Employee.FindAsync(id);
            if (employeeDetail == null)
            {
                return NotFound();
            }
            return View(employeeDetail);
        }

        // POST: Employee/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,DOB,Gender,Email,Password,Address,Phone")] EmployeeDetail employeeDetail)
        {
            if (id != employeeDetail.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employeeDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeDetailExists(employeeDetail.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employeeDetail);
        }

        // GET: Employee/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Employee == null)
            {
                return NotFound();
            }

            var employeeDetail = await _context.Employee
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employeeDetail == null)
            {
                return NotFound();
            }

            return View(employeeDetail);
        }

        // POST: Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Employee == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Employee'  is null.");
            }
            var employeeDetail = await _context.Employee.FindAsync(id);
            if (employeeDetail != null)
            {
                _context.Employee.Remove(employeeDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeDetailExists(int id)
        {
            return _context.Employee.Any(e => e.Id == id);
        }
        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Fetch all users
            var usersList = await _context.ApplicationUser.ToListAsync();

            // Fetch all user roles and roles as a list for performance
            var userRoles = await _context.UserRoles.ToListAsync();
            var roles = await _context.Roles.ToListAsync();

            // Filter out users with the admin role
            var adminRoleId = roles.FirstOrDefault(r => r.Name == "Admin")?.Id;
            var customerHandlerEmployeeId = roles.FirstOrDefault(r => r.Name == "Customer-Handler")?.Id;
            var nonAdminUsers = new List<ApplicationUser>();

            foreach (var user in usersList)
            {
                var userRoleIds = userRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToList();
                if (!userRoleIds.Contains(adminRoleId) && !userRoleIds.Contains(customerHandlerEmployeeId))
                {
                    // Assuming you want to set the Role property of non-admin users,
                    // you need to handle users with multiple roles or no roles.
                    var firstRoleId = userRoleIds.FirstOrDefault();
                    var roleName = roles.FirstOrDefault(r => r.Id == firstRoleId)?.Name;

                    // Set the role name if you have a Role property in your ApplicationUser
                    // This is just an example, adjust according to your actual user model
                    user.Role = roleName;

                    nonAdminUsers.Add(user);
                }
            }

            return Json(new { data = nonAdminUsers });
        }

        [HttpPost]
        public async Task<IActionResult> LockUnlock([FromBody]string id)
        {
          var objFromDb = _context.ApplicationUser.FirstOrDefault(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }
            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                // user is currently locked, we will unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(100);
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Operation Successful" });
        }
        #endregion
    }

}
