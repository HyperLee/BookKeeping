using System.Text;
using BookKeeping.Data;
using BookKeeping.Models;
using BookKeeping.Services;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Tests.Unit.Services;

public class CsvServiceTests
{
    [Fact]
    public async Task ExportTransactionsAsync_ShouldIncludeUtf8BomAndHeaderRow()
    {
        using var context = CreateInMemoryContext();
        var (category, account) = await SeedReferenceDataAsync(context);
        var service = new CsvService(context);

        context.Transactions.Add(new Transaction
        {
            Date = new DateOnly(2026, 2, 15),
            Amount = 123.45m,
            Type = TransactionType.Expense,
            CategoryId = category.Id,
            AccountId = account.Id,
            Note = "一般備註"
        });
        await context.SaveChangesAsync();

        var csvBytes = await service.ExportTransactionsAsync();

        AssertUtf8Bom(csvBytes);
        var content = DecodeCsvContent(csvBytes);
        Assert.StartsWith("日期,類型,金額,分類,帳戶,備註\r\n", content);
    }

    [Fact]
    public async Task ExportTransactionsAsync_ShouldEscapeCommaQuoteAndNewlinePerRfc4180()
    {
        using var context = CreateInMemoryContext();
        var category = new Category
        {
            Name = "餐飲,\"聚會\"",
            Icon = "🍽️",
            Type = TransactionType.Expense,
            Color = "#FF6384"
        };
        var account = new Account
        {
            Name = "現金,主帳戶",
            Type = AccountType.Cash,
            Icon = "💵",
            InitialBalance = 0m,
            Currency = "TWD"
        };
        context.Categories.Add(category);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        context.Transactions.Add(new Transaction
        {
            Date = new DateOnly(2026, 2, 18),
            Amount = 99.95m,
            Type = TransactionType.Expense,
            CategoryId = category.Id,
            AccountId = account.Id,
            Note = "第一行,\"內容\"\r\n第二行"
        });
        await context.SaveChangesAsync();

        var service = new CsvService(context);
        var csvBytes = await service.ExportTransactionsAsync();
        var content = DecodeCsvContent(csvBytes);

        Assert.Contains("\"餐飲,\"\"聚會\"\"\"", content);
        Assert.Contains("\"現金,主帳戶\"", content);
        Assert.Contains("\"第一行,\"\"內容\"\"\r\n第二行\"", content);
    }

    [Fact]
    public async Task ExportTransactionsAsync_ShouldFilterByDateRange()
    {
        using var context = CreateInMemoryContext();
        var (category, account) = await SeedReferenceDataAsync(context);
        var service = new CsvService(context);

        context.Transactions.AddRange(
            new Transaction
            {
                Date = new DateOnly(2026, 1, 20),
                Amount = 10m,
                Type = TransactionType.Expense,
                CategoryId = category.Id,
                AccountId = account.Id,
                Note = "Jan"
            },
            new Transaction
            {
                Date = new DateOnly(2026, 2, 20),
                Amount = 20m,
                Type = TransactionType.Income,
                CategoryId = category.Id,
                AccountId = account.Id,
                Note = "Feb"
            });
        await context.SaveChangesAsync();

        var csvBytes = await service.ExportTransactionsAsync(
            startDate: new DateOnly(2026, 2, 1),
            endDate: new DateOnly(2026, 2, 28));
        var content = DecodeCsvContent(csvBytes);

        Assert.DoesNotContain("2026-01-20", content);
        Assert.Contains("2026-02-20,收入,20", content);
    }

    [Fact]
    public async Task ExportTransactionsAsync_WithNoTransactions_ShouldReturnHeaderOnlyFile()
    {
        using var context = CreateInMemoryContext();
        var service = new CsvService(context);

        var csvBytes = await service.ExportTransactionsAsync();

        AssertUtf8Bom(csvBytes);
        var content = DecodeCsvContent(csvBytes);
        Assert.Equal("日期,類型,金額,分類,帳戶,備註\r\n", content);
    }

    private static BookKeepingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BookKeepingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BookKeepingDbContext(options);
    }

    private static async Task<(Category Category, Account Account)> SeedReferenceDataAsync(BookKeepingDbContext context)
    {
        var category = new Category
        {
            Name = "餐飲",
            Icon = "🍽️",
            Type = TransactionType.Expense,
            Color = "#FF6384"
        };

        var account = new Account
        {
            Name = "現金",
            Type = AccountType.Cash,
            Icon = "💵",
            InitialBalance = 0m,
            Currency = "TWD"
        };

        context.Categories.Add(category);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        return (category, account);
    }

    private static void AssertUtf8Bom(byte[] bytes)
    {
        Assert.True(bytes.Length >= 3);
        Assert.Equal(0xEF, bytes[0]);
        Assert.Equal(0xBB, bytes[1]);
        Assert.Equal(0xBF, bytes[2]);
    }

    private static string DecodeCsvContent(byte[] bytes)
    {
        return bytes.Length <= 3
            ? string.Empty
            : Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
    }
}
