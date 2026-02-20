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

    [Fact]
    public async Task GetPagedAsync_WithNoFilters_ShouldReturnAllNonDeletedTransactions()
    {
        using var context = CreateInMemoryContext();
        var (expenseCategory, incomeCategory, account) = await SeedReferenceDataAsync(context);
        var service = new TransactionService(context);

        context.Transactions.AddRange(
            new Transaction
            {
                Date = new DateOnly(2026, 1, 1),
                Amount = 50m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = account.Id,
                Note = "Groceries"
            },
            new Transaction
            {
                Date = new DateOnly(2026, 1, 2),
                Amount = 100m,
                Type = TransactionType.Income,
                CategoryId = incomeCategory.Id,
                AccountId = account.Id,
                Note = "Salary"
            },
            new Transaction
            {
                Date = new DateOnly(2026, 1, 3),
                Amount = 20m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = account.Id,
                Note = "Coffee"
            },
            new Transaction
            {
                Date = new DateOnly(2026, 1, 4),
                Amount = 200m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = account.Id,
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow,
                Note = "Deleted"
            });
        await context.SaveChangesAsync();

        var (transactions, totalCount) = await service.GetPagedAsync(page: 1, pageSize: 20);

        Assert.Equal(3, totalCount);
        Assert.Equal(3, transactions.Count);
        Assert.DoesNotContain(transactions, t => t.IsDeleted);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldApplySingleFilters()
    {
        using var context = CreateInMemoryContext();
        var (expenseCategory, incomeCategory, cashAccount) = await SeedReferenceDataAsync(context);
        var bankAccount = new Account
        {
            Name = "Bank",
            Type = AccountType.Bank,
            Icon = "🏦",
            InitialBalance = 500m,
            Currency = "TWD"
        };
        context.Accounts.Add(bankAccount);
        await context.SaveChangesAsync();

        context.Transactions.AddRange(
            new Transaction
            {
                Date = new DateOnly(2026, 1, 1),
                Amount = 50m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = cashAccount.Id,
                Note = "Groceries"
            },
            new Transaction
            {
                Date = new DateOnly(2026, 1, 10),
                Amount = 120m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = bankAccount.Id,
                Note = "Monthly rent"
            },
            new Transaction
            {
                Date = new DateOnly(2026, 1, 20),
                Amount = 180m,
                Type = TransactionType.Income,
                CategoryId = incomeCategory.Id,
                AccountId = cashAccount.Id,
                Note = "Salary bonus"
            },
            new Transaction
            {
                Date = new DateOnly(2026, 2, 1),
                Amount = 300m,
                Type = TransactionType.Income,
                CategoryId = incomeCategory.Id,
                AccountId = bankAccount.Id,
                Note = "Gift"
            });
        await context.SaveChangesAsync();

        var service = new TransactionService(context);

        var (dateFiltered, dateFilteredCount) = await service.GetPagedAsync(
            startDate: new DateOnly(2026, 1, 5),
            endDate: new DateOnly(2026, 1, 31));
        var (categoryFiltered, categoryFilteredCount) = await service.GetPagedAsync(categoryId: expenseCategory.Id);
        var (accountFiltered, accountFilteredCount) = await service.GetPagedAsync(accountId: bankAccount.Id);
        var (amountFiltered, amountFilteredCount) = await service.GetPagedAsync(minAmount: 100m, maxAmount: 200m);
        var (keywordFiltered, keywordFilteredCount) = await service.GetPagedAsync(keyword: "salary");

        Assert.Equal(2, dateFilteredCount);
        Assert.All(dateFiltered, t => Assert.InRange(t.Date, new DateOnly(2026, 1, 5), new DateOnly(2026, 1, 31)));

        Assert.Equal(2, categoryFilteredCount);
        Assert.All(categoryFiltered, t => Assert.Equal(expenseCategory.Id, t.CategoryId));

        Assert.Equal(2, accountFilteredCount);
        Assert.All(accountFiltered, t => Assert.Equal(bankAccount.Id, t.AccountId));

        Assert.Equal(2, amountFilteredCount);
        Assert.All(amountFiltered, t => Assert.InRange(t.Amount, 100m, 200m));

        Assert.Single(keywordFiltered);
        Assert.Equal(1, keywordFilteredCount);
        Assert.Equal("Salary bonus", keywordFiltered[0].Note);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldApplyCombinedFilters()
    {
        using var context = CreateInMemoryContext();
        var (expenseCategory, incomeCategory, cashAccount) = await SeedReferenceDataAsync(context);
        var bankAccount = new Account
        {
            Name = "Bank",
            Type = AccountType.Bank,
            Icon = "🏦",
            InitialBalance = 1000m,
            Currency = "TWD"
        };
        context.Accounts.Add(bankAccount);
        await context.SaveChangesAsync();

        context.Transactions.AddRange(
            new Transaction
            {
                Date = new DateOnly(2026, 3, 10),
                Amount = 150m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = bankAccount.Id,
                Note = "Office supplies"
            },
            new Transaction
            {
                Date = new DateOnly(2026, 3, 12),
                Amount = 250m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = bankAccount.Id,
                Note = "Office rent"
            },
            new Transaction
            {
                Date = new DateOnly(2026, 3, 15),
                Amount = 140m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = cashAccount.Id,
                Note = "Office supplies"
            },
            new Transaction
            {
                Date = new DateOnly(2026, 3, 18),
                Amount = 130m,
                Type = TransactionType.Income,
                CategoryId = incomeCategory.Id,
                AccountId = bankAccount.Id,
                Note = "Office reimbursement"
            });
        await context.SaveChangesAsync();

        var service = new TransactionService(context);
        var (transactions, totalCount) = await service.GetPagedAsync(
            startDate: new DateOnly(2026, 3, 1),
            endDate: new DateOnly(2026, 3, 31),
            categoryId: expenseCategory.Id,
            accountId: bankAccount.Id,
            minAmount: 100m,
            maxAmount: 200m,
            keyword: "office");

        Assert.Single(transactions);
        Assert.Equal(1, totalCount);
        Assert.Equal("Office supplies", transactions[0].Note);
    }

    [Fact]
    public async Task GetPagedAsync_KeywordFilter_ShouldMatchNoteCaseInsensitive()
    {
        using var context = CreateInMemoryContext();
        var (expenseCategory, _, account) = await SeedReferenceDataAsync(context);

        context.Transactions.AddRange(
            new Transaction
            {
                Date = new DateOnly(2026, 4, 1),
                Amount = 88m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = account.Id,
                Note = "Monthly RENT payment"
            },
            new Transaction
            {
                Date = new DateOnly(2026, 4, 2),
                Amount = 66m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = account.Id,
                Note = "Groceries"
            });
        await context.SaveChangesAsync();

        var service = new TransactionService(context);
        var (transactions, totalCount) = await service.GetPagedAsync(keyword: "rEnT");

        Assert.Single(transactions);
        Assert.Equal(1, totalCount);
        Assert.Equal("Monthly RENT payment", transactions[0].Note);
    }

    [Fact]
    public async Task GetPagedAsync_WithFilters_ShouldReturnConsistentPagination()
    {
        using var context = CreateInMemoryContext();
        var (expenseCategory, _, account) = await SeedReferenceDataAsync(context);

        context.Transactions.AddRange(
            new Transaction
            {
                Date = new DateOnly(2026, 5, 1),
                Amount = 10m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = account.Id,
                Note = "Rent Jan"
            },
            new Transaction
            {
                Date = new DateOnly(2026, 5, 2),
                Amount = 20m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = account.Id,
                Note = "Rent Feb"
            },
            new Transaction
            {
                Date = new DateOnly(2026, 5, 3),
                Amount = 30m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = account.Id,
                Note = "Rent Mar"
            },
            new Transaction
            {
                Date = new DateOnly(2026, 5, 4),
                Amount = 40m,
                Type = TransactionType.Expense,
                CategoryId = expenseCategory.Id,
                AccountId = account.Id,
                Note = "Utilities"
            });
        await context.SaveChangesAsync();

        var service = new TransactionService(context);

        var (firstPage, firstTotalCount) = await service.GetPagedAsync(
            page: 1,
            pageSize: 1,
            categoryId: expenseCategory.Id,
            accountId: account.Id,
            keyword: "rent");
        var (secondPage, secondTotalCount) = await service.GetPagedAsync(
            page: 2,
            pageSize: 1,
            categoryId: expenseCategory.Id,
            accountId: account.Id,
            keyword: "rent");

        Assert.Equal(3, firstTotalCount);
        Assert.Equal(firstTotalCount, secondTotalCount);
        Assert.Single(firstPage);
        Assert.Single(secondPage);
        Assert.NotEqual(firstPage[0].Id, secondPage[0].Id);
        Assert.Contains("rent", firstPage[0].Note!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rent", secondPage[0].Note!, StringComparison.OrdinalIgnoreCase);
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
