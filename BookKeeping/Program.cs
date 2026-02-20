using Microsoft.EntityFrameworkCore;

using Serilog;
using System.Security.Cryptography;

using Ganss.Xss;

using BookKeeping.Data;
using BookKeeping.Data.Seed;
using BookKeeping.Services;

namespace BookKeeping;

public class Program
{
    public static void Main(string[] args)
    {
        // Bootstrap Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .Build())
            .CreateLogger();

        try
        {
            Log.Information("Starting BookKeeping application");

            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog
            builder.Host.UseSerilog();

            // Add DbContext with SQLite
            builder.Services.AddDbContext<BookKeepingDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add HtmlSanitizer as singleton
            builder.Services.AddSingleton<HtmlSanitizer>();

            // Register application services
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<ITransactionService, TransactionService>();
            builder.Services.AddScoped<ICsvService, CsvService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IBudgetService, BudgetService>();

            // Add services to the container
            builder.Services.AddRazorPages();

            // Add anti-forgery token support
            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });

            var app = builder.Build();

            // Run database migrations and seed default data
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BookKeepingDbContext>();
                context.Database.Migrate();
                
                var seeder = new DefaultDataSeeder(context);
                seeder.SeedAsync().Wait();
                
                Log.Information("Database migration and seeding completed");
            }

            // Configure the HTTP request pipeline
            app.UseExceptionHandler("/Error");
            app.UseSerilogRequestLogging();

            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(
                        ex,
                        "Unhandled exception for {RequestMethod} {RequestPath}. TraceId={TraceId}",
                        context.Request.Method,
                        context.Request.Path,
                        context.TraceIdentifier);
                    throw;
                }
            });

            app.Use(async (context, next) =>
            {
                var cspNonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                context.Items["CspNonce"] = cspNonce;
                context.Response.Headers["Content-Security-Policy"] =
                    $"default-src 'self'; script-src 'self' 'nonce-{cspNonce}'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; object-src 'none'; frame-ancestors 'none'; base-uri 'self';";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                await next();
            });

            if (!app.Environment.IsDevelopment())
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
