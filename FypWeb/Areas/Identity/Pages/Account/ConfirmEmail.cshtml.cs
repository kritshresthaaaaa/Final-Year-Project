// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Linq;
using System.Net.Mail;
using System.Net;
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
        /// 
        private async Task<bool> SendEmailAsync(string email, string subject, string confirmLink)
        {
            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            message.From = new MailAddress("np03cs4s220079@heraldcollege.edu.np");
            message.To.Add(email);
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = confirmLink;
            smtp.Port = 587;
            smtp.Host = "smtp.gmail.com";
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("np03cs4s220079@heraldcollege.edu.np", "eizdbtlqjhheslfd");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            try
            {
                await smtp.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }


        }
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
                    else
                    {
                        try
                        {
                            var emailContent = $"Dear {user.FullName},\n\n" +
                                "Your email has been confirmed. You can now login to your account.\n\n" +
                                "Customer Handler account has been created for you. You can now login to your account using the following credentials:\n" +
                                $"Username: {customerHandlerUser.Email}\n" + // Assuming email is used as username
                                $"Password: Same as your employee account \n\n" + // You should provide a secure way to send passwords
                                "Thank you for using our service.\n\n" +
                                "Best regards,\n" +
                                "Customer Handler Team";

                            var isEmailSent = await SendEmailAsync(user.Email, "Account Confirmation and Customer Handler Account Details", emailContent);

                            if (!isEmailSent)
                            {
                                // Handle the case where email sending failed
                                ModelState.AddModelError(string.Empty, "Error sending email to user about customer handler account creation.");
                                StatusMessage = "Error sending email to user about customer handler account creation.";
                                return Page();
                            }

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
            }
            StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
            return Page();
        }
    }
}
