using BookKeeping.Data;
using BookKeeping.Models;
using BookKeeping.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookKeeping.Services;

/// <summary>
/// Service implementation for monthly report aggregation.
/// </summary>
public class ReportService : IReportService
{
    private const string DefaultCategoryColor = "#6C757D";
    private readonly BookKeepingDbContext _context;
    private readonly ILogger<ReportService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportService"/> class.
    /// </summary>
    /// <param name="context">Database context.</param>
    public ReportService(BookKeepingDbContext context, ILogger<ReportService>? logger = null)
    {
        _context = context;
        _logger = logger ?? NullLogger<ReportService>.Instance;
    }

    /// <inheritdoc />
    public async Task<(decimal TotalIncome, decimal TotalExpense, decimal Balance, bool HasData)> GetMonthlySummaryAsync(int year, int month)
    {
        try
        {
            var (startDate, endDate) = GetMonthRange(year, month);
            var monthTransactions = _context.Transactions
                .Where(t => t.Date >= startDate && t.Date <= endDate);

            var totalIncome = await monthTransactions
                .Where(t => t.Type == TransactionType.Income)
                .SumAsync(t => (decimal?)t.Amount) ?? 0m;

            var totalExpense = await monthTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .SumAsync(t => (decimal?)t.Amount) ?? 0m;

            var hasData = await monthTransactions.AnyAsync();
            _logger.LogInformation(
                "Generated monthly summary for {Year}-{Month}. HasData={HasData}",
                year,
                month,
                hasData);
            return (totalIncome, totalExpense, totalIncome - totalExpense, hasData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate monthly summary for {Year}-{Month}", year, month);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<CategoryExpenseDto>> GetCategoryBreakdownAsync(int year, int month)
    {
        try
        {
            var (startDate, endDate) = GetMonthRange(year, month);

            var expenses = await _context.Transactions
                .Where(t => t.Date >= startDate && t.Date <= endDate && t.Type == TransactionType.Expense)
                .Include(t => t.Category)
                .ToListAsync();

            if (expenses.Count == 0)
            {
                _logger.LogInformation("No expense data for category breakdown {Year}-{Month}", year, month);
                return [];
            }

            var groupedExpenses = expenses
                .GroupBy(t => new
                {
                    CategoryName = t.Category?.Name ?? "未分類",
                    CategoryColor = !string.IsNullOrWhiteSpace(t.Category?.Color)
                        ? t.Category!.Color!
                        : DefaultCategoryColor
                })
                .Select(g => new
                {
                    Name = g.Key.CategoryName,
                    Color = g.Key.CategoryColor,
                    Amount = g.Sum(t => t.Amount)
                })
                .OrderByDescending(g => g.Amount)
                .ToList();

            var totalExpense = groupedExpenses.Sum(g => g.Amount);
            var categoryBreakdown = new List<CategoryExpenseDto>(groupedExpenses.Count);
            decimal allocatedPercentage = 0m;

            for (var index = 0; index < groupedExpenses.Count; index++)
            {
                var category = groupedExpenses[index];
                var percentage = index == groupedExpenses.Count - 1
                    ? Math.Max(0m, 100m - allocatedPercentage)
                    : Math.Round(category.Amount / totalExpense * 100m, 2, MidpointRounding.AwayFromZero);

                allocatedPercentage += percentage;
                categoryBreakdown.Add(new CategoryExpenseDto
                {
                    Name = category.Name,
                    Color = category.Color,
                    Amount = category.Amount,
                    Percentage = percentage
                });
            }

            _logger.LogInformation(
                "Generated category breakdown for {Year}-{Month}. CategoryCount={CategoryCount}",
                year,
                month,
                categoryBreakdown.Count);
            return categoryBreakdown;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate category breakdown for {Year}-{Month}", year, month);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<DailyTrendDto>> GetDailyTrendsAsync(int year, int month)
    {
        try
        {
            var (startDate, endDate) = GetMonthRange(year, month);

            var transactions = await _context.Transactions
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .Select(t => new
                {
                    t.Date,
                    t.Type,
                    t.Amount
                })
                .ToListAsync();

            var dailyTrends = transactions
                .GroupBy(t => t.Date)
                .OrderBy(g => g.Key)
                .Select(g => new DailyTrendDto
                {
                    Date = g.Key,
                    Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    Expense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                })
                .ToList();
            _logger.LogInformation(
                "Generated daily trends for {Year}-{Month}. DayCount={DayCount}",
                year,
                month,
                dailyTrends.Count);
            return dailyTrends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate daily trends for {Year}-{Month}", year, month);
            throw;
        }
    }

    /// <summary>
    /// Gets the date range for a specific month.
    /// </summary>
    /// <param name="year">Target year.</param>
    /// <param name="month">Target month.</param>
    /// <returns>Start and end dates of the target month.</returns>
    private static (DateOnly StartDate, DateOnly EndDate) GetMonthRange(int year, int month)
    {
        if (year is < 1 or > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 1 and 9999.");
        }

        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        }

        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        return (startDate, endDate);
    }
}
