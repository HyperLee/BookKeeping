using BookKeeping.Data;
using BookKeeping.Models;
using BookKeeping.Services;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Tests.Unit.Services;

public class CategoryServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldEnforceUniqueNamePerType()
    {
        using var context = CreateInMemoryContext();
        var service = new CategoryService(context);

        var expenseCategory = await service.CreateAsync(new Category
        {
            Name = "餐飲",
            Icon = "🍽️",
            Type = TransactionType.Expense,
            Color = "#FF6384"
        });

        var incomeCategory = await service.CreateAsync(new Category
        {
            Name = "餐飲",
            Icon = "💰",
            Type = TransactionType.Income,
            Color = "#4CAF50"
        });

        var duplicateExpense = new Category
        {
            Name = "餐飲",
            Icon = "🍽️",
            Type = TransactionType.Expense,
            Color = "#FF6384"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(duplicateExpense));
        Assert.True(expenseCategory.Id > 0);
        Assert.True(incomeCategory.Id > 0);
        Assert.Equal(2, await context.Categories.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCategoryFields()
    {
        using var context = CreateInMemoryContext();
        var category = new Category
        {
            Name = "原始分類",
            Icon = "📌",
            Type = TransactionType.Expense,
            Color = "#111111",
            SortOrder = 1
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        category.Name = "更新分類";
        category.Icon = "🧾";
        category.Color = "#222222";

        var updated = await service.UpdateAsync(category);

        Assert.NotNull(updated);
        Assert.Equal("更新分類", updated!.Name);
        Assert.Equal("🧾", updated.Icon);
        Assert.Equal("#222222", updated.Color);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalseWhenCategoryHasTransactions()
    {
        using var context = CreateInMemoryContext();
        var (category, _, account) = await SeedTransactionReferencesAsync(context);
        context.Transactions.Add(new Transaction
        {
            Date = new DateOnly(2026, 2, 1),
            Amount = 100m,
            Type = TransactionType.Expense,
            CategoryId = category.Id,
            AccountId = account.Id
        });
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        var deleted = await service.DeleteAsync(category.Id);

        Assert.False(deleted);
        var persisted = await context.Categories
            .IgnoreQueryFilters()
            .SingleAsync(c => c.Id == category.Id);
        Assert.False(persisted.IsDeleted);
    }

    [Fact]
    public async Task DeleteAndMigrateAsync_ShouldMoveTransactionsToTargetCategory()
    {
        using var context = CreateInMemoryContext();
        var (sourceCategory, targetCategory, account) = await SeedTransactionReferencesAsync(context);
        context.Transactions.AddRange(
            new Transaction
            {
                Date = new DateOnly(2026, 2, 1),
                Amount = 100m,
                Type = TransactionType.Expense,
                CategoryId = sourceCategory.Id,
                AccountId = account.Id
            },
            new Transaction
            {
                Date = new DateOnly(2026, 2, 2),
                Amount = 50m,
                Type = TransactionType.Expense,
                CategoryId = sourceCategory.Id,
                AccountId = account.Id
            });
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        var success = await service.DeleteAndMigrateAsync(sourceCategory.Id, targetCategory.Id);

        Assert.True(success);
        var migratedCategoryIds = await context.Transactions
            .Select(t => t.CategoryId)
            .ToListAsync();
        Assert.All(migratedCategoryIds, id => Assert.Equal(targetCategory.Id, id));

        var deletedSource = await context.Categories
            .IgnoreQueryFilters()
            .SingleAsync(c => c.Id == sourceCategory.Id);
        Assert.True(deletedSource.IsDeleted);
        Assert.NotNull(deletedSource.DeletedAt);
    }

    [Fact]
    public async Task HasTransactionsAsync_ShouldReturnExpectedResult()
    {
        using var context = CreateInMemoryContext();
        var (usedCategory, unusedCategory, account) = await SeedTransactionReferencesAsync(context);
        context.Transactions.Add(new Transaction
        {
            Date = new DateOnly(2026, 2, 3),
            Amount = 80m,
            Type = TransactionType.Expense,
            CategoryId = usedCategory.Id,
            AccountId = account.Id
        });
        await context.SaveChangesAsync();

        var service = new CategoryService(context);

        Assert.True(await service.HasTransactionsAsync(usedCategory.Id));
        Assert.False(await service.HasTransactionsAsync(unusedCategory.Id));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalseForDefaultCategory()
    {
        using var context = CreateInMemoryContext();
        var defaultCategory = new Category
        {
            Name = "預設分類",
            Icon = "📎",
            Type = TransactionType.Expense,
            Color = "#7C8798",
            IsDefault = true
        };
        context.Categories.Add(defaultCategory);
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        var deleted = await service.DeleteAsync(defaultCategory.Id);

        Assert.False(deleted);
        var persisted = await context.Categories
            .IgnoreQueryFilters()
            .SingleAsync(c => c.Id == defaultCategory.Id);
        Assert.False(persisted.IsDeleted);
    }

    private static BookKeepingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BookKeepingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BookKeepingDbContext(options);
    }

    private static async Task<(Category SourceCategory, Category TargetCategory, Account Account)> SeedTransactionReferencesAsync(BookKeepingDbContext context)
    {
        var sourceCategory = new Category
        {
            Name = "來源分類",
            Icon = "🍽️",
            Type = TransactionType.Expense,
            Color = "#FF6384",
            SortOrder = 1
        };
        var targetCategory = new Category
        {
            Name = "目標分類",
            Icon = "🛒",
            Type = TransactionType.Expense,
            Color = "#4BC0C0",
            SortOrder = 2
        };
        var account = new Account
        {
            Name = "現金帳戶",
            Type = AccountType.Cash,
            Icon = "💵",
            InitialBalance = 0m,
            Currency = "TWD"
        };

        context.Categories.AddRange(sourceCategory, targetCategory);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        return (sourceCategory, targetCategory, account);
    }
}
