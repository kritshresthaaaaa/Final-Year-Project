using Fyp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace FypWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class Enter2FACodeModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public Enter2FACodeModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(8, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [Display(Name = "2FA Code")]
            public string TwoFactorCode { get; set; }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var userEmail = HttpContext.Session.GetString("UserEmailFor2FA");

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                // Log the absence of userEmail or handle it accordingly
                ModelState.AddModelError(string.Empty, "Session expired or user email not found.");
                return Page();
            }

         

            // Find the user by their email or username
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to load user.");
                return Page();
            }

            var result = await _signInManager.TwoFactorSignInAsync("Email", Input.TwoFactorCode, false, rememberClient: false);
            if (result.Succeeded)
            {
                HttpContext.Session.Remove("UserEmailFor2FA");
                                 
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                {
                    returnUrl = "/Admin/Home/Index";
                }
                else if (roles.Contains("Sales Employee"))
                {
                    return LocalRedirect("/SalesEmployee/Home/Index");
                }
                // Add additional roles and redirections as needed...

                // If user's role doesn't match any of the above, redirect to a default page
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }
            else if (result.IsLockedOut)
            {
                // Handle lockout scenario
                return RedirectToPage("./Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid 2FA code.");
                return Page();
            }
        }

    }
}
