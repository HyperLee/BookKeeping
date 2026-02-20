using System.ComponentModel.DataAnnotations;

namespace BookKeeping.Models;

/// <summary>
/// Represents a budget for a category.
/// </summary>
public class Budget : ISoftDeletable, IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier for the budget.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the category ID (foreign key to expense category).
    /// </summary>
    [Required]
    public int CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the budget amount.
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Budget amount must be greater than 0")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the budget period (Monthly or Weekly).
    /// </summary>
    [Required]
    public BudgetPeriod Period { get; set; } = BudgetPeriod.Monthly;

    /// <summary>
    /// Gets or sets the budget start date.
    /// </summary>
    [Required]
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this budget is soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this budget was deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this budget was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this budget was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
