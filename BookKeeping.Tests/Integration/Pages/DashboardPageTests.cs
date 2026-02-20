using System.Net;
using System.Text.RegularExpressions;
using BookKeeping.Data;
using BookKeeping.Models;
using BookKeeping.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookKeeping.Tests.Integration.Pages;

public class DashboardPageTests
{
    [Fact]
    public async Task DashboardPage_OnGet_ShouldRenderSummaryAccountBalancesBudgetProgressAndRecentTransactions()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        await SeedDashboardDataAsync(factory.Services);

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("本月收入", html);
        Assert.Contains("本月支出", html);
        Assert.Contains("結餘", html);
        Assert.Contains("$2,000.00", html);
        Assert.Contains("$660.00", html);
        Assert.Contains("$1,340.00", html);
        Assert.Contains("帳戶餘額", html);
        Assert.Contains("role=\"progressbar\"", html);
        Assert.Equal(10, CountRecentTransactionItems(html));
    }

    private static int CountRecentTransactionItems(string html)
    {
        var match = Regex.Match(
            html,
            "<h5 class=\"mb-0\">最近交易</h5>(.*?)</ul>",
            RegexOptions.Singleline);

        if (!match.Success)
        {
            return 0;
        }

        return Regex.Matches(match.Groups[1].Value, "<li class=\"list-group-item\">").Count;
    }

    private static async Task SeedDashboardDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookKeepingDbContext>();
        var now = DateOnly.FromDateTime(DateTime.Today);
        var monthStart = new DateOnly(now.Year, now.Month, 1);
        var accountId = await context.Accounts.Select(account => account.Id).FirstAsync();
        var expenseCategoryId = await context.Categories
            .Where(category => category.Type == TransactionType.Expense)
            .Select(category => category.Id)
            .FirstAsync();
        var incomeCategoryId = await context.Categories
            .Where(category => category.Type == TransactionType.Income)
            .Select(category => category.Id)
            .FirstAsync();

        context.Budgets.Add(new Budget
        {
            CategoryId = expenseCategoryId,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = monthStart
        });

        var transactions = new List<Transaction>
        {
            new()
            {
                Date = monthStart,
                Amount = 2000m,
                Type = TransactionType.Income,
                CategoryId = incomeCategoryId,
                AccountId = accountId,
                Note = "dashboard-income"
            }
        };

        for (var day = 2; day <= 12; day++)
        {
            transactions.Add(new Transaction
            {
                Date = new DateOnly(now.Year, now.Month, day),
                Amount = 60m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategoryId,
                AccountId = accountId,
                Note = $"dashboard-expense-{day}"
            });
        }

        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();
    }
}
