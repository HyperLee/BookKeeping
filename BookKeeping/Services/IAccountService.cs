using BookKeeping.Models;

namespace BookKeeping.Services;

/// <summary>
/// Service for managing accounts
/// </summary>
/// <example>
/// <code>
/// var accounts = await accountService.GetAllAsync();
/// var created = await accountService.CreateAsync(new Account { Name = "旅遊基金", Type = AccountType.Bank, Icon = "🏦" });
/// </code>
/// </example>
public interface IAccountService
{
    /// <summary>
    /// Get all accounts (excluding soft-deleted)
    /// </summary>
    /// <returns>Active account list ordered by identifier.</returns>
    Task<List<Account>> GetAllAsync();

    /// <summary>
    /// Get the current balance for an account
    /// Calculation: InitialBalance + Income - Expense
    /// </summary>
    /// <param name="accountId">Account identifier.</param>
    /// <returns>Computed current balance.</returns>
    /// <example>
    /// <code>
    /// var balance = await accountService.GetBalanceAsync(1);
    /// </code>
    /// </example>
    Task<decimal> GetBalanceAsync(int accountId);

    /// <summary>
    /// Create an account.
    /// </summary>
    /// <param name="account">Account to create.</param>
    /// <returns>Created account entity.</returns>
    Task<Account> CreateAsync(Account account);

    /// <summary>
    /// Update an existing account.
    /// </summary>
    /// <param name="account">Account values to update.</param>
    /// <returns>Updated account when found; otherwise <see langword="null"/>.</returns>
    Task<Account?> UpdateAsync(Account account);

    /// <summary>
    /// Delete an account when it has no related transactions.
    /// </summary>
    /// <param name="accountId">Account identifier.</param>
    /// <returns><see langword="true"/> when deleted; otherwise <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(int accountId);

    /// <summary>
    /// Check whether an account is referenced by any transactions.
    /// </summary>
    /// <param name="accountId">Account identifier.</param>
    /// <returns><see langword="true"/> when referenced; otherwise <see langword="false"/>.</returns>
    Task<bool> HasTransactionsAsync(int accountId);
}
