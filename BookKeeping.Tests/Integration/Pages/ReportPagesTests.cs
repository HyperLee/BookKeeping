using System.Net;
using System.Text.Json;
using BookKeeping.Data;
using BookKeeping.Models;
using BookKeeping.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookKeeping.Tests.Integration.Pages;

public class ReportPagesTests
{
    [Fact]
    public async Task ReportsPage_OnGet_ShouldReturnOk()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/Reports");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReportsPage_ChartDataHandler_ShouldReturnValidJson()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        const int year = 2026;
        const int month = 2;
        await SeedMonthlyTransactionsAsync(factory.Services, year, month);

        var response = await client.GetAsync($"/Reports?handler=ChartData&year={year}&month={month}");
        var jsonContent = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(jsonContent);
        var root = json.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(root.TryGetProperty("categoryExpenses", out var categoryExpenses));
        Assert.Equal(JsonValueKind.Array, categoryExpenses.ValueKind);
        Assert.NotEqual(0, categoryExpenses.GetArrayLength());
        Assert.True(root.TryGetProperty("dailyTrends", out var dailyTrends));
        Assert.Equal(JsonValueKind.Array, dailyTrends.ValueKind);
        Assert.NotEqual(0, dailyTrends.GetArrayLength());
    }

    [Fact]
    public async Task ReportsPage_EmptyMonth_ShouldDisplayFriendlyMessage()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/Reports?year=1999&month=1");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("本月尚無紀錄", html);
    }

    private static async Task SeedMonthlyTransactionsAsync(IServiceProvider services, int year, int month)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookKeepingDbContext>();
        var expenseCategoryId = await context.Categories
            .Where(c => c.Type == TransactionType.Expense)
            .Select(c => c.Id)
            .FirstAsync();
        var incomeCategoryId = await context.Categories
            .Where(c => c.Type == TransactionType.Income)
            .Select(c => c.Id)
            .FirstAsync();
        var accountId = await context.Accounts
            .Select(a => a.Id)
            .FirstAsync();

        context.Transactions.AddRange(
            new Transaction
            {
                Date = new DateOnly(year, month, 1),
                Amount = 900m,
                Type = TransactionType.Income,
                CategoryId = incomeCategoryId,
                AccountId = accountId,
                Note = "report-income"
            },
            new Transaction
            {
                Date = new DateOnly(year, month, 2),
                Amount = 120m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategoryId,
                AccountId = accountId,
                Note = "report-expense"
            });
        await context.SaveChangesAsync();
    }
}
