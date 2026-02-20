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
}
