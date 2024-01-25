using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Fyp.DataAccess.Data;
using Fyp.Models;
using Fyp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FypWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public EmployeeController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Employee
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employee.
                Select(e => new EmployeeViewModel
                {
                    Employee = e
                }).
                
                ToListAsync();
            return View(employees);
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
    }
}
