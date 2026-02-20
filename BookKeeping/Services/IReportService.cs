using BookKeeping.ViewModels;

namespace BookKeeping.Services;

/// <summary>
/// Service for generating monthly report data.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Gets monthly summary totals for a specific year and month.
    /// </summary>
    /// <param name="year">Target year.</param>
    /// <param name="month">Target month (1-12).</param>
    /// <returns>Total income, total expense, balance, and whether the month has any transactions.</returns>
    Task<(decimal TotalIncome, decimal TotalExpense, decimal Balance, bool HasData)> GetMonthlySummaryAsync(int year, int month);

    /// <summary>
    /// Gets expense category breakdown for a specific year and month.
    /// </summary>
    /// <param name="year">Target year.</param>
    /// <param name="month">Target month (1-12).</param>
    /// <returns>Expense category breakdown with percentage information.</returns>
    Task<List<CategoryExpenseDto>> GetCategoryBreakdownAsync(int year, int month);

    /// <summary>
    /// Gets daily income and expense trends for a specific year and month.
    /// </summary>
    /// <param name="year">Target year.</param>
    /// <param name="month">Target month (1-12).</param>
    /// <returns>Daily trend entries for chart rendering.</returns>
    Task<List<DailyTrendDto>> GetDailyTrendsAsync(int year, int month);
}
