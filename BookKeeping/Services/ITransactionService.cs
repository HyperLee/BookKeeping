using BookKeeping.Models;

namespace BookKeeping.Services;

/// <summary>
/// Service for managing transactions
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Get paginated transactions with optional filtering
    /// </summary>
    Task<(List<Transaction> Transactions, int TotalCount)> GetPagedAsync(
        int page = 1,
        int pageSize = 20,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        int? categoryId = null,
        int? accountId = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        string? keyword = null);

    /// <summary>
    /// Get a transaction by ID
    /// </summary>
    Task<Transaction?> GetByIdAsync(int id);

    /// <summary>
    /// Create a new transaction
    /// </summary>
    Task<Transaction> CreateAsync(Transaction transaction);

    /// <summary>
    /// Update an existing transaction
    /// </summary>
    Task<Transaction> UpdateAsync(Transaction transaction);

    /// <summary>
    /// Soft delete a transaction
    /// </summary>
    Task<bool> SoftDeleteAsync(int id);
}
