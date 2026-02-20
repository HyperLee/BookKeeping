using Microsoft.EntityFrameworkCore;
using BookKeeping.Data;
using BookKeeping.Models;

namespace BookKeeping.Services;

/// <summary>
/// Service implementation for managing transactions
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly BookKeepingDbContext _context;

    public TransactionService(BookKeepingDbContext context)
    {
        _context = context;
    }

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

    public async Task<Transaction?> GetByIdAsync(int id)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<Transaction> UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null)
        {
            return false;
        }

        transaction.IsDeleted = true;
        transaction.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
