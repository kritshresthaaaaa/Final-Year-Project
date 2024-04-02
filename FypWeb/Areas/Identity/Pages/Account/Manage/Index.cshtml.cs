// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fyp.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FypWeb.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IndexModel(
          UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
          IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            public string FullName { get; set; }

            public string? ImageUrl { get; set; }
            public int? StockAlerter { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);


            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FullName = user.FullName,
                ImageUrl = user.ImageUrl,
                StockAlerter = user.StockAlerter
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile? file)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            var fullName = user.FullName;
            if (Input.FullName != fullName)
            {
                user.FullName = Input.FullName;
                var setFullNameResult = await _userManager.UpdateAsync(user);
                if (!setFullNameResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set full name.";
                    return RedirectToPage();
                }
            }
            var stockAlerter = user.StockAlerter;
            if (Input.StockAlerter != stockAlerter)
            {
                user.StockAlerter = Input.StockAlerter;
                var setStockAlerterResult = await _userManager.UpdateAsync(user);
                if (!setStockAlerterResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set stock alerter.";
                    return RedirectToPage();
                }
            }

            // Check if a file was uploaded
            if (file != null && file.Length > 0)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string employeePath = Path.Combine(wwwRootPath, "images", "employee");

                // Create the directory if it doesn't exist
                if (!Directory.Exists(employeePath))
                {
                    Directory.CreateDirectory(employeePath);
                }

                string filePath = Path.Combine(employeePath, fileName);
                // Save the uploaded file to disk
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                // Update the user's ImageUrl property
                user.ImageUrl = "/images/employee/" + fileName;
            }

            // Update the user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to update user profile.";
                return RedirectToPage();
            }

            // Refresh user's sign-in session
            await _signInManager.RefreshSignInAsync(user);
            TempData["success"] = "Successfully updated your profile";
            return RedirectToPage();
        }

        /*    public async Task<IActionResult> OnPostAsync(IFormFile? file)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                if (!ModelState.IsValid)
                {
                    await LoadAsync(user);
                    return Page();
                }

                var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
                if (Input.PhoneNumber != phoneNumber)
                {
                    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                    if (!setPhoneResult.Succeeded)
                    {
                        StatusMessage = "Unexpected error when trying to set phone number.";
                        return RedirectToPage();
                    }
                }
                var fullName = user.FullName;
                if (Input.FullName != fullName)
                {
                    user.FullName = Input.FullName;
                    var setFullNameResult = await _userManager.UpdateAsync(user);
                    if (!setFullNameResult.Succeeded)
                    {
                        StatusMessage = "Unexpected error when trying to set full name.";
                        return RedirectToPage();
                    }
                }
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string employeePath = Path.Combine(wwwRootPath, "images", "employee");
                    if (!Directory.Exists(employeePath))
                    {
                        Directory.CreateDirectory(employeePath);
                    }
                    string filePath = Path.Combine(employeePath, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    user.ImageUrl = "/images/employee/" + fileName;


                }


                await _signInManager.RefreshSignInAsync(user);
                TempData["success"] = "Successfully updated your profile";
                *//*     StatusMessage = "Your profile has been updated";*//*
                return RedirectToPage();
            }*/
    }
}
