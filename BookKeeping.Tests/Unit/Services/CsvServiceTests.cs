using System.Text;
using BookKeeping.Data;
using BookKeeping.Models;
using BookKeeping.Services;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Tests.Unit.Services;

public class CsvServiceTests
{
    [Fact]
    public async Task ExportTransactionsAsync_ShouldIncludeUtf8BomAndHeaderRow()
    {
        using var context = CreateInMemoryContext();
        var (category, account) = await SeedReferenceDataAsync(context);
        var service = CreateCsvService(context);

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

        var service = CreateCsvService(context);
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
        var service = CreateCsvService(context);

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
        var service = CreateCsvService(context);

        var csvBytes = await service.ExportTransactionsAsync();

        AssertUtf8Bom(csvBytes);
        var content = DecodeCsvContent(csvBytes);
        Assert.Equal("日期,類型,金額,分類,帳戶,備註\r\n", content);
    }

    [Fact]
    public async Task ImportTransactionsAsync_ShouldImportAllValidRows()
    {
        using var context = CreateInMemoryContext();
        await SeedReferenceDataAsync(context);
        var service = CreateCsvService(context);
        using var csvStream = CreateCsvStream(
            "日期,類型,金額,分類,帳戶,備註\r\n" +
            "2026-02-15,支出,120,餐飲,現金,午餐\r\n" +
            "2026-02-16,支出,80,餐飲,現金,晚餐");

        var result = await service.ImportTransactionsAsync(csvStream, csvStream.Length);

        Assert.Equal(2, result.TotalRows);
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.FailedCount);
        Assert.Empty(result.Errors);
        Assert.Equal(2, await context.Transactions.CountAsync());
    }

    [Fact]
    public async Task ImportTransactionsAsync_ShouldSkipInvalidDateRowWithErrorMessage()
    {
        using var context = CreateInMemoryContext();
        await SeedReferenceDataAsync(context);
        var service = CreateCsvService(context);
        using var csvStream = CreateCsvStream(
            "日期,類型,金額,分類,帳戶,備註\r\n" +
            "2026/13/01,支出,100,餐飲,現金,錯誤日期\r\n" +
            "2026-02-16,支出,80,餐飲,現金,有效資料");

        var result = await service.ImportTransactionsAsync(csvStream, csvStream.Length);

        Assert.Equal(2, result.TotalRows);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(1, result.FailedCount);
        var error = Assert.Single(result.Errors);
        Assert.Equal(2, error.LineNumber);
        Assert.Contains("日期格式無效", error.ErrorMessage);
        Assert.Equal(1, await context.Transactions.CountAsync());
    }

    [Fact]
    public async Task ImportTransactionsAsync_ShouldSkipAmountLessThanOrEqualToZeroRow()
    {
        using var context = CreateInMemoryContext();
        await SeedReferenceDataAsync(context);
        var service = CreateCsvService(context);
        using var csvStream = CreateCsvStream(
            "日期,類型,金額,分類,帳戶,備註\r\n" +
            "2026-02-15,支出,0,餐飲,現金,無效金額\r\n" +
            "2026-02-16,支出,80,餐飲,現金,有效資料");

        var result = await service.ImportTransactionsAsync(csvStream, csvStream.Length);

        Assert.Equal(2, result.TotalRows);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(1, result.FailedCount);
        var error = Assert.Single(result.Errors);
        Assert.Contains("金額必須大於 0", error.ErrorMessage);
        Assert.Equal(1, await context.Transactions.CountAsync());
    }

    [Fact]
    public async Task ImportTransactionsAsync_ShouldAutoCreateMissingCategory()
    {
        using var context = CreateInMemoryContext();
        await SeedReferenceDataAsync(context);
        var service = CreateCsvService(context);
        using var csvStream = CreateCsvStream(
            "日期,類型,金額,分類,帳戶,備註\r\n" +
            "2026-02-15,支出,120,旅遊,現金,行程");

        var result = await service.ImportTransactionsAsync(csvStream, csvStream.Length);

        Assert.Equal(1, result.SuccessCount);
        var createdCategory = await context.Categories
            .SingleOrDefaultAsync(c => c.Name == "旅遊" && c.Type == TransactionType.Expense);
        Assert.NotNull(createdCategory);
        Assert.Equal(1, await context.Transactions.CountAsync());
    }

    [Fact]
    public async Task ImportTransactionsAsync_ShouldSanitizeNoteCategoryAndAccountFields()
    {
        using var context = CreateInMemoryContext();
        await SeedReferenceDataAsync(context);
        var service = CreateCsvService(context);
        using var csvStream = CreateCsvStream(
            "日期,類型,金額,分類,帳戶,備註\r\n" +
            "2026-02-15,支出,120,<script>alert(1)</script>旅遊,<script>alert(2)</script>現金,<script>alert(3)</script>備註");

        var result = await service.ImportTransactionsAsync(csvStream, csvStream.Length);

        Assert.Equal(1, result.SuccessCount);
        Assert.Empty(result.Errors);
        var transaction = await context.Transactions
            .Include(t => t.Category)
            .SingleAsync();
        Assert.NotNull(transaction.Category);
        Assert.Equal("旅遊", transaction.Category.Name);
        Assert.DoesNotContain("<script>", transaction.Note ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ImportTransactionsAsync_ShouldEnforceFileSizeLimit()
    {
        using var context = CreateInMemoryContext();
        await SeedReferenceDataAsync(context);
        var service = CreateCsvService(context);
        using var csvStream = CreateCsvStream(
            "日期,類型,金額,分類,帳戶,備註\r\n" +
            "2026-02-15,支出,120,餐飲,現金,午餐");

        var result = await service.ImportTransactionsAsync(csvStream, 5 * 1024 * 1024 + 1);

        var error = Assert.Single(result.Errors);
        Assert.Contains("5MB", error.ErrorMessage);
        Assert.Equal(0, await context.Transactions.CountAsync());
    }

    [Fact]
    public async Task ImportTransactionsAsync_ShouldEnforceTenThousandRowLimit()
    {
        using var context = CreateInMemoryContext();
        await SeedReferenceDataAsync(context);
        var service = CreateCsvService(context);

        var builder = new StringBuilder("日期,類型,金額,分類,帳戶,備註\r\n");
        for (var row = 1; row <= 10001; row++)
        {
            builder.Append("2026-02-15,支出,1,餐飲,現金,測試")
                .Append("\r\n");
        }

        using var csvStream = CreateCsvStream(builder.ToString());
        var result = await service.ImportTransactionsAsync(csvStream, csvStream.Length);

        Assert.Equal(0, result.SuccessCount);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("10,000", StringComparison.Ordinal));
        Assert.Equal(0, await context.Transactions.CountAsync());
    }

    [Fact]
    public async Task ImportTransactionsAsync_WithHeaderOnly_ShouldReturnNoValidDataMessage()
    {
        using var context = CreateInMemoryContext();
        await SeedReferenceDataAsync(context);
        var service = CreateCsvService(context);
        using var csvStream = CreateCsvStream("日期,類型,金額,分類,帳戶,備註\r\n");

        var result = await service.ImportTransactionsAsync(csvStream, csvStream.Length);

        var error = Assert.Single(result.Errors);
        Assert.Equal("無有效資料", error.ErrorMessage);
        Assert.Equal(0, await context.Transactions.CountAsync());
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

    private static CsvService CreateCsvService(BookKeepingDbContext context)
    {
        return new CsvService(context, new HtmlSanitizer());
    }

    private static MemoryStream CreateCsvStream(string content)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(content));
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
