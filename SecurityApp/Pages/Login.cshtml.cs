using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SecurityApp.Data;
using SecurityApp.Models;
using SecurityApp.Services;

namespace SecurityApp.Pages;

public class LoginModel(AppDbContext dbContext, JwtTokenService jwtTokenService) : PageModel
{
    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var normalizedEmail = Email?.Trim();
        var user = normalizedEmail is null
            ? null
            : await dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Email == normalizedEmail);

        if (user is null || string.IsNullOrWhiteSpace(user.Password) ||
            string.IsNullOrWhiteSpace(Password) || !BCrypt.Net.BCrypt.Verify(Password, user.Password))
        {
            ErrorMessage = "Invalid username or password.";
            return Page();
        }

        var role = user.Role ?? "User";

        var jwtToken = jwtTokenService.GenerateToken(user.UserID.ToString(), user.Email, role);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
            });

        // Response.Cookies.Append("SecurityApp.AuthCookie", jwtToken, new CookieOptions
        // {
        //     HttpOnly = true,
        //     Secure = Request.IsHttps,
        //     SameSite = SameSiteMode.Lax,
        //     Expires = DateTimeOffset.UtcNow.AddMinutes(60)
        // });

        return RedirectToPage("/Index");
    }
}