using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ecom_bookshopping_1001.Data;
using ecom_bookshopping_1001.Models;

namespace ecom_bookshopping_1001.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            ILogger<LoginModel> logger,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public IList<AuthenticationScheme>? ExternalLogins { get; set; }

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = default!;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = default!;

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
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

            return Page();
        }

        public async Task<IActionResult> OnPostSendLoginOtpAsync([FromBody] LoginOtpRequest data)
        {
            if (data == null || string.IsNullOrEmpty(data.Email) || string.IsNullOrEmpty(data.Password))
            {
                return new JsonResult(new { success = false, message = "Enter Email and password !!" });
            }

            var user = await _userManager.FindByEmailAsync(data.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, data.Password))
            {
                string otp = new Random().Next(100000, 999999).ToString();

                HttpContext.Session.SetString("LoginOtp", otp);
                HttpContext.Session.SetString("OtpUserEmail", data.Email);
                HttpContext.Session.SetString("RememberMe", data.RememberMe.ToString());

                // ReturnUrl session me store karein agar frontend se pass hua ho
                if (!string.IsNullOrEmpty(data.ReturnUrl))
                {
                    HttpContext.Session.SetString("ReturnUrl", data.ReturnUrl);
                }

                string senderEmail = "paraskumar188643@gmail.com";
                string appPassword = "dgzinxkrwhjmmatk";

                try
                {
                    var smtpClient = new SmtpClient("smtp.gmail.com", 587)
                    {
                        EnableSsl = true,
                        Credentials = new NetworkCredential(senderEmail, appPassword)
                    };

                    var mailMessage = new MailMessage(
                        senderEmail,
                        data.Email,
                        "BookShopping - Login OTP",
                        $"Aapka login verification code hai: {otp}\nYeh code 5 minute ke liye valid hai."
                    );

                    await smtpClient.SendMailAsync(mailMessage);

                    return new JsonResult(new { success = true });
                }
                catch (Exception ex)
                {
                    return new JsonResult(new { success = false, message = "Email bhejne me issue aaya: " + ex.Message });
                }
            }

            return new JsonResult(new { success = false, message = "Email ya password galat hai!" });
        }

        public async Task<IActionResult> OnPostVerifyLoginOtpAsync([FromBody] OtpVerifyRequest data)
        {
            var sessionOtp = HttpContext.Session.GetString("LoginOtp");
            var userEmail = HttpContext.Session.GetString("OtpUserEmail");
            bool remember = bool.TryParse(HttpContext.Session.GetString("RememberMe"), out var r) && r;

            // Session ya Request body se ReturnUrl fetch karein
            var targetReturnUrl = HttpContext.Session.GetString("ReturnUrl");
            if (string.IsNullOrEmpty(targetReturnUrl) && !string.IsNullOrEmpty(data?.ReturnUrl))
            {
                targetReturnUrl = data.ReturnUrl;
            }

            if (!string.IsNullOrEmpty(sessionOtp) && sessionOtp == data?.EnteredOtp)
            {
                var user = await _userManager.FindByEmailAsync(userEmail!);
                if (user != null)
                {
                    await _signInManager.SignInAsync(user, isPersistent: remember);

                    HttpContext.Session.Remove("LoginOtp");
                    HttpContext.Session.Remove("OtpUserEmail");
                    HttpContext.Session.Remove("RememberMe");
                    HttpContext.Session.Remove("ReturnUrl");

                    // Agar targetReturnUrl valid local path hai toh wahi redirect karein
                    if (!string.IsNullOrEmpty(targetReturnUrl) && Url.IsLocalUrl(targetReturnUrl))
                    {
                        return new JsonResult(new { success = true, returnUrl = targetReturnUrl });
                    }

                    return new JsonResult(new { success = true, returnUrl = "/" });
                }
            }

            return new JsonResult(new { success = false, message = "Invalid ya Expired OTP. Dobara check karein." });
        }

        public class LoginOtpRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public bool RememberMe { get; set; }
            public string? ReturnUrl { get; set; }
        }

        public class OtpVerifyRequest
        {
            public string EnteredOtp { get; set; } = string.Empty;
            public string? ReturnUrl { get; set; }
        }
    }
}