using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using BookKeeping.Data;
using BookKeeping.Models;
using BookKeeping.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookKeeping.Tests.Integration.Pages;

public class TransactionPagesTests
{
    [Fact]
    public async Task CreatePage_OnGet_ShouldReturnOk()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/Transactions/Create");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreatePage_OnPostWithValidInput_ShouldRedirectAndPersistTransaction()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var (categoryId, accountId) = await GetReferenceIdsAsync(factory.Services);
        var getResponse = await client.GetAsync("/Transactions/Create");
        var html = await getResponse.Content.ReadAsStringAsync();
        var token = ExtractRequestVerificationToken(html);
        var note = $"integration-create-{Guid.NewGuid():N}";

        var postResponse = await client.PostAsync(
            "/Transactions/Create",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Input.Date"] = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["Input.Amount"] = "123.45",
                ["Input.Type"] = ((int)TransactionType.Expense).ToString(CultureInfo.InvariantCulture),
                ["Input.CategoryId"] = categoryId.ToString(CultureInfo.InvariantCulture),
                ["Input.AccountId"] = accountId.ToString(CultureInfo.InvariantCulture),
                ["Input.Note"] = note
            }));

        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.NotNull(postResponse.Headers.Location);
        Assert.Contains("/Transactions", postResponse.Headers.Location!.ToString());

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookKeepingDbContext>();
        var persisted = await context.Transactions.SingleAsync(t => t.Note == note);

        Assert.Equal(123.45m, persisted.Amount);
        Assert.Equal(TransactionType.Expense, persisted.Type);
        Assert.Equal(categoryId, persisted.CategoryId);
        Assert.Equal(accountId, persisted.AccountId);
    }

    [Fact]
    public async Task CreatePage_OnPostWithInvalidInput_ShouldReturnValidationError()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var (categoryId, accountId) = await GetReferenceIdsAsync(factory.Services);
        var getResponse = await client.GetAsync("/Transactions/Create");
        var html = await getResponse.Content.ReadAsStringAsync();
        var token = ExtractRequestVerificationToken(html);

        var note = $"integration-invalid-{Guid.NewGuid():N}";
        var postResponse = await client.PostAsync(
            "/Transactions/Create",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Input.Date"] = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["Input.Amount"] = "0",
                ["Input.Type"] = ((int)TransactionType.Expense).ToString(CultureInfo.InvariantCulture),
                ["Input.CategoryId"] = categoryId.ToString(CultureInfo.InvariantCulture),
                ["Input.AccountId"] = accountId.ToString(CultureInfo.InvariantCulture),
                ["Input.Note"] = note
            }));

        var postHtml = await postResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        Assert.Contains("field-validation-error", postHtml);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookKeepingDbContext>();
        Assert.Null(await context.Transactions.SingleOrDefaultAsync(t => t.Note == note));
    }

    [Fact]
    public async Task EditPage_OnGet_ShouldLoadExistingTransaction()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        var note = $"integration-edit-{Guid.NewGuid():N}";
        var transactionId = await CreateTransactionAsync(factory.Services, note);

        var response = await client.GetAsync($"/Transactions/Edit?id={transactionId}");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(note, html);
    }

    [Fact]
    public async Task IndexPage_OnPostDelete_ShouldSoftDeleteTransaction()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var transactionId = await CreateTransactionAsync(factory.Services, $"integration-delete-{Guid.NewGuid():N}");
        var getResponse = await client.GetAsync("/Transactions/Index");
        var html = await getResponse.Content.ReadAsStringAsync();
        var token = ExtractRequestVerificationToken(html);

        var postResponse = await client.PostAsync(
            $"/Transactions/Index?handler=Delete&id={transactionId}",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            }));

        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.NotNull(postResponse.Headers.Location);
        Assert.Contains("/Transactions", postResponse.Headers.Location!.ToString());

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookKeepingDbContext>();
        var deleted = await context.Transactions
            .IgnoreQueryFilters()
            .SingleAsync(t => t.Id == transactionId);

        Assert.True(deleted.IsDeleted);
        Assert.NotNull(deleted.DeletedAt);
    }

    private static async Task<(int CategoryId, int AccountId)> GetReferenceIdsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookKeepingDbContext>();
        var categoryId = await context.Categories
            .Where(c => c.Type == TransactionType.Expense)
            .Select(c => c.Id)
            .FirstAsync();
        var accountId = await context.Accounts
            .Select(a => a.Id)
            .FirstAsync();

        return (categoryId, accountId);
    }

    private static async Task<int> CreateTransactionAsync(IServiceProvider services, string note)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookKeepingDbContext>();
        var categoryId = await context.Categories
            .Where(c => c.Type == TransactionType.Expense)
            .Select(c => c.Id)
            .FirstAsync();
        var accountId = await context.Accounts
            .Select(a => a.Id)
            .FirstAsync();

        var transaction = new Transaction
        {
            Date = DateOnly.FromDateTime(DateTime.Today),
            Amount = 77.77m,
            Type = TransactionType.Expense,
            CategoryId = categoryId,
            AccountId = accountId,
            Note = note
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        return transaction.Id;
    }

    private static string ExtractRequestVerificationToken(string html)
    {
        var match = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase);

        return match.Success
            ? WebUtility.HtmlDecode(match.Groups[1].Value)
            : throw new InvalidOperationException("Unable to locate request verification token.");
    }
}
