using System.ComponentModel.DataAnnotations;
using BookKeeping.Models;

namespace BookKeeping.ViewModels;

/// <summary>
/// View model for displaying budget usage progress.
/// </summary>
/// <example>
/// <code>
/// var progress = new BudgetProgressViewModel { BudgetId = 1, UsageRate = 75m, Status = "normal" };
/// </code>
/// </example>
public class BudgetProgressViewModel
{
    /// <summary>
    /// Gets or sets the budget identifier.
    /// </summary>
    public int BudgetId { get; set; }

    /// <summary>
    /// Gets or sets the linked expense category identifier.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the category display name.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category icon.
    /// </summary>
    public string CategoryIcon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the configured budget amount.
    /// </summary>
    public decimal BudgetAmount { get; set; }

    /// <summary>
    /// Gets or sets the spent amount in the current period.
    /// </summary>
    public decimal SpentAmount { get; set; }

    /// <summary>
    /// Gets or sets the budget usage rate in percentage.
    /// </summary>
    public decimal UsageRate { get; set; }

    /// <summary>
    /// Gets or sets the budget status value (normal, warning, exceeded).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the budget period.
    /// </summary>
    public BudgetPeriod Period { get; set; }

    /// <summary>
    /// Gets or sets the budget start date.
    /// </summary>
    public DateOnly StartDate { get; set; }
}

/// <summary>
/// Input model for creating and updating budgets.
/// </summary>
/// <example>
/// <code>
/// var input = new BudgetInputModel { CategoryId = 1, Amount = 2000m, Period = BudgetPeriod.Monthly };
/// </code>
/// </example>
public class BudgetInputModel
{
    /// <summary>
    /// Gets or sets the selected expense category identifier.
    /// </summary>
    [Required(ErrorMessage = "請選擇分類")]
    public int CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the budget amount.
    /// </summary>
    [Required(ErrorMessage = "請輸入預算金額")]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "預算金額必須大於零")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the budget period.
    /// </summary>
    public BudgetPeriod Period { get; set; } = BudgetPeriod.Monthly;

    /// <summary>
    /// Gets or sets an optional budget start date.
    /// </summary>
    public DateOnly? StartDate { get; set; }
}
