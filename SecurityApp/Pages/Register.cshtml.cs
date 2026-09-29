using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SecurityApp.Data;
using SecurityApp.Helper;
using SecurityApp.Models;

namespace SecurityApp.Pages;

public class RegisterModel(AppDbContext dbContext) : PageModel
{
    [BindProperty]
    public string? Email { get; set; }

    [BindProperty]
    public string? Password { get; set; }

    public string? SuccessMessage { get; private set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var emailError = ValidationHelper.ValidateEmail(Email);
        if (emailError is not null)
        {
            ModelState.AddModelError(nameof(Email), emailError);
        }

        var passwordError = ValidationHelper.ValidatePassword(Password);
        if (passwordError is not null)
        {
            ModelState.AddModelError(nameof(Password), passwordError);
        }

        var normalizedEmail = Email?.Trim();
        if (normalizedEmail is not null && await dbContext.Users.AnyAsync(user => user.Email == normalizedEmail))
        {
            ModelState.AddModelError(nameof(Email), "An account with this email already exists.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new User
        {
            Username = null,
            Email = normalizedEmail,
            Password = BCrypt.Net.BCrypt.HashPassword(Password!),
            Role = normalizedEmail.Contains("@globaldizajn.hr") ? "Admin" : "User"
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Password = null;
        SuccessMessage = "Registration successful.";
        return Page();
    }
}