using BookKeeping.Models;

namespace BookKeeping.Data.Seed;

/// <summary>
/// Seeds default categories and accounts into the database
/// </summary>
public class DefaultDataSeeder
{
    private readonly BookKeepingDbContext _context;

    public DefaultDataSeeder(BookKeepingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Seeds default data if not already present (idempotent)
    /// </summary>
    public async Task SeedAsync()
    {
        await SeedCategoriesAsync();
        await SeedAccountsAsync();
        await _context.SaveChangesAsync();
    }

    private async Task SeedCategoriesAsync()
    {
        // Only seed if no categories exist
        if (_context.Categories.Any())
        {
            return;
        }

        var expenseCategories = new[]
        {
            new Category { Name = "餐飲", Icon = "🍽️", Type = TransactionType.Expense, Color = "#FF6384", SortOrder = 1, IsDefault = true },
            new Category { Name = "交通", Icon = "🚗", Type = TransactionType.Expense, Color = "#36A2EB", SortOrder = 2, IsDefault = true },
            new Category { Name = "娛樂", Icon = "🎮", Type = TransactionType.Expense, Color = "#FFCE56", SortOrder = 3, IsDefault = true },
            new Category { Name = "購物", Icon = "🛒", Type = TransactionType.Expense, Color = "#4BC0C0", SortOrder = 4, IsDefault = true },
            new Category { Name = "居住", Icon = "🏠", Type = TransactionType.Expense, Color = "#9966FF", SortOrder = 5, IsDefault = true },
            new Category { Name = "醫療", Icon = "🏥", Type = TransactionType.Expense, Color = "#FF9F40", SortOrder = 6, IsDefault = true },
            new Category { Name = "教育", Icon = "📚", Type = TransactionType.Expense, Color = "#C9CBCF", SortOrder = 7, IsDefault = true },
            new Category { Name = "其他", Icon = "📎", Type = TransactionType.Expense, Color = "#7C8798", SortOrder = 8, IsDefault = true }
        };

        var incomeCategories = new[]
        {
            new Category { Name = "薪資", Icon = "💰", Type = TransactionType.Income, Color = "#4CAF50", SortOrder = 1, IsDefault = true },
            new Category { Name = "獎金", Icon = "🎁", Type = TransactionType.Income, Color = "#8BC34A", SortOrder = 2, IsDefault = true },
            new Category { Name = "投資收益", Icon = "📈", Type = TransactionType.Income, Color = "#00BCD4", SortOrder = 3, IsDefault = true },
            new Category { Name = "其他收入", Icon = "💵", Type = TransactionType.Income, Color = "#009688", SortOrder = 4, IsDefault = true }
        };

        await _context.Categories.AddRangeAsync(expenseCategories);
        await _context.Categories.AddRangeAsync(incomeCategories);
    }

    private async Task SeedAccountsAsync()
    {
        // Only seed if no accounts exist
        if (_context.Accounts.Any())
        {
            return;
        }

        var defaultAccounts = new[]
        {
            new Account { Name = "現金", Type = AccountType.Cash, Icon = "💵", InitialBalance = 0, Currency = "TWD" },
            new Account { Name = "銀行帳戶", Type = AccountType.Bank, Icon = "🏦", InitialBalance = 0, Currency = "TWD" },
            new Account { Name = "信用卡", Type = AccountType.CreditCard, Icon = "💳", InitialBalance = 0, Currency = "TWD" }
        };

        await _context.Accounts.AddRangeAsync(defaultAccounts);
    }
}
