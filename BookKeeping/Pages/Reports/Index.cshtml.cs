using BookKeeping.Services;
using BookKeeping.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookKeeping.Pages.Reports;

/// <summary>
/// Monthly reports page model.
/// </summary>
public class IndexModel : PageModel
{
    private readonly IReportService _reportService;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexModel"/> class.
    /// </summary>
    /// <param name="reportService">Report service.</param>
    public IndexModel(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Gets report page view model.
    /// </summary>
    public MonthlyReportViewModel ViewModel { get; private set; } = new();

    /// <summary>
    /// Loads monthly report summary for selected year and month.
    /// </summary>
    public async Task OnGetAsync(int? year, int? month)
    {
        var (targetYear, targetMonth) = ResolveYearMonth(year, month);
        var summary = await _reportService.GetMonthlySummaryAsync(targetYear, targetMonth);

        ViewModel = new MonthlyReportViewModel
        {
            Year = targetYear,
            Month = targetMonth,
            TotalIncome = summary.TotalIncome,
            TotalExpense = summary.TotalExpense,
            Balance = summary.Balance,
            HasData = summary.HasData
        };
    }

    /// <summary>
    /// Returns chart data for asynchronous chart rendering.
    /// </summary>
    public async Task<IActionResult> OnGetChartDataAsync(int? year, int? month)
    {
        var (targetYear, targetMonth) = ResolveYearMonth(year, month);
        var categoryExpenses = await _reportService.GetCategoryBreakdownAsync(targetYear, targetMonth);
        var dailyTrends = await _reportService.GetDailyTrendsAsync(targetYear, targetMonth);

        return new JsonResult(new
        {
            categoryExpenses = categoryExpenses.Select(item => new
            {
                label = item.Name,
                value = item.Amount,
                color = item.Color,
                percentage = item.Percentage
            }),
            dailyTrends = dailyTrends.Select(item => new
            {
                date = item.Date.ToString("yyyy-MM-dd"),
                income = item.Income,
                expense = item.Expense
            })
        });
    }

    /// <summary>
    /// Resolves and validates target year and month.
    /// </summary>
    /// <param name="year">Input year.</param>
    /// <param name="month">Input month.</param>
    /// <returns>Validated year and month.</returns>
    private static (int Year, int Month) ResolveYearMonth(int? year, int? month)
    {
        var now = DateTime.Now;
        var resolvedYear = year.GetValueOrDefault(now.Year);
        var resolvedMonth = month.GetValueOrDefault(now.Month);

        if (resolvedYear is < 1 or > 9999)
        {
            resolvedYear = now.Year;
        }

        if (resolvedMonth is < 1 or > 12)
        {
            resolvedMonth = now.Month;
        }

        return (resolvedYear, resolvedMonth);
    }
}
