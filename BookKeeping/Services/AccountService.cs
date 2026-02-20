using Microsoft.EntityFrameworkCore;
using BookKeeping.Data;
using BookKeeping.Models;

namespace BookKeeping.Services;

/// <summary>
/// Service implementation for managing accounts
/// </summary>
public class AccountService : IAccountService
{
    private readonly BookKeepingDbContext _context;

    public AccountService(BookKeepingDbContext context)
    {
        _context = context;
    }

    public async Task<List<Account>> GetAllAsync()
    {
        return await _context.Accounts
            .OrderBy(a => a.Id)
            .ToListAsync();
    }

    public async Task<decimal> GetBalanceAsync(int accountId)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId);

        if (account == null)
        {
            return 0;
        }

        // Calculate: InitialBalance + Income - Expense
        var income = await _context.Transactions
            .Where(t => t.AccountId == accountId && t.Type == TransactionType.Income)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var expense = await _context.Transactions
            .Where(t => t.AccountId == accountId && t.Type == TransactionType.Expense)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        return account.InitialBalance + income - expense;
    }
}
