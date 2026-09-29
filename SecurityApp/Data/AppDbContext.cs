using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using SecurityApp.Models;

namespace SecurityApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.UserID);
            entity.Property(user => user.UserID)
                .ValueGeneratedOnAdd();
            entity.Property(user => user.Username)
                .HasMaxLength(100);
            entity.Property(user => user.Email)
                .HasMaxLength(100);
            entity.Property(user => user.Password)
                .HasMaxLength(72);
            entity.Property(user => user.Role)
                .HasMaxLength(50)
                .HasDefaultValue("User");
        });
    }
}
