using System.Net.Mail;
using System.Text.RegularExpressions;
using SecurityApp.Models;

namespace SecurityApp.Helper;

public static partial class ValidationHelper
{
    private const int MaximumUsernameLength = 100;
    private const int MaximumEmailLength = 100;

    public static Dictionary<string, string> ValidateUser(User? user)
    {
        var errors = new Dictionary<string, string>();

        if (user is null)
        {
            errors["User"] = "User is required.";
            return errors;
        }

        var usernameError = ValidateUsername(user.Username);
        if (usernameError is not null)
        {
            errors[nameof(User.Username)] = usernameError;
        }

        var emailError = ValidateEmail(user.Email);
        if (emailError is not null)
        {
            errors[nameof(User.Email)] = emailError;
        }

        return errors;
    }

    public static string? ValidateUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return "Username is required.";
        }

        if (username.Length > MaximumUsernameLength)
        {
            return $"Username must be {MaximumUsernameLength} characters or fewer.";
        }

        if (!UsernamePattern().IsMatch(username))
        {
            return "Username may contain only letters, numbers, underscores, and hyphens.";
        }

        return null;
    }

    public static string? ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "Email is required.";
        }

        if (email.Length > MaximumEmailLength)
        {
            return $"Email must be {MaximumEmailLength} characters or fewer.";
        }

        try
        {
            var address = new MailAddress(email);
            if (!string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase))
            {
                return "Email format is invalid.";
            }
        }
        catch (FormatException)
        {
            return "Email format is invalid.";
        }

        return null;
    }

    public static string? ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return "Password is required.";
        }

        if (password.Length < 8)
        {
            return "Password must be at least 8 characters long.";
        }

        if (System.Text.Encoding.UTF8.GetByteCount(password) > 72)
        {
            return "Password must not exceed 72 bytes.";
        }

        return null;
    }

    [GeneratedRegex("^[a-zA-Z0-9_-]+$")]
    private static partial Regex UsernamePattern();
}
