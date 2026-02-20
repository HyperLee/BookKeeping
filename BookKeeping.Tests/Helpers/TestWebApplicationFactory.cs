using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data.Common;

using BookKeeping.Data;

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
            services.RemoveAll(typeof(DbContextOptions<BookKeepingDbContext>));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<BookKeepingDbContext>));
            services.RemoveAll(typeof(DbConnection));

            // Add in-memory SQLite database for testing
            services.AddSingleton<DbConnection>(_ =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();
                return connection;
            });
            services.AddDbContext<BookKeepingDbContext>((serviceProvider, options) =>
            {
                var connection = serviceProvider.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });
        });
    }
}
