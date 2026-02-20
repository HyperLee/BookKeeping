using BookKeeping.Models;

namespace BookKeeping.Services;

/// <summary>
/// Service for managing categories
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Get all categories (excluding soft-deleted)
    /// </summary>
    Task<List<Category>> GetAllAsync();

    /// <summary>
    /// Get categories by transaction type
    /// </summary>
    Task<List<Category>> GetByTypeAsync(TransactionType type);

    /// <summary>
    /// Create a category.
    /// </summary>
    Task<Category> CreateAsync(Category category);

    /// <summary>
    /// Update an existing category.
    /// </summary>
    Task<Category?> UpdateAsync(Category category);

    /// <summary>
    /// Delete a category when it has no related transactions.
    /// </summary>
    Task<bool> DeleteAsync(int categoryId);

    /// <summary>
    /// Check whether a category is referenced by any transactions.
    /// </summary>
    Task<bool> HasTransactionsAsync(int categoryId);

    /// <summary>
    /// Move related transactions to another category, then delete the source category.
    /// </summary>
    Task<bool> DeleteAndMigrateAsync(int categoryId, int targetCategoryId);
}
