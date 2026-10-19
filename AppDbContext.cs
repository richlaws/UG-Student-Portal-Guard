using System;
using Microsoft.EntityFrameworkCore;

namespace s;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<Course> Courses { get; set; }

    public DbSet<RegistrationRule> RegistrationRules { get; set; }

    public DbSet<RegisteredCourse> RegisteredCourses { get; set; }

    protected override void OnConfiguring(
        DbContextOptionsBuilder options)
    {
        if (options.IsConfigured)
            return;

        string? conn =
            Environment.GetEnvironmentVariable(
                "UG_DATABASE_CONNECTION");

        if (string.IsNullOrWhiteSpace(conn))
        {
            throw new InvalidOperationException(
                "The MySQL database connection is not configured. " +
                "Please set the UG_DATABASE_CONNECTION environment variable.");
        }

        options.UseMySql(
            conn,
            ServerVersion.AutoDetect(conn));
    }
}
