// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Fyp.Models;
using System.Net.Mail;
using System.Net;
using Microsoft.IdentityModel.Tokens;

namespace FypWeb.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

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
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

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
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        /*    public async Task OnGetAsync(string returnUrl = null)
            {
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    ModelState.AddModelError(string.Empty, ErrorMessage);
                }

                returnUrl ??= Url.Content("~/");

                // Clear the existing external cookie to ensure a clean login process
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

                ReturnUrl = returnUrl;
            }*/

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                // User is already logged in, check their roles for redirection
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    string selectedService = HttpContext.Session.GetString("SelectedService");

                    // Redirect based on roles
                    if (roles.Contains("Admin"))
                    {
                        return LocalRedirect(Url.Content("~/Admin/Home/Index"));
                    }
                    else if (roles.Contains("Employee"))
                    {
                        return LocalRedirect(Url.Content("~/Employee/Home/Index"));
                    }
                    else if (roles.Contains("Customer-Handler") && !string.IsNullOrEmpty(selectedService))
                    {
                        switch (selectedService)
                        {
                            case "SmartFittingRoom":
                                returnUrl = "/Customer/Home/TrailRoom";
                                break;
                            case "SmartCheckout":
                                returnUrl = "/SmartCheckout/Checkout/Index";
                                break;
                            case "RecommendationsCheckout":
                                returnUrl = "/Customer/Home/Index";
                                break;
                            default:
                                // Keep the default returnUrl if no service selected or for other roles
                                break;
                        }
                        return LocalRedirect(returnUrl);
                  
                    }
                    // Add more role checks as needed
                }

                // Default redirection if no specific role found
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;

            return Page();
        }


        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    string selectedService = Request.Form["selectedService"];
                    // Store selectedService in session if it's not null or empty
                    if (!string.IsNullOrEmpty(selectedService))
                    {
                        HttpContext.Session.SetString("SelectedService", selectedService);
                    }

                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);

                        if (roles.Contains("Admin"))
                        {
                            returnUrl = "/Admin/Home/Index";
                        }
                        else if (roles.Contains("Employee"))
                        {
                            returnUrl = "/Employee/Home/Index"; // Update this as needed
                        }
                        else if (roles.Contains("Customer-Handler") && !string.IsNullOrEmpty(selectedService) )
                        {
                            switch (selectedService)
                            {
                                case "SmartFittingRoom":
                                    returnUrl = "/Customer/Home/TrailRoom";
                                    break;
                                case "SmartCheckout":
                                    returnUrl = "/SmartCheckout/Checkout/Index";
                                    break;
                                case "RecommendationsCheckout":
                                    returnUrl = "/Customer/Home/Index";
                                    break;
                                default:
                                    // Keep the default returnUrl if no service selected or for other roles
                                    break;
                            }
                            return LocalRedirect(returnUrl);
                        }
                        else if (roles.Contains("Fitting Room Employee"))
                        {
                            returnUrl = "/FittingRoomEmployee/Home/AllNotifications"; // Update this as needed
                        }
                    }

                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    if (user != null)
                    {
                        var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                        bool emailSent = await Send2FACodeByEmail(user.Email, token);
                        if (!emailSent)
                        {
                            ModelState.AddModelError(string.Empty, "Failed to send 2FA code via email. Please try again.");
                            return Page();
                        }

                        // Store the user's email in session
                        HttpContext.Session.SetString("UserEmailFor2FA", user.Email);

                        TempData["2FAEmailSent"] = "Check your email for the 2FA code.";
                        return RedirectToPage("./Enter2FACode", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                        return Page();
                    }
                }


                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
        private async Task<bool> Send2FACodeByEmail(string email, string twoFactorCode)
        {
            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            message.From = new MailAddress("np03cs4s220079@heraldcollege.edu.np"); // Use your actual email
            message.To.Add(email);
            message.Subject = "Your 2FA Code";

            // Construct the email body with the 2FA code
            string body = $"<p>Your two-factor authentication code is: <strong>{twoFactorCode}</strong></p>" +
                           "<p>Please enter this code to complete your sign-in process.</p>";

            message.IsBodyHtml = true; // Since we're using HTML tags in the body
            message.Body = body;

            // SMTP server configuration
            smtp.Port = 587;
            smtp.Host = "smtp.gmail.com"; // Assuming Gmail. Adjust if using another email provider.
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("np03cs4s220079@heraldcollege.edu.np", "eizdbtlqjhheslfd"); // Use your actual credentials
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

            try
            {
                await smtp.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return false;
            }
        }


    }
}
