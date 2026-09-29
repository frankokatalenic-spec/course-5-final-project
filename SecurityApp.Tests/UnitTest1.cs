using NUnit.Framework;
using SecurityApp.Helper;
using SecurityApp.Models;

namespace SecurityApp.Tests;

[TestFixture]
public class TestInputValidation
{
    [Test]
    public void TestForSQLInjection()
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
    public void TestForXSS()
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
}