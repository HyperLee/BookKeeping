namespace BookKeeping.ViewModels;

/// <summary>
/// View model for monthly report page.
/// </summary>
public class MonthlyReportViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }
    public bool HasData { get; set; }
    public List<CategoryExpenseDto> CategoryExpenses { get; set; } = [];
    public List<DailyTrendDto> DailyTrends { get; set; } = [];
}

/// <summary>
/// DTO for expense category chart data.
/// </summary>
public class CategoryExpenseDto
{
    public string Name { get; set; } = "";
    public string Color { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// DTO for daily trend chart data.
/// </summary>
public class DailyTrendDto
{
    public DateOnly Date { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
}
