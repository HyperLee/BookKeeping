using BookKeeping.Models;

namespace BookKeeping.ViewModels;

/// <summary>
/// View model for the Dashboard page
/// </summary>
public class DashboardViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }  // TotalIncome - TotalExpense
    public List<AccountBalanceDto> AccountBalances { get; set; } = [];
    public List<BudgetProgressDto> BudgetProgress { get; set; } = [];
    public List<TransactionDto> RecentTransactions { get; set; } = [];
}

/// <summary>
/// DTO for account balance display
/// </summary>
public class AccountBalanceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public decimal CurrentBalance { get; set; }
}

/// <summary>
/// DTO for budget progress display
/// </summary>
public class BudgetProgressDto
{
    public int BudgetId { get; set; }
    public string CategoryName { get; set; } = "";
    public string CategoryIcon { get; set; } = "";
    public decimal BudgetAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal UsageRate { get; set; }  // percentage (0-100+)
    public string Status { get; set; } = ""; // "normal" | "warning" | "exceeded"
}
