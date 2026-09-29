using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityApp.Data;
using SecurityApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace SecurityApp.Pages;

public class IndexModel(AppDbContext dbContext) : PageModel
{
    public IReadOnlyList<User> Users { get; private set; } = [];

    public string? StatusMessage { get; private set; }

    public async Task OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            Users = await dbContext.Users
                .AsNoTracking()
                .OrderBy(user => user.UserID)
                .ToListAsync();
        }
        else
        {
            Users = [];
        }
    }

    public async Task<IActionResult> OnPostDeleteAllUsersAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge();
        }

        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var deletedCount = await dbContext.Users.ExecuteDeleteAsync();

        Users = [];
        StatusMessage = $"Deleted {deletedCount} users.";
        return Page();
    }

    public async Task<IActionResult> OnPostSeedUsersAsync()
    {
        var sampleUsers = new[]
        {
            new User { Username = "alice", Email = "alice@example.com" },
            new User { Username = "bob", Email = "bob@example.com" },
            new User { Username = "carol", Email = "carol@example.com" }
        };

        dbContext.Users.AddRange(sampleUsers);
        await dbContext.SaveChangesAsync();

        Users = await dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.UserID)
            .ToListAsync();

        StatusMessage = $"Inserted {sampleUsers.Length} users. Showing {Users.Count} users from the database.";
        return Page();
    }
}
