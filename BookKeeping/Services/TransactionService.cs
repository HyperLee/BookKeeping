using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using BookKeeping.Data;
using BookKeeping.Models;

namespace BookKeeping.Services;

/// <summary>
/// Service implementation for managing transactions
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly BookKeepingDbContext _context;
    private readonly ILogger<TransactionService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionService"/> class.
    /// </summary>
    /// <param name="context">Database context.</param>
    /// <param name="logger">Structured logger instance.</param>
    public TransactionService(BookKeepingDbContext context, ILogger<TransactionService>? logger = null)
    {
        _context = context;
        _logger = logger ?? NullLogger<TransactionService>.Instance;
    }

    /// <inheritdoc />
    public async Task<(List<Transaction> Transactions, int TotalCount)> GetPagedAsync(
        int page = 1,
        int pageSize = 20,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        int? categoryId = null,
        int? accountId = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        string? keyword = null)
    {
        var query = _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .AsQueryable();

        // Apply filters
        if (startDate.HasValue)
        {
            query = query.Where(t => t.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.Date <= endDate.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }

        if (accountId.HasValue)
        {
            query = query.Where(t => t.AccountId == accountId.Value);
        }

        if (minAmount.HasValue)
        {
            query = query.Where(t => t.Amount >= minAmount.Value);
        }

        if (maxAmount.HasValue)
        {
            query = query.Where(t => t.Amount <= maxAmount.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim().ToLower();
            query = query.Where(t => t.Note != null && t.Note.ToLower().Contains(normalizedKeyword));
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination and ordering
        var transactions = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (transactions, totalCount);
    }

    /// <inheritdoc />
    public async Task<Transaction?> GetByIdAsync(int id)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <inheritdoc />
    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        try
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            _logger.LogInformation(
                "Transaction created. TransactionId={TransactionId} NewValues={@NewValues}",
                transaction.Id,
                BuildTransactionSnapshot(transaction));
            return transaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create transaction for account {AccountId}", transaction.AccountId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Transaction> UpdateAsync(Transaction transaction)
    {
        try
        {
            var existingTransaction = await _context.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == transaction.Id);

            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
            _logger.LogInformation(
                "Transaction updated. TransactionId={TransactionId} OldValues={@OldValues} NewValues={@NewValues}",
                transaction.Id,
                existingTransaction is null ? null : BuildTransactionSnapshot(existingTransaction),
                BuildTransactionSnapshot(transaction));
            return transaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update transaction {TransactionId}", transaction.Id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SoftDeleteAsync(int id)
    {
        try
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction is null)
            {
                return false;
            }

            var oldValues = BuildTransactionSnapshot(transaction);
            transaction.IsDeleted = true;
            transaction.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation(
                "Transaction soft-deleted. TransactionId={TransactionId} OldValues={@OldValues} NewValues={@NewValues}",
                transaction.Id,
                oldValues,
                BuildTransactionSnapshot(transaction));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to soft-delete transaction {TransactionId}", id);
            throw;
        }
    }

    private static object BuildTransactionSnapshot(Transaction transaction)
    {
        return new
        {
            transaction.Id,
            transaction.Date,
            Amount = MaskAmount(transaction.Amount),
            transaction.Type,
            transaction.CategoryId,
            transaction.AccountId,
            transaction.Note,
            transaction.IsDeleted
        };
    }

    private static string MaskAmount(decimal amount)
    {
        var suffix = (decimal.Truncate(decimal.Abs(amount)) % 100m).ToString("00", CultureInfo.InvariantCulture);
        return $"***{suffix}";
    }
}
