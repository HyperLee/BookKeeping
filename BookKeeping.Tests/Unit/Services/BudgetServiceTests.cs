using BookKeeping.Data;
using BookKeeping.Models;
using BookKeeping.Services;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Tests.Unit.Services;

public class BudgetServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateBudgetForExpenseCategory()
    {
        using var context = CreateInMemoryContext();
        var expenseCategory = await SeedCategoryAsync(context, TransactionType.Expense, "Food");
        var service = new BudgetService(context);

        var createdBudget = await service.CreateAsync(new Budget
        {
            CategoryId = expenseCategory.Id,
            Amount = 5000.25m,
            Period = BudgetPeriod.Monthly,
            StartDate = new DateOnly(2026, 2, 1)
        });

        Assert.True(createdBudget.Id > 0);
        Assert.Equal(1, await context.Budgets.CountAsync());
        Assert.Equal(5000.25m, createdBudget.Amount);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateBudgetFields()
    {
        using var context = CreateInMemoryContext();
        var expenseCategory = await SeedCategoryAsync(context, TransactionType.Expense, "Food");
        var budget = new Budget
        {
            CategoryId = expenseCategory.Id,
            Amount = 3000m,
            Period = BudgetPeriod.Monthly,
            StartDate = new DateOnly(2026, 2, 1)
        };
        context.Budgets.Add(budget);
        await context.SaveChangesAsync();

        var service = new BudgetService(context);
        var updatedBudget = await service.UpdateAsync(new Budget
        {
            Id = budget.Id,
            CategoryId = expenseCategory.Id,
            Amount = 3500.75m,
            Period = BudgetPeriod.Weekly,
            StartDate = new DateOnly(2026, 2, 3)
        });

        Assert.NotNull(updatedBudget);
        Assert.Equal(3500.75m, updatedBudget!.Amount);
        Assert.Equal(BudgetPeriod.Weekly, updatedBudget.Period);
        Assert.Equal(new DateOnly(2026, 2, 3), updatedBudget.StartDate);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteBudget()
    {
        using var context = CreateInMemoryContext();
        var expenseCategory = await SeedCategoryAsync(context, TransactionType.Expense, "Food");
        var budget = new Budget
        {
            CategoryId = expenseCategory.Id,
            Amount = 2500m,
            Period = BudgetPeriod.Monthly,
            StartDate = new DateOnly(2026, 2, 1)
        };
        context.Budgets.Add(budget);
        await context.SaveChangesAsync();

        var service = new BudgetService(context);
        var deleted = await service.DeleteAsync(budget.Id);

        Assert.True(deleted);
        var persistedBudget = await context.Budgets
            .IgnoreQueryFilters()
            .SingleAsync(item => item.Id == budget.Id);
        Assert.True(persistedBudget.IsDeleted);
        Assert.NotNull(persistedBudget.DeletedAt);
    }

    [Fact]
    public async Task GetAllWithProgressAsync_ShouldCalculateUsageRateWithDecimalPrecision()
    {
        using var context = CreateInMemoryContext();
        var expenseCategory = await SeedCategoryAsync(context, TransactionType.Expense, "Food");
        var account = await SeedAccountAsync(context);

        context.Budgets.Add(new Budget
        {
            CategoryId = expenseCategory.Id,
            Amount = 400m,
            Period = BudgetPeriod.Monthly,
            StartDate = new DateOnly(2026, 2, 1)
        });
        await context.SaveChangesAsync();

        context.Transactions.AddRange(
            new Transaction
            {
                Date = new DateOnly(2026, 2, 5),
                Amount = 70.10m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = account.Id
            },
            new Transaction
            {
                Date = new DateOnly(2026, 2, 6),
                Amount = 30.15m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = account.Id
            });
        await context.SaveChangesAsync();

        var service = new BudgetService(context);
        var budgetProgress = await service.GetAllWithProgressAsync(new DateOnly(2026, 2, 20));

        var progress = Assert.Single(budgetProgress);
        Assert.Equal(100.25m, progress.SpentAmount);
        Assert.Equal(25.0625m, progress.UsageRate);
        Assert.Equal("normal", progress.Status);
    }

    [Fact]
    public async Task CheckBudgetStatusAsync_ShouldReturnExpectedStatusThresholds()
    {
        using var context = CreateInMemoryContext();
        var expenseCategory = await SeedCategoryAsync(context, TransactionType.Expense, "Food");
        var account = await SeedAccountAsync(context);
        context.Budgets.Add(new Budget
        {
            CategoryId = expenseCategory.Id,
            Amount = 100m,
            Period = BudgetPeriod.Monthly,
            StartDate = new DateOnly(2026, 2, 1)
        });
        await context.SaveChangesAsync();

        var service = new BudgetService(context);
        context.Transactions.Add(new Transaction
        {
            Date = new DateOnly(2026, 2, 5),
            Amount = 70m,
            Type = TransactionType.Expense,
            CategoryId = expenseCategory.Id,
            AccountId = account.Id
        });
        await context.SaveChangesAsync();

        var normalStatus = await service.CheckBudgetStatusAsync(expenseCategory.Id, new DateOnly(2026, 2, 5));
        Assert.NotNull(normalStatus);
        Assert.Equal("normal", normalStatus!.Status);

        context.Transactions.Add(new Transaction
        {
            Date = new DateOnly(2026, 2, 6),
            Amount = 20m,
            Type = TransactionType.Expense,
            CategoryId = expenseCategory.Id,
            AccountId = account.Id
        });
        await context.SaveChangesAsync();

        var warningStatus = await service.CheckBudgetStatusAsync(expenseCategory.Id, new DateOnly(2026, 2, 6));
        Assert.NotNull(warningStatus);
        Assert.Equal("warning", warningStatus!.Status);

        context.Transactions.Add(new Transaction
        {
            Date = new DateOnly(2026, 2, 7),
            Amount = 20m,
            Type = TransactionType.Expense,
            CategoryId = expenseCategory.Id,
            AccountId = account.Id
        });
        await context.SaveChangesAsync();

        var exceededStatus = await service.CheckBudgetStatusAsync(expenseCategory.Id, new DateOnly(2026, 2, 7));
        Assert.NotNull(exceededStatus);
        Assert.Equal("exceeded", exceededStatus!.Status);
    }

    [Fact]
    public async Task GetAllWithProgressAsync_ShouldResetSpentAmountInNewMonth()
    {
        using var context = CreateInMemoryContext();
        var expenseCategory = await SeedCategoryAsync(context, TransactionType.Expense, "Food");
        var account = await SeedAccountAsync(context);
        context.Budgets.Add(new Budget
        {
            CategoryId = expenseCategory.Id,
            Amount = 100m,
            Period = BudgetPeriod.Monthly,
            StartDate = new DateOnly(2026, 1, 1)
        });
        await context.SaveChangesAsync();

        context.Transactions.AddRange(
            new Transaction
            {
                Date = new DateOnly(2026, 1, 10),
                Amount = 80m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = account.Id
            },
            new Transaction
            {
                Date = new DateOnly(2026, 2, 2),
                Amount = 30m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = account.Id
            });
        await context.SaveChangesAsync();

        var service = new BudgetService(context);
        var januaryProgress = Assert.Single(await service.GetAllWithProgressAsync(new DateOnly(2026, 1, 31)));
        var februaryProgress = Assert.Single(await service.GetAllWithProgressAsync(new DateOnly(2026, 2, 2)));

        Assert.Equal(80m, januaryProgress.SpentAmount);
        Assert.Equal(80m, januaryProgress.UsageRate);
        Assert.Equal(30m, februaryProgress.SpentAmount);
        Assert.Equal(30m, februaryProgress.UsageRate);
    }

    private static BookKeepingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BookKeepingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BookKeepingDbContext(options);
    }

    private static async Task<Category> SeedCategoryAsync(BookKeepingDbContext context, TransactionType type, string name)
    {
        var category = new Category
        {
            Name = $"{name}-{Guid.NewGuid():N}",
            Icon = "🍽️",
            Type = type,
            Color = "#FF6384"
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category;
    }

    private static async Task<Account> SeedAccountAsync(BookKeepingDbContext context)
    {
        var account = new Account
        {
            Name = $"Cash-{Guid.NewGuid():N}",
            Type = AccountType.Cash,
            Icon = "💵",
            InitialBalance = 0m,
            Currency = "TWD"
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        return account;
    }
}
