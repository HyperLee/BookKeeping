using BookKeeping.Data;
using BookKeeping.Models;
using BookKeeping.Services;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Tests.Unit.Services;

public class TransactionServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldPersistTransactionAndSetAuditTimestamps()
    {
        using var context = CreateInMemoryContext();
        var (expenseCategory, _, account) = await SeedReferenceDataAsync(context);
        var service = new TransactionService(context);

        var transaction = new Transaction
        {
            Date = new DateOnly(2026, 2, 20),
            Amount = 123.45m,
            Type = TransactionType.Expense,
            CategoryId = expenseCategory.Id,
            AccountId = account.Id,
            Note = "Lunch"
        };

        var created = await service.CreateAsync(transaction);

        Assert.True(created.Id > 0);
        Assert.NotEqual(default, created.CreatedAt);
        Assert.NotEqual(default, created.UpdatedAt);
        Assert.Equal(created.CreatedAt, created.UpdatedAt);

        var persisted = await context.Transactions.SingleAsync();
        Assert.Equal(created.Id, persisted.Id);
        Assert.Equal(123.45m, persisted.Amount);
    }

    [Fact]
    public async Task UpdateAsync_ShouldChangeAmountAndRefreshUpdatedAt()
    {
        using var context = CreateInMemoryContext();
        var (expenseCategory, _, account) = await SeedReferenceDataAsync(context);
        var service = new TransactionService(context);
        var transaction = new Transaction
        {
            Date = new DateOnly(2026, 2, 20),
            Amount = 50.25m,
            Type = TransactionType.Expense,
            CategoryId = expenseCategory.Id,
            AccountId = account.Id,
            Note = "Original"
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var originalUpdatedAt = transaction.UpdatedAt;
        await Task.Delay(20);

        transaction.Amount = 88.88m;
        var updated = await service.UpdateAsync(transaction);

        Assert.Equal(88.88m, updated.Amount);
        Assert.True(updated.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldMarkTransactionAsDeleted()
    {
        using var context = CreateInMemoryContext();
        var (expenseCategory, _, account) = await SeedReferenceDataAsync(context);
        var service = new TransactionService(context);
        var transaction = new Transaction
        {
            Date = new DateOnly(2026, 2, 20),
            Amount = 66.66m,
            Type = TransactionType.Expense,
            CategoryId = expenseCategory.Id,
            AccountId = account.Id,
            Note = "To delete"
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var success = await service.SoftDeleteAsync(transaction.Id);

        Assert.True(success);

        var deleted = await context.Transactions
            .IgnoreQueryFilters()
            .SingleAsync(t => t.Id == transaction.Id);

        Assert.True(deleted.IsDeleted);
        Assert.NotNull(deleted.DeletedAt);
        Assert.Null(await context.Transactions.SingleOrDefaultAsync(t => t.Id == transaction.Id));
    }

    [Fact]
    public async Task GetPagedAsync_ShouldPaginateAndSortByDateDescendingAndExcludeSoftDeleted()
    {
        using var context = CreateInMemoryContext();
        var (expenseCategory, incomeCategory, account) = await SeedReferenceDataAsync(context);
        var service = new TransactionService(context);

        context.Transactions.AddRange(
            new Transaction
            {
                Date = new DateOnly(2026, 1, 1),
                Amount = 10m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = account.Id,
                Note = "First"
            },
            new Transaction
            {
                Date = new DateOnly(2026, 1, 3),
                Amount = 20m,
                Type = TransactionType.Income,
                CategoryId = incomeCategory.Id,
                AccountId = account.Id,
                Note = "Second"
            },
            new Transaction
            {
                Date = new DateOnly(2026, 1, 2),
                Amount = 30m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = account.Id,
                Note = "Third"
            },
            new Transaction
            {
                Date = new DateOnly(2026, 1, 4),
                Amount = 40m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = account.Id,
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow,
                Note = "Deleted"
            });

        await context.SaveChangesAsync();

        var (firstPage, totalCount) = await service.GetPagedAsync(page: 1, pageSize: 2);
        var (secondPage, secondTotalCount) = await service.GetPagedAsync(page: 2, pageSize: 2);

        Assert.Equal(3, totalCount);
        Assert.Equal(3, secondTotalCount);
        Assert.Equal(2, firstPage.Count);
        Assert.Single(secondPage);

        Assert.Equal(new DateOnly(2026, 1, 3), firstPage[0].Date);
        Assert.Equal(new DateOnly(2026, 1, 2), firstPage[1].Date);
        Assert.Equal(new DateOnly(2026, 1, 1), secondPage[0].Date);
        Assert.DoesNotContain(firstPage, t => t.IsDeleted);
        Assert.DoesNotContain(secondPage, t => t.IsDeleted);
    }

    private static BookKeepingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BookKeepingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BookKeepingDbContext(options);
    }

    private static async Task<(Category ExpenseCategory, Category IncomeCategory, Account Account)> SeedReferenceDataAsync(BookKeepingDbContext context)
    {
        var expenseCategory = new Category
        {
            Name = "Expense",
            Icon = "🧾",
            Type = TransactionType.Expense,
            Color = "#FF6384"
        };

        var incomeCategory = new Category
        {
            Name = "Income",
            Icon = "💰",
            Type = TransactionType.Income,
            Color = "#4CAF50"
        };

        var account = new Account
        {
            Name = "Cash",
            Type = AccountType.Cash,
            Icon = "💵",
            InitialBalance = 100m,
            Currency = "TWD"
        };

        context.Categories.AddRange(expenseCategory, incomeCategory);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        return (expenseCategory, incomeCategory, account);
    }
}
