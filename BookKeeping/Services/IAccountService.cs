using BookKeeping.Models;

namespace BookKeeping.Services;

/// <summary>
/// Service for managing accounts
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Get all accounts (excluding soft-deleted)
    /// </summary>
    Task<List<Account>> GetAllAsync();

    /// <summary>
    /// Get the current balance for an account
    /// Calculation: InitialBalance + Income - Expense
    /// </summary>
    Task<decimal> GetBalanceAsync(int accountId);
}
