using BookKeeping.Models;

namespace BookKeeping.ViewModels;

/// <summary>
/// View model for the Dashboard page.
/// </summary>
/// <example>
/// <code>
/// var dashboard = new DashboardViewModel
/// {
///     Year = 2026,
///     Month = 2,
///     TotalIncome = 5000m,
///     TotalExpense = 3200m
/// };
/// </code>
/// </example>
public class DashboardViewModel
{
    /// <summary>
    /// Gets or sets report year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets report month.
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Gets or sets total income amount.
    /// </summary>
    public decimal TotalIncome { get; set; }

    /// <summary>
    /// Gets or sets total expense amount.
    /// </summary>
    public decimal TotalExpense { get; set; }

    /// <summary>
    /// Gets or sets current balance.
    /// </summary>
    public decimal Balance { get; set; }  // TotalIncome - TotalExpense

    /// <summary>
    /// Gets or sets account balance rows.
    /// </summary>
    public List<AccountBalanceDto> AccountBalances { get; set; } = [];

    /// <summary>
    /// Gets or sets budget progress rows.
    /// </summary>
    public List<BudgetProgressDto> BudgetProgress { get; set; } = [];

    /// <summary>
    /// Gets or sets recent transactions.
    /// </summary>
    public List<TransactionDto> RecentTransactions { get; set; } = [];
}

/// <summary>
/// DTO for account balance display.
/// </summary>
public class AccountBalanceDto
{
    /// <summary>
    /// Gets or sets account identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets account display name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets account icon.
    /// </summary>
    public string Icon { get; set; } = "";

    /// <summary>
    /// Gets or sets current account balance.
    /// </summary>
    public decimal CurrentBalance { get; set; }
}

/// <summary>
/// DTO for budget progress display.
/// </summary>
public class BudgetProgressDto
{
    /// <summary>
    /// Gets or sets budget identifier.
    /// </summary>
    public int BudgetId { get; set; }

    /// <summary>
    /// Gets or sets category name.
    /// </summary>
    public string CategoryName { get; set; } = "";

    /// <summary>
    /// Gets or sets category icon.
    /// </summary>
    public string CategoryIcon { get; set; } = "";

    /// <summary>
    /// Gets or sets budget amount.
    /// </summary>
    public decimal BudgetAmount { get; set; }

    /// <summary>
    /// Gets or sets spent amount.
    /// </summary>
    public decimal SpentAmount { get; set; }

    /// <summary>
    /// Gets or sets usage percentage.
    /// </summary>
    public decimal UsageRate { get; set; }  // percentage (0-100+)

    /// <summary>
    /// Gets or sets budget status.
    /// </summary>
    public string Status { get; set; } = ""; // "normal" | "warning" | "exceeded"
}
