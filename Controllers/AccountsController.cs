using Hoot.Data;
using Hoot.Models;
using Hoot.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Hoot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountsController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }
    [HttpGet("token")]
    public IActionResult GetToken()
    {
        if (Request.Cookies.TryGetValue("AuthToken", out var token))
        {
            return Ok(new { Token = token });
        }

        return Unauthorized(new { Message = "No token found" });
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(UserViewModel Input, string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email, Name = Input.Name, CreatedOn = DateTime.UtcNow };
            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    var callbackUrl = Url.Action(nameof(ConfirmEmail), null, new { userId = user.Id, code = code }, Request.Scheme);
                    //SendService.SendMail(Input.Email, "Confirm your email | Innovus", $"<div style=\"width:100%;height:100%;background-color:#f5f5f9;box-sizing:border-box;font-family:sans-serif\"><div style=\"max-width:480px;background-color:#fff;padding:40px;font-family:sans-serif;margin:auto;color:#121212\"><div style=\"width:160px;margin-bottom:32px\"><img src=\"https://www.innovustech.in/img/logo.png\" style=\"margin:auto;display:inline-block;width:160px\"></div><hr style=\"border:1px solid #ccc;margin:32px 0\"><h1 style=\"color:#121212;font-size:40px;line-height:44px;margin-bottom:32px\">Verify your e-mail to finish signing up for Innovus</h1><p style=\"font-family:sans-serif;line-height:1.45em;margin-bottom:32px\">Thank you for choosing Innovus.<br><br>Please confirm that <b>{user.Email}</b> is your email address by clicking on the button below.</p><a style=\"text-decoration:none;color:#fff;background-color:#0000bc;box-sizing:border-box;padding:16px 32px;display:inline-flex;color:#fff;font-family:sans-serif;border-radius:6px;font-weight:600;cursor:pointer;width:fit-content\" href=\"{HtmlEncoder.Default.Encode(callbackUrl)}\">Verify Your Email</a><hr style=\"border:1px solid #ccc;margin:32px 0\"><div style=\"text-align:center;margin-bottom:32px;color:#525252;font-size:14px\"><p>Need help? Ask at<a style=\"text-decoration:none;color:#0000bc\" href=\"mailto:support@innovustech.in\">support@innovustech.in</a></p><p>Innovus</p></div></div></div>");
                    //SendService.SendMail(Input.Email, "Confirm your email | Innovus", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    //return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl 
                    return RedirectToAction(nameof(EmailVerificationSent));
                }
                else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // If we got this far, something failed, redisplay form
        return View();
    }

    [HttpGet("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        ViewBag.ShowModelError = "collapse";

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
        ViewBag.StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
        ViewBag.Alert = result.Succeeded ? "success" : "danger";

        return View();
    }
    [HttpGet]
    public IActionResult EmailVerificationSent()
    {
        return View();
    }
    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }
    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }
    public IActionResult ResetPassword(string code = null, string userid = null)
    {
        if (code == null || userid == null)
        {
            return BadRequest("A code must be supplied for password reset.");
        }
        else
        {
            var user = _userManager.FindByIdAsync(userid);
            if (user == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }

            var resetPassword = new ResetPasswordViewModel
            {
                Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)),
                Email = user.Result.Email,
            };
            return View(resetPassword);
        }
        //ViewBag.ShowModelError = "collapse";
        //return View();
    }
    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel input)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        var user = await _userManager.FindByEmailAsync(input.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        var result = await _userManager.ResetPasswordAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(input.Code)), input.Password);

        if (result.Succeeded)
        {
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return View(input);
    }

    [HttpPost("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword(EmailViewModel input)
    {
        ViewBag.ShowModelError = "collapse";
        if (!ModelState.IsValid)
        {
            return View();
        }

        var user = await _userManager.FindByEmailAsync(input.Email);
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            // Don't reveal that the user does not exist or is not confirmed
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        // For more information on how to enable account confirmation and password reset please
        // visit https://go.microsoft.com/fwlink/?LinkID=532713
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = $"{Request.Scheme}://{this.Request.Host}/Account/ResetPassword?code={code}&userid={user.Id}";

        //SendService.SendMail(input.Email, "Reset Password | Innovus", $"<div style=\"width:100%;height:100%;background-color:#f5f5f9;box-sizing:border-box;font-family:sans-serif\"><div style=\"max-width:480px;background-color:#fff;padding:40px;font-family:sans-serif;margin:auto;color:#121212\"><div style=\"width:160px;margin-bottom:32px\"><img src=\"https://www.innovustech.in/img/logo.png\" style=\"margin:auto;display:inline-block;width:160px\"></div><hr style=\"border:1px solid #ccc;margin:32px 0\"><h1 style=\"color:#121212;font-size:40px;line-height:44px;margin-bottom:32px\">Reset your Innovus password</h1><p style=\"font-family:sans-serif;line-height:1.45em;margin-bottom:32px\">Hi {user.Name},<br>We're sending you this email because you requested a password reset. Click on below button to create a new password.</p><a style=\"text-decoration:none;color:#fff;background-color:#0000bc;box-sizing:border-box;padding:16px 32px;display:inline-flex;color:#fff;font-family:sans-serif;border-radius:6px;font-weight:600;cursor:pointer;width:fit-content;margin-bottom:32px\" href=\"{HtmlEncoder.Default.Encode(callbackUrl)}\">Reset Password</a><p style=\"font-family:sans-serif;line-height:1.45em;margin:0\">If you didn't request a password reset, you can ignore this email. Your password will not be changed.</p><hr style=\"border:1px solid #ccc;margin:32px 0\"><div style=\"text-align:center;margin-bottom:32px;color:#525252;font-size:14px\"><p>Need help? Ask at<a style=\"text-decoration:none;color:#0000bc\" href=\"mailto:support@innovustech.in\">support@innovustech.in</a></p><p>Innovus</p></div></div></div>");

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpPost("VerifyEmail")]
    public async Task<IActionResult> VerifyEmailAddress(EmailViewModel input)
    {
        ViewBag.ShowModelError = "collapse";
        if (!ModelState.IsValid)
        {
            return View();
        }

        var user = await _userManager.FindByEmailAsync(input.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "We couldn't find an account with that email address.");
            return View();
        }

        var userId = await _userManager.GetUserIdAsync(user);

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Action(nameof(ConfirmEmail), null, new { userId = user.Id, code = code }, Request.Scheme);
        //SendService.SendMail(input.Email, "Confirm your email | Innovus", $"<div style=\"width:100%;height:100%;background-color:#f5f5f9;box-sizing:border-box;font-family:sans-serif\"><div style=\"max-width:480px;background-color:#fff;padding:40px;font-family:sans-serif;margin:auto;color:#121212\"><div style=\"width:160px;margin-bottom:32px\"><img src=\"https://www.innovustech.in/img/logo.png\" style=\"margin:auto;display:inline-block;width:160px\"></div><hr style=\"border:1px solid #ccc;margin:32px 0\"><h1 style=\"color:#121212;font-size:40px;line-height:44px;margin-bottom:32px\">Verify your e-mail to finish signing up for Innovus</h1><p style=\"font-family:sans-serif;line-height:1.45em;margin-bottom:32px\">Thank you for choosing Innovus.<br><br>Please confirm that <b>{user.Email}</b> is your email address by clicking on the button below.</p><a style=\"text-decoration:none;color:#fff;background-color:#0000bc;box-sizing:border-box;padding:16px 32px;display:inline-flex;color:#fff;font-family:sans-serif;border-radius:6px;font-weight:600;cursor:pointer;width:fit-content\" href=\"{HtmlEncoder.Default.Encode(callbackUrl)}\">Verify Your Email</a><hr style=\"border:1px solid #ccc;margin:32px 0\"><div style=\"text-align:center;margin-bottom:32px;color:#525252;font-size:14px\"><p>Need help? Ask at<a style=\"text-decoration:none;color:#0000bc\" href=\"mailto:support@innovustech.in\">support@innovustech.in</a></p><p>Innovus</p></div></div></div>");

        ViewBag.ShowModelError = string.Empty;
        //ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
        ViewBag.GetModelError = "Verification email sent. Please check your email.";
        return View(input);
    }
}
