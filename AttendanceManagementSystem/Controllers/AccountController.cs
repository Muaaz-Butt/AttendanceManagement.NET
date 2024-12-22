using AttendanceManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims; 


namespace AttendanceManagementSystem.Controllers
{
  public class AccountController : Controller
  {
    private readonly AppilcationDBContext? _context;
    private readonly PasswordHasher<User> _passwordHasher;

    public AccountController(AppilcationDBContext context)
    {
      _context = context;
      _passwordHasher = new PasswordHasher<User>();
    }

    // Register GET
    public IActionResult Register()
    {
      return View();
    }

    // Register POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(User user)
    {
      if (ModelState.IsValid)
      {
        if (await _context.Users.AnyAsync(u => u.Email == user.Email))
        {
          ModelState.AddModelError("Email", "Email is already registered");
          return View(user);
        }
        if (user.Email.Contains("stu") || user.Email.Contains("student"))
        {
          user.Role = "Student";
        }
        else
        {
          user.Role = "Teacher";
        }

        user.Password = _passwordHasher.HashPassword(user, user.Password);

        _context.Add(user);
        await _context.SaveChangesAsync();
        return RedirectToAction("Login", "Account");
      }
      return View(user);
    }

    // Login GET
    [HttpGet]
    public IActionResult Login()
    {
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
      // Find the user by email
      var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

      if (user == null)
      {
        ModelState.AddModelError("", "Invalid login attempt.");
        return View();
      }

      // Verify the password
      var passwordVerification = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
      if (passwordVerification == PasswordVerificationResult.Failed)
      {
        ModelState.AddModelError("", "Invalid login attempt.");
        return View();
      }

      // If login is successful, check role and create authentication cookie
      if (user.Role == "Teacher")
      {
        // Create claims for the user
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, "Teacher")
            };

        // Create the identity and sign-in the user
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

        return RedirectToAction("TeacherDashboard", "Dashboard");
      }
      else if (user.Role == "Student")
      {
        // Similarly, handle student login (if needed)
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, "Student")
            };

        // Create the identity and sign-in the user
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

        return RedirectToAction("StudentDashboard", "Dashboard");
      }

      // Default redirection if no valid role
      ModelState.AddModelError("", "Invalid login attempt.");
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
      // Clear the authentication cookies (this will log the user out)
      await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

      // Redirect to the Login page
      return RedirectToAction("Login");
    }
    // Forgot Password GET
    public IActionResult ForgotPassword()
    {
      return View();
    }

    // Forgot Password POST
    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

      if (user == null)
      {
        ViewData["Message"] = "If this email exists, a reset link has been sent.";
        return View();
      }

      var resetToken = Guid.NewGuid().ToString();
      user.ResetToken = resetToken;
      user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Set expiration time for the token

      await _context.SaveChangesAsync();

      var resetLink = Url.Action("ResetPassword", "Account", new { token = resetToken }, Request.Scheme);

      await SendResetEmailAsync(model.Email, resetLink);

      ViewData["Message"] = "A password reset link has been sent to your email.";
      return View();
    }

    // Send Reset Email
    private async Task SendResetEmailAsync(string? email, string? resetLink)
    {
      var fromAddress = new MailAddress("buttzayyad123@gmail.com", "AttendanceSystem");
      var toAddress = new MailAddress(email);
      const string fromPassword = "tsfu bviz ezqs ecta";
      const string subject = "Password Reset Request";
      string body = $"Click on the following link to reset your password: {resetLink}";

      var smtp = new SmtpClient
      {
        Host = "smtp.gmail.com",
        Port = 587,
        EnableSsl = true,
        DeliveryMethod = SmtpDeliveryMethod.Network,
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
      };

      using (var message = new MailMessage(fromAddress, toAddress)
      {
        Subject = subject,
        Body = body,
        IsBodyHtml = false
      })
      {
        try
        {
          await smtp.SendMailAsync(message);
          Console.WriteLine("Email sent successfully!");
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Failed to send email: {ex.Message}");
          throw;
        }
      }
    }

    // Reset Password GET
    [HttpGet]
    public async Task<IActionResult> ResetPassword(string token)
    {
      if (string.IsNullOrEmpty(token))
      {
        return RedirectToAction("Index", "Home"); // Redirect if no token provided
      }

      var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.UtcNow);

      if (user == null)
      {
        return RedirectToAction("Index", "Home"); // Invalid token or expired
      }

      return View(new ResetPasswordViewModel { Token = token });
    }

    // Reset Password POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
      if (ModelState.IsValid)
      {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == model.Token && u.ResetTokenExpiry > DateTime.UtcNow);

        if (user == null)
        {
          ModelState.AddModelError(string.Empty, "Invalid or expired token.");
          return View(model);
        }

        if (model.NewPassword != model.ConfirmPassword)
        {
          ModelState.AddModelError(string.Empty, "Passwords do not match.");
          return View(model);
        }

        // Hash the new password and update
        user.Password = _passwordHasher.HashPassword(user, model.NewPassword);

        // Clear the reset token and expiry
        user.ResetToken = null;
        user.ResetTokenExpiry = null;

        await _context.SaveChangesAsync();

        // Redirect to login page after password reset
        return RedirectToAction("Login", "Account");
      }

      return View(model);
    }
  }
}
