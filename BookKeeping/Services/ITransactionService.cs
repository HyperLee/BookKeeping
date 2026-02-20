using BookKeeping.Models;

namespace BookKeeping.Services;

/// <summary>
/// Service for managing transactions
/// </summary>
/// <example>
/// <code>
/// var (rows, total) = await transactionService.GetPagedAsync(page: 1, pageSize: 20);
/// var created = await transactionService.CreateAsync(new Transaction { Amount = 100m, Type = TransactionType.Expense });
/// </code>
/// </example>
public interface ITransactionService
{
    /// <summary>
    /// Get paginated transactions with optional filtering
    /// </summary>
    /// <param name="page">Page number starting from 1.</param>
    /// <param name="pageSize">Records per page.</param>
    /// <param name="startDate">Optional inclusive start date.</param>
    /// <param name="endDate">Optional inclusive end date.</param>
    /// <param name="categoryId">Optional category filter.</param>
    /// <param name="accountId">Optional account filter.</param>
    /// <param name="minAmount">Optional minimum amount filter.</param>
    /// <param name="maxAmount">Optional maximum amount filter.</param>
    /// <param name="keyword">Optional note keyword filter.</param>
    /// <returns>Paged transaction list and total count.</returns>
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
    /// <param name="id">Transaction identifier.</param>
    /// <returns>Transaction entity when found; otherwise <see langword="null"/>.</returns>
    Task<Transaction?> GetByIdAsync(int id);

    /// <summary>
    /// Create a new transaction
    /// </summary>
    /// <param name="transaction">Transaction to create.</param>
    /// <returns>Created transaction entity.</returns>
    Task<Transaction> CreateAsync(Transaction transaction);

    /// <summary>
    /// Update an existing transaction
    /// </summary>
    /// <param name="transaction">Transaction values to update.</param>
    /// <returns>Updated transaction entity.</returns>
    Task<Transaction> UpdateAsync(Transaction transaction);

    /// <summary>
    /// Soft delete a transaction
    /// </summary>
    /// <param name="id">Transaction identifier.</param>
    /// <returns><see langword="true"/> when deleted; otherwise <see langword="false"/>.</returns>
    Task<bool> SoftDeleteAsync(int id);
}
