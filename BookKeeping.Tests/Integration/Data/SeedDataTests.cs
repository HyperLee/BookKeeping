using Microsoft.EntityFrameworkCore;
using BookKeeping.Data;
using BookKeeping.Data.Seed;
using BookKeeping.Models;
using Xunit;

namespace BookKeeping.Tests.Integration.Data;

public class SeedDataTests
{
    private BookKeepingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BookKeepingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new BookKeepingDbContext(options);
    }

    [Fact]
    public async Task SeedAsync_ShouldSeed8ExpenseCategories()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var seeder = new DefaultDataSeeder(context);

        // Act
        await seeder.SeedAsync();

        // Assert
        var expenseCategories = await context.Categories
            .Where(c => c.Type == TransactionType.Expense && !c.IsDeleted)
            .ToListAsync();
        
        Assert.Equal(8, expenseCategories.Count);
        
        // Verify specific categories
        var categoryNames = expenseCategories.Select(c => c.Name).ToList();
        Assert.Contains("餐飲", categoryNames);
        Assert.Contains("交通", categoryNames);
        Assert.Contains("娛樂", categoryNames);
        Assert.Contains("購物", categoryNames);
        Assert.Contains("居住", categoryNames);
        Assert.Contains("醫療", categoryNames);
        Assert.Contains("教育", categoryNames);
        Assert.Contains("其他", categoryNames);
        
        // Verify all have icons
        Assert.All(expenseCategories, c => Assert.False(string.IsNullOrEmpty(c.Icon)));
        
        // Verify all are marked as default
        Assert.All(expenseCategories, c => Assert.True(c.IsDefault));
    }

    [Fact]
    public async Task SeedAsync_ShouldSeed4IncomeCategories()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var seeder = new DefaultDataSeeder(context);

        // Act
        await seeder.SeedAsync();

        // Assert
        var incomeCategories = await context.Categories
            .Where(c => c.Type == TransactionType.Income && !c.IsDeleted)
            .ToListAsync();
        
        Assert.Equal(4, incomeCategories.Count);
        
        // Verify specific categories
        var categoryNames = incomeCategories.Select(c => c.Name).ToList();
        Assert.Contains("薪資", categoryNames);
        Assert.Contains("獎金", categoryNames);
        Assert.Contains("投資收益", categoryNames);
        Assert.Contains("其他收入", categoryNames);
        
        // Verify all have icons
        Assert.All(incomeCategories, c => Assert.False(string.IsNullOrEmpty(c.Icon)));
        
        // Verify all are marked as default
        Assert.All(incomeCategories, c => Assert.True(c.IsDefault));
    }

    [Fact]
    public async Task SeedAsync_ShouldSeed3DefaultAccounts()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var seeder = new DefaultDataSeeder(context);

        // Act
        await seeder.SeedAsync();

        // Assert
        var accounts = await context.Accounts
            .Where(a => !a.IsDeleted)
            .ToListAsync();
        
        Assert.Equal(3, accounts.Count);
        
        // Verify specific accounts
        var accountNames = accounts.Select(a => a.Name).ToList();
        Assert.Contains("現金", accountNames);
        Assert.Contains("銀行帳戶", accountNames);
        Assert.Contains("信用卡", accountNames);
        
        // Verify account types
        var cash = accounts.First(a => a.Name == "現金");
        Assert.Equal(AccountType.Cash, cash.Type);
        Assert.Equal("💵", cash.Icon);
        
        var bank = accounts.First(a => a.Name == "銀行帳戶");
        Assert.Equal(AccountType.Bank, bank.Type);
        Assert.Equal("🏦", bank.Icon);
        
        var creditCard = accounts.First(a => a.Name == "信用卡");
        Assert.Equal(AccountType.CreditCard, creditCard.Type);
        Assert.Equal("💳", creditCard.Icon);
        
        // Verify all have zero initial balance and TWD currency
        Assert.All(accounts, a => Assert.Equal(0, a.InitialBalance));
        Assert.All(accounts, a => Assert.Equal("TWD", a.Currency));
    }

    [Fact]
    public async Task SeedAsync_ShouldBeIdempotent_WhenCalledMultipleTimes()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var seeder = new DefaultDataSeeder(context);

        // Act - Seed twice
        await seeder.SeedAsync();
        await seeder.SeedAsync();

        // Assert - Still only have the original seeded data
        var categories = await context.Categories.CountAsync();
        var accounts = await context.Accounts.CountAsync();
        
        Assert.Equal(12, categories); // 8 expense + 4 income
        Assert.Equal(3, accounts);
    }

    [Fact]
    public async Task SeedAsync_ShouldNotSeedCategories_WhenCategoriesAlreadyExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        // Add a custom category first
        context.Categories.Add(new Category
        {
            Name = "Custom Category",
            Icon = "📌",
            Type = TransactionType.Expense,
            Color = "#000000",
            IsDefault = false
        });
        await context.SaveChangesAsync();
        
        var seeder = new DefaultDataSeeder(context);

        // Act
        await seeder.SeedAsync();

        // Assert - Should only have the custom category, no default ones added
        var categories = await context.Categories.CountAsync();
        Assert.Equal(1, categories);
    }

    [Fact]
    public async Task SeedAsync_ShouldNotSeedAccounts_WhenAccountsAlreadyExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        // Add a custom account first
        context.Accounts.Add(new Account
        {
            Name = "Custom Account",
            Type = AccountType.Bank,
            Icon = "🏛️",
            InitialBalance = 1000,
            Currency = "TWD"
        });
        await context.SaveChangesAsync();
        
        var seeder = new DefaultDataSeeder(context);

        // Act
        await seeder.SeedAsync();

        // Assert - Should only have the custom account, no default ones added
        var accounts = await context.Accounts.CountAsync();
        Assert.Equal(1, accounts);
    }

    [Fact]
    public async Task SeedAsync_ShouldSetCorrectColors_ForCategories()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var seeder = new DefaultDataSeeder(context);

        // Act
        await seeder.SeedAsync();

        // Assert
        var categories = await context.Categories.ToListAsync();
        
        // All categories should have a color
        Assert.All(categories, c => Assert.False(string.IsNullOrEmpty(c.Color)));
        
        // Verify specific colors from data-model.md
        var diningCategory = categories.First(c => c.Name == "餐飲");
        Assert.Equal("#FF6384", diningCategory.Color);
        
        var salaryCategory = categories.First(c => c.Name == "薪資");
        Assert.Equal("#4CAF50", salaryCategory.Color);
    }

    [Fact]
    public async Task SeedAsync_ShouldSetCorrectSortOrder_ForCategories()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var seeder = new DefaultDataSeeder(context);

        // Act
        await seeder.SeedAsync();

        // Assert
        var expenseCategories = await context.Categories
            .Where(c => c.Type == TransactionType.Expense)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
        
        var incomeCategories = await context.Categories
            .Where(c => c.Type == TransactionType.Income)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
        
        // Verify sort orders are sequential starting from 1
        Assert.Equal(1, expenseCategories[0].SortOrder);
        Assert.Equal(2, expenseCategories[1].SortOrder);
        
        Assert.Equal(1, incomeCategories[0].SortOrder);
        Assert.Equal(2, incomeCategories[1].SortOrder);
    }
}
