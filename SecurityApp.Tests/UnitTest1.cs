using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using SecurityApp.Data;
using NUnit.Framework;
using SecurityApp.Helper;
using SecurityApp.Models;
using SecurityApp.Pages;
using SecurityApp.Services;

namespace SecurityApp.Tests;

[TestFixture]
public class SecurityRegressionTests
{
    [Test]
    public void SqlInjectionSyntaxIsRejectedInUsernames()
    {
        var user = new User
        {
            Username = "admin' OR '1'='1",
            Email = "test@example.com"
        };

        var errors = ValidationHelper.ValidateUser(user);

        Assert.That(errors, Does.ContainKey(nameof(User.Username)));
    }

    [Test]
    public void ScriptMarkupIsRejectedInUsernames()
    {
        var user = new User
        {
            Username = "<script>alert('XSS')</script>",
            Email = "test@example.com"
        };

        var errors = ValidationHelper.ValidateUser(user);

        Assert.That(errors, Does.ContainKey(nameof(User.Username)));
    }

    [Test]
    public void ValidUserShouldPassValidation()
    {
        var user = new User
        {
            Username = "john_doe",
            Email = "john@example.com"
        };

        var errors = ValidationHelper.ValidateUser(user);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void InvalidEmailIsRejected()
    {
        Assert.That(ValidationHelper.ValidateEmail("' OR '1'='1"), Is.Not.Null);
    }

    [Test]
    public void ShortPasswordIsRejected()
    {
        Assert.That(ValidationHelper.ValidatePassword("short"), Is.Not.Null);
    }

    [Test]
    public void EfCoreEmailFilterParameterizesSqlInjectionPayload()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql("Server=localhost;Database=security_test;", new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;
        using var dbContext = new AppDbContext(options);
        var injectedEmail = "' OR 1=1 --";

        var sql = dbContext.Users
            .Where(user => user.Email == injectedEmail)
            .ToQueryString();
        var command = sql[sql.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase)..];

        Assert.That(command, Does.Contain("= ''' OR 1=1 --'"));
        Assert.That(command, Does.Not.Contain("= '' OR 1=1 --"));
    }

    [Test]
    public async Task UnauthenticatedUserCannotDeleteUsers()
    {
        using var dbContext = CreateUnconfiguredDbContext();
        var page = new IndexModel(dbContext);
        page.PageContext = CreatePageContext(new ClaimsPrincipal(new ClaimsIdentity()));

        var result = await page.OnPostDeleteAllUsersAsync();

        Assert.That(result, Is.TypeOf<ChallengeResult>());
    }

    [Test]
    public async Task NonAdminUserCannotDeleteUsers()
    {
        using var dbContext = CreateUnconfiguredDbContext();
        var page = new IndexModel(dbContext);
        page.PageContext = CreatePageContext(CreatePrincipal("User"));

        var result = await page.OnPostDeleteAllUsersAsync();

        Assert.That(result, Is.TypeOf<ForbidResult>());
    }

    private static ClaimsPrincipal CreatePrincipal(string role)
    {
        var claims = new[] { new Claim(ClaimTypes.Role, role) };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    private static PageContext CreatePageContext(ClaimsPrincipal principal)
    {
        var httpContext = new DefaultHttpContext { User = principal };
        return new PageContext
        {
            HttpContext = httpContext,
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        };
    }

    private static AppDbContext CreateUnconfiguredDbContext()
    {
        return new AppDbContext(new DbContextOptions<AppDbContext>());
    }
}