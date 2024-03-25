// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fyp.Models;
using Fyp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace FypWeb.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }
        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(SD.Role_Employee))
            {
                // Confirm the email for the customer handler account associated with this employee
                var employeeEmailPrefix = user.Email.Split('@')[0];
                var customerHandlerEmail = $"{employeeEmailPrefix}@customerhandler.com";
                var customerHandlerUser = await _userManager.FindByEmailAsync(customerHandlerEmail);

                if (customerHandlerUser != null)
                {
                    // Directly setting the EmailConfirmed field to true
                    customerHandlerUser.EmailConfirmed = true;
                    var customerHandlerResult = await _userManager.UpdateAsync(customerHandlerUser);

                    // Optionally, log success or failure
                    if (!customerHandlerResult.Succeeded)
                    {
                        // Log the error or add to ModelState to display in the view
                        foreach (var error in result.Errors)
                        {
                       
                            ModelState.AddModelError(string.Empty, $"Error confirming your email: {error.Description}");
                        }
                        StatusMessage = "Error confirming your email.";
                        return Page();
                    }
                }
            }
            StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
            return Page();
        }
    }
}
