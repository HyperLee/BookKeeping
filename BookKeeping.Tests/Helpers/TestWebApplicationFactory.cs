using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using BookKeeping.Data;
using BookKeeping.Models;

namespace BookKeeping.Tests.Helpers;

/// <summary>
/// Test web application factory with in-memory SQLite provider.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the app's DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<BookKeepingDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory SQLite database for testing
            services.AddDbContext<BookKeepingDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryBookKeepingTest");
            });

            // Build service provider and seed test data
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<BookKeepingDbContext>();

            db.Database.EnsureCreated();
            SeedTestData(db);
        });
    }

    /// <summary>
    /// Seeds test data into the in-memory database.
    /// </summary>
    /// <param name="context">The database context.</param>
    private static void SeedTestData(BookKeepingDbContext context)
    {
        // Seed expense categories
        foreach (var (name, icon, color) in Category.DefaultExpenseCategories)
        {
            context.Categories.Add(new Category
            {
                Name = name,
                Icon = icon,
                Type = TransactionType.Expense,
                Color = color,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Seed income categories
        foreach (var (name, icon, color) in Category.DefaultIncomeCategories)
        {
            context.Categories.Add(new Category
            {
                Name = name,
                Icon = icon,
                Type = TransactionType.Income,
                Color = color,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Seed default accounts
        foreach (var (name, type, icon) in Account.DefaultAccounts)
        {
            context.Accounts.Add(new Account
            {
                Name = name,
                Type = type,
                Icon = icon,
                InitialBalance = 0,
                Currency = "TWD",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        context.SaveChanges();
    }
}
