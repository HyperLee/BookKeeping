using BookKeeping.Models;
using BookKeeping.ViewModels;

namespace BookKeeping.Services;

/// <summary>
/// Service for managing budgets and budget usage progress.
/// </summary>
/// <example>
/// <code>
/// var progress = await budgetService.GetAllWithProgressAsync();
/// var budget = await budgetService.CreateAsync(new Budget { CategoryId = 1, Amount = 3000m });
/// </code>
/// </example>
public interface IBudgetService
{
    /// <summary>
    /// Creates a budget.
    /// </summary>
    /// <param name="budget">Budget entity to create.</param>
    /// <returns>The created budget.</returns>
    Task<Budget> CreateAsync(Budget budget);

    /// <summary>
    /// Updates a budget.
    /// </summary>
    /// <param name="budget">Budget entity containing updated values.</param>
    /// <returns>The updated budget when found; otherwise <see langword="null"/>.</returns>
    Task<Budget?> UpdateAsync(Budget budget);

    /// <summary>
    /// Deletes a budget using soft delete.
    /// </summary>
    /// <param name="budgetId">Budget identifier.</param>
    /// <returns><see langword="true"/> when deleted; otherwise <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(int budgetId);

    /// <summary>
    /// Gets all active budgets with usage progress.
    /// </summary>
    /// <param name="referenceDate">Optional date used to determine the target period.</param>
    /// <returns>Budget progress list.</returns>
    Task<List<BudgetProgressViewModel>> GetAllWithProgressAsync(DateOnly? referenceDate = null);

    /// <summary>
    /// Gets budget status for a single category.
    /// </summary>
    /// <param name="categoryId">Expense category identifier.</param>
    /// <param name="referenceDate">Optional date used to determine the target period.</param>
    /// <returns>Budget progress when budget exists; otherwise <see langword="null"/>.</returns>
    Task<BudgetProgressViewModel?> CheckBudgetStatusAsync(int categoryId, DateOnly? referenceDate = null);
}
