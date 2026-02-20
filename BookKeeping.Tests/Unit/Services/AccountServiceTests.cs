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

    private static BookKeepingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BookKeepingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BookKeepingDbContext(options);
    }
}
