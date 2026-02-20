using BookKeeping.Models;

namespace BookKeeping.Services;

/// <summary>
/// Service for managing categories
/// </summary>
/// <example>
/// <code>
/// var expenses = await categoryService.GetByTypeAsync(TransactionType.Expense);
/// var created = await categoryService.CreateAsync(new Category { Name = "旅遊", Type = TransactionType.Expense, Icon = "✈️" });
/// </code>
/// </example>
public interface ICategoryService
{
    /// <summary>
    /// Get all categories (excluding soft-deleted)
    /// </summary>
    /// <returns>Active category list.</returns>
    Task<List<Category>> GetAllAsync();

    /// <summary>
    /// Get categories by transaction type
    /// </summary>
    /// <param name="type">Target transaction type.</param>
    /// <returns>Category list for the target type.</returns>
    Task<List<Category>> GetByTypeAsync(TransactionType type);

    /// <summary>
    /// Create a category.
    /// </summary>
    /// <param name="category">Category to create.</param>
    /// <returns>Created category entity.</returns>
    Task<Category> CreateAsync(Category category);

    /// <summary>
    /// Update an existing category.
    /// </summary>
    /// <param name="category">Category values to update.</param>
    /// <returns>Updated category when found; otherwise <see langword="null"/>.</returns>
    Task<Category?> UpdateAsync(Category category);

    /// <summary>
    /// Delete a category when it has no related transactions.
    /// </summary>
    /// <param name="categoryId">Category identifier.</param>
    /// <returns><see langword="true"/> when deleted; otherwise <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(int categoryId);

    /// <summary>
    /// Check whether a category is referenced by any transactions.
    /// </summary>
    /// <param name="categoryId">Category identifier.</param>
    /// <returns><see langword="true"/> when referenced; otherwise <see langword="false"/>.</returns>
    Task<bool> HasTransactionsAsync(int categoryId);

    /// <summary>
    /// Move related transactions to another category, then delete the source category.
    /// </summary>
    /// <param name="categoryId">Source category identifier.</param>
    /// <param name="targetCategoryId">Target category identifier.</param>
    /// <returns><see langword="true"/> when migration and delete succeed; otherwise <see langword="false"/>.</returns>
    Task<bool> DeleteAndMigrateAsync(int categoryId, int targetCategoryId);
}
