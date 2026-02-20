using BookKeeping.Data;
using BookKeeping.Models;
using BookKeeping.Services;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Tests.Unit.Services;

public class AccountServiceTests
{
    [Fact]
    public async Task GetBalanceAsync_ShouldCalculateInitialBalancePlusIncomeMinusExpenseWithDecimalPrecision()
    {
        using var context = CreateInMemoryContext();
        var account = new Account
        {
            Name = "Primary",
            Type = AccountType.Bank,
            Icon = "🏦",
            InitialBalance = 1000.1234m,
            Currency = "TWD"
        };

        var otherAccount = new Account
        {
            Name = "Secondary",
            Type = AccountType.Cash,
            Icon = "💵",
            InitialBalance = 0m,
            Currency = "TWD"
        };

        var incomeCategory = new Category
        {
            Name = "Salary",
            Icon = "💰",
            Type = TransactionType.Income,
            Color = "#4CAF50"
        };

        var expenseCategory = new Category
        {
            Name = "Food",
            Icon = "🍜",
            Type = TransactionType.Expense,
            Color = "#FF6384"
        };

        context.Accounts.AddRange(account, otherAccount);
        context.Categories.AddRange(incomeCategory, expenseCategory);
        await context.SaveChangesAsync();

        context.Transactions.AddRange(
            new Transaction
            {
                Date = new DateOnly(2026, 2, 1),
                Amount = 250.8766m,
                Type = TransactionType.Income,
                CategoryId = incomeCategory.Id,
                AccountId = account.Id
            },
            new Transaction
            {
                Date = new DateOnly(2026, 2, 2),
                Amount = 49.5m,
                Type = TransactionType.Income,
                CategoryId = incomeCategory.Id,
                AccountId = account.Id
            },
            new Transaction
            {
                Date = new DateOnly(2026, 2, 3),
                Amount = 100.1111m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = account.Id
            },
            new Transaction
            {
                Date = new DateOnly(2026, 2, 4),
                Amount = 999m,
                Type = TransactionType.Income,
                CategoryId = incomeCategory.Id,
                AccountId = otherAccount.Id
            });

        await context.SaveChangesAsync();

        var service = new AccountService(context);
        var balance = await service.GetBalanceAsync(account.Id);

        Assert.Equal(1200.3889m, balance);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAccountsOrderedByIdAndExcludeSoftDeleted()
    {
        using var context = CreateInMemoryContext();

        var firstAccount = new Account
        {
            Name = "First",
            Type = AccountType.Cash,
            Icon = "💵",
            InitialBalance = 50m,
            Currency = "TWD"
        };

        var secondAccount = new Account
        {
            Name = "Second",
            Type = AccountType.Bank,
            Icon = "🏦",
            InitialBalance = 100m,
            Currency = "TWD"
        };

        var deletedAccount = new Account
        {
            Name = "Deleted",
            Type = AccountType.CreditCard,
            Icon = "💳",
            InitialBalance = 0m,
            Currency = "TWD",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        };

        context.Accounts.AddRange(firstAccount, secondAccount, deletedAccount);
        await context.SaveChangesAsync();

        var service = new AccountService(context);
        var accounts = await service.GetAllAsync();

        Assert.Equal(2, accounts.Count);
        Assert.Equal(firstAccount.Id, accounts[0].Id);
        Assert.Equal(secondAccount.Id, accounts[1].Id);
        Assert.DoesNotContain(accounts, account => account.IsDeleted);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAccountWhenNameIsUnique()
    {
        using var context = CreateInMemoryContext();
        var service = new AccountService(context);

        var created = await service.CreateAsync(new Account
        {
            Name = "Wallet",
            Type = AccountType.Cash,
            Icon = "👛",
            InitialBalance = 10m,
            Currency = "TWD"
        });

        Assert.True(created.Id > 0);
        Assert.Equal("Wallet", created.Name);
        Assert.Equal(1, await context.Accounts.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationExceptionWhenNameAlreadyExists()
    {
        using var context = CreateInMemoryContext();
        context.Accounts.Add(new Account
        {
            Name = "Duplicate",
            Type = AccountType.Bank,
            Icon = "🏦",
            InitialBalance = 0m,
            Currency = "TWD"
        });
        await context.SaveChangesAsync();

        var service = new AccountService(context);
        var duplicateAccount = new Account
        {
            Name = "Duplicate",
            Type = AccountType.CreditCard,
            Icon = "💳",
            InitialBalance = 0m,
            Currency = "TWD"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(duplicateAccount));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAccountFields()
    {
        using var context = CreateInMemoryContext();
        var account = new Account
        {
            Name = "Original",
            Type = AccountType.Cash,
            Icon = "💵",
            InitialBalance = 100m,
            Currency = "TWD"
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var service = new AccountService(context);
        account.Name = "Updated";
        account.Type = AccountType.CreditCard;
        account.Icon = "💳";
        account.InitialBalance = 250.5m;

        var updated = await service.UpdateAsync(account);

        Assert.NotNull(updated);
        Assert.Equal("Updated", updated!.Name);
        Assert.Equal(AccountType.CreditCard, updated.Type);
        Assert.Equal("💳", updated.Icon);
        Assert.Equal(250.5m, updated.InitialBalance);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalseWhenAccountHasTransactions()
    {
        using var context = CreateInMemoryContext();
        var account = new Account
        {
            Name = "InUse",
            Type = AccountType.Bank,
            Icon = "🏦",
            InitialBalance = 0m,
            Currency = "TWD"
        };
        context.Accounts.Add(account);
        var expenseCategory = await SeedExpenseCategoryAsync(context);
        await context.SaveChangesAsync();

        context.Transactions.Add(new Transaction
        {
            Date = new DateOnly(2026, 2, 10),
            Amount = 100m,
            Type = TransactionType.Expense,
            CategoryId = expenseCategory.Id,
            AccountId = account.Id
        });
        await context.SaveChangesAsync();

        var service = new AccountService(context);
        var deleted = await service.DeleteAsync(account.Id);

        Assert.False(deleted);
        var persisted = await context.Accounts
            .IgnoreQueryFilters()
            .SingleAsync(a => a.Id == account.Id);
        Assert.False(persisted.IsDeleted);
    }

    [Fact]
    public async Task HasTransactionsAsync_ShouldReturnExpectedResult()
    {
        using var context = CreateInMemoryContext();
        var usedAccount = new Account
        {
            Name = "Used",
            Type = AccountType.Cash,
            Icon = "💵",
            InitialBalance = 0m,
            Currency = "TWD"
        };
        var unusedAccount = new Account
        {
            Name = "Unused",
            Type = AccountType.Bank,
            Icon = "🏦",
            InitialBalance = 0m,
            Currency = "TWD"
        };
        context.Accounts.AddRange(usedAccount, unusedAccount);
        var expenseCategory = await SeedExpenseCategoryAsync(context);
        await context.SaveChangesAsync();

        context.Transactions.Add(new Transaction
        {
            Date = new DateOnly(2026, 2, 11),
            Amount = 80m,
            Type = TransactionType.Expense,
            CategoryId = expenseCategory.Id,
            AccountId = usedAccount.Id
        });
        await context.SaveChangesAsync();

        var service = new AccountService(context);

        Assert.True(await service.HasTransactionsAsync(usedAccount.Id));
        Assert.False(await service.HasTransactionsAsync(unusedAccount.Id));
    }

    private static BookKeepingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BookKeepingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BookKeepingDbContext(options);
    }

    private static async Task<Category> SeedExpenseCategoryAsync(BookKeepingDbContext context)
    {
        var category = new Category
        {
            Name = $"Category-{Guid.NewGuid():N}",
            Icon = "🍽️",
            Type = TransactionType.Expense,
            Color = "#FF6384"
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category;
    }
}
