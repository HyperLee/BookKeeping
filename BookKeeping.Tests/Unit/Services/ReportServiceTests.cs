using BookKeeping.Data;
using BookKeeping.Models;
using BookKeeping.Services;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Tests.Unit.Services;

public class ReportServiceTests
{
    [Fact]
    public async Task GetMonthlySummaryAsync_ShouldReturnIncomeExpenseAndBalanceWithDecimalPrecision()
    {
        using var context = CreateInMemoryContext();
        var (expenseCategoryA, _, incomeCategory, account) = await SeedReferenceDataAsync(context);
        var service = new ReportService(context);

        context.Transactions.AddRange(
            new Transaction
            {
                Date = new DateOnly(2026, 2, 1),
                Amount = 1000.55m,
                Type = TransactionType.Income,
                CategoryId = incomeCategory.Id,
                AccountId = account.Id
            },
            new Transaction
            {
                Date = new DateOnly(2026, 2, 2),
                Amount = 100.10m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategoryA.Id,
                AccountId = account.Id
            },
            new Transaction
            {
                Date = new DateOnly(2026, 2, 3),
                Amount = 50.25m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategoryA.Id,
                AccountId = account.Id
            },
            new Transaction
            {
                Date = new DateOnly(2026, 1, 31),
                Amount = 999m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategoryA.Id,
                AccountId = account.Id
            });
        await context.SaveChangesAsync();

        var summary = await service.GetMonthlySummaryAsync(2026, 2);

        Assert.Equal(1000.55m, summary.TotalIncome);
        Assert.Equal(150.35m, summary.TotalExpense);
        Assert.Equal(850.20m, summary.Balance);
        Assert.True(summary.HasData);
    }

    [Fact]
    public async Task GetCategoryBreakdownAsync_ShouldReturnPercentagesSummedToOneHundred()
    {
        using var context = CreateInMemoryContext();
        var (expenseCategoryA, expenseCategoryB, incomeCategory, account) = await SeedReferenceDataAsync(context);
        var service = new ReportService(context);

        context.Transactions.AddRange(
            new Transaction
            {
                Date = new DateOnly(2026, 2, 5),
                Amount = 60m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategoryA.Id,
                AccountId = account.Id
            },
            new Transaction
            {
                Date = new DateOnly(2026, 2, 6),
                Amount = 40m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategoryB.Id,
                AccountId = account.Id
            },
            new Transaction
            {
                Date = new DateOnly(2026, 2, 7),
                Amount = 500m,
                Type = TransactionType.Income,
                CategoryId = incomeCategory.Id,
                AccountId = account.Id
            });
        await context.SaveChangesAsync();

        var breakdown = await service.GetCategoryBreakdownAsync(2026, 2);
        var percentageSum = breakdown.Sum(item => item.Percentage);

        Assert.Equal(2, breakdown.Count);
        Assert.Equal(100m, percentageSum);
        Assert.Contains(breakdown, item => item.Name == expenseCategoryA.Name && item.Amount == 60m);
        Assert.Contains(breakdown, item => item.Name == expenseCategoryB.Name && item.Amount == 40m);
    }

    [Fact]
    public async Task GetDailyTrendsAsync_ShouldAggregateIncomeAndExpenseByDate()
    {
        using var context = CreateInMemoryContext();
        var (expenseCategoryA, _, incomeCategory, account) = await SeedReferenceDataAsync(context);
        var service = new ReportService(context);

        context.Transactions.AddRange(
            new Transaction
            {
                Date = new DateOnly(2026, 2, 1),
                Amount = 200m,
                Type = TransactionType.Income,
                CategoryId = incomeCategory.Id,
                AccountId = account.Id
            },
            new Transaction
            {
                Date = new DateOnly(2026, 2, 1),
                Amount = 50m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategoryA.Id,
                AccountId = account.Id
            },
            new Transaction
            {
                Date = new DateOnly(2026, 2, 1),
                Amount = 30m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategoryA.Id,
                AccountId = account.Id
            },
            new Transaction
            {
                Date = new DateOnly(2026, 2, 3),
                Amount = 99.9m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategoryA.Id,
                AccountId = account.Id
            });
        await context.SaveChangesAsync();

        var trends = await service.GetDailyTrendsAsync(2026, 2);

        Assert.Equal(2, trends.Count);
        Assert.Equal(new DateOnly(2026, 2, 1), trends[0].Date);
        Assert.Equal(200m, trends[0].Income);
        Assert.Equal(80m, trends[0].Expense);
        Assert.Equal(new DateOnly(2026, 2, 3), trends[1].Date);
        Assert.Equal(0m, trends[1].Income);
        Assert.Equal(99.9m, trends[1].Expense);
    }

    [Fact]
    public async Task EmptyMonth_ShouldReturnZeroSummaryAndNoChartData()
    {
        using var context = CreateInMemoryContext();
        var (expenseCategoryA, _, _, account) = await SeedReferenceDataAsync(context);
        var service = new ReportService(context);

        context.Transactions.Add(new Transaction
        {
            Date = new DateOnly(2026, 2, 1),
            Amount = 10m,
            Type = TransactionType.Expense,
            CategoryId = expenseCategoryA.Id,
            AccountId = account.Id
        });
        await context.SaveChangesAsync();

        var summary = await service.GetMonthlySummaryAsync(2026, 3);
        var categoryBreakdown = await service.GetCategoryBreakdownAsync(2026, 3);
        var dailyTrends = await service.GetDailyTrendsAsync(2026, 3);

        Assert.Equal(0m, summary.TotalIncome);
        Assert.Equal(0m, summary.TotalExpense);
        Assert.Equal(0m, summary.Balance);
        Assert.False(summary.HasData);
        Assert.Empty(categoryBreakdown);
        Assert.Empty(dailyTrends);
    }

    private static BookKeepingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BookKeepingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BookKeepingDbContext(options);
    }

    private static async Task<(Category ExpenseCategoryA, Category ExpenseCategoryB, Category IncomeCategory, Account Account)> SeedReferenceDataAsync(BookKeepingDbContext context)
    {
        var expenseCategoryA = new Category
        {
            Name = "Food",
            Icon = "🍽️",
            Type = TransactionType.Expense,
            Color = "#FF6384"
        };

        var expenseCategoryB = new Category
        {
            Name = "Travel",
            Icon = "🚗",
            Type = TransactionType.Expense,
            Color = "#36A2EB"
        };

        var incomeCategory = new Category
        {
            Name = "Salary",
            Icon = "💰",
            Type = TransactionType.Income,
            Color = "#4CAF50"
        };

        var account = new Account
        {
            Name = "Cash",
            Type = AccountType.Cash,
            Icon = "💵",
            InitialBalance = 0m,
            Currency = "TWD"
        };

        context.Categories.AddRange(expenseCategoryA, expenseCategoryB, incomeCategory);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        return (expenseCategoryA, expenseCategoryB, incomeCategory, account);
    }
}
