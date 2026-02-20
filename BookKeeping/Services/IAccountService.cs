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

    /// <summary>
    /// Create an account.
    /// </summary>
    Task<Account> CreateAsync(Account account);

    /// <summary>
    /// Update an existing account.
    /// </summary>
    Task<Account?> UpdateAsync(Account account);

    /// <summary>
    /// Delete an account when it has no related transactions.
    /// </summary>
    Task<bool> DeleteAsync(int accountId);

    /// <summary>
    /// Check whether an account is referenced by any transactions.
    /// </summary>
    Task<bool> HasTransactionsAsync(int accountId);
}
