namespace BookKeeping.ViewModels;

/// <summary>
/// View model for monthly report page.
/// </summary>
/// <example>
/// <code>
/// var report = new MonthlyReportViewModel { Year = 2026, Month = 2 };
/// </code>
/// </example>
public class MonthlyReportViewModel
{
    /// <summary>
    /// Gets or sets selected year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets selected month.
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Gets or sets total monthly income.
    /// </summary>
    public decimal TotalIncome { get; set; }

    /// <summary>
    /// Gets or sets total monthly expense.
    /// </summary>
    public decimal TotalExpense { get; set; }

    /// <summary>
    /// Gets or sets monthly balance.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Gets or sets whether the selected month has any data.
    /// </summary>
    public bool HasData { get; set; }

    /// <summary>
    /// Gets or sets category breakdown rows.
    /// </summary>
    public List<CategoryExpenseDto> CategoryExpenses { get; set; } = [];

    /// <summary>
    /// Gets or sets daily trend rows.
    /// </summary>
    public List<DailyTrendDto> DailyTrends { get; set; } = [];
}

/// <summary>
/// DTO for expense category chart data.
/// </summary>
public class CategoryExpenseDto
{
    /// <summary>
    /// Gets or sets category display name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets chart color.
    /// </summary>
    public string Color { get; set; } = "";

    /// <summary>
    /// Gets or sets category amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets category percentage.
    /// </summary>
    public decimal Percentage { get; set; }
}

/// <summary>
/// DTO for daily trend chart data.
/// </summary>
public class DailyTrendDto
{
    /// <summary>
    /// Gets or sets transaction date.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Gets or sets daily income amount.
    /// </summary>
    public decimal Income { get; set; }

    /// <summary>
    /// Gets or sets daily expense amount.
    /// </summary>
    public decimal Expense { get; set; }
}
