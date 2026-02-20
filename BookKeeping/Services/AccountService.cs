using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using BookKeeping.Data;
using BookKeeping.Models;

namespace BookKeeping.Services;

/// <summary>
/// Service implementation for managing accounts
/// </summary>
public class AccountService : IAccountService
{
    private readonly BookKeepingDbContext _context;
    private readonly ILogger<AccountService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountService"/> class.
    /// </summary>
    /// <param name="context">Database context.</param>
    public AccountService(BookKeepingDbContext context, ILogger<AccountService>? logger = null)
    {
        _context = context;
        _logger = logger ?? NullLogger<AccountService>.Instance;
    }

    /// <summary>
    /// Get all active accounts ordered by identifier.
    /// </summary>
    public async Task<List<Account>> GetAllAsync()
    {
        return await _context.Accounts
            .OrderBy(a => a.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Calculate account balance as initial balance plus income minus expense.
    /// </summary>
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

    /// <summary>
    /// Create an account after uniqueness validation.
    /// </summary>
    public async Task<Account> CreateAsync(Account account)
    {
        try
        {
            var normalizedName = account.Name.Trim();
            var hasDuplicate = await _context.Accounts
                .AnyAsync(a => a.Name == normalizedName);

            if (hasDuplicate)
            {
                throw new InvalidOperationException("帳戶名稱已存在");
            }

            account.Name = normalizedName;
            account.Icon = account.Icon.Trim();

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            _logger.LogInformation(
                "Account created. AccountId={AccountId} NewValues={@NewValues}",
                account.Id,
                BuildAccountSnapshot(account));
            return account;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create account {AccountName}", account.Name);
            throw;
        }
    }

    /// <summary>
    /// Update an existing account after uniqueness validation.
    /// </summary>
    public async Task<Account?> UpdateAsync(Account account)
    {
        try
        {
            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == account.Id);

            if (existingAccount is null)
            {
                return null;
            }

            var oldValues = BuildAccountSnapshot(existingAccount);
            var normalizedName = account.Name.Trim();
            var hasDuplicate = await _context.Accounts
                .AnyAsync(a => a.Id != account.Id && a.Name == normalizedName);

            if (hasDuplicate)
            {
                throw new InvalidOperationException("帳戶名稱已存在");
            }

            existingAccount.Name = normalizedName;
            existingAccount.Type = account.Type;
            existingAccount.Icon = account.Icon.Trim();
            existingAccount.InitialBalance = account.InitialBalance;

            await _context.SaveChangesAsync();
            _logger.LogInformation(
                "Account updated. AccountId={AccountId} OldValues={@OldValues} NewValues={@NewValues}",
                existingAccount.Id,
                oldValues,
                BuildAccountSnapshot(existingAccount));
            return existingAccount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update account {AccountId}", account.Id);
            throw;
        }

    }

    /// <summary>
    /// Delete an account when it has no related transactions.
    /// </summary>
    public async Task<bool> DeleteAsync(int accountId)
    {
        try
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account is null || await HasTransactionsAsync(accountId))
            {
                return false;
            }

            var oldValues = BuildAccountSnapshot(account);
            account.IsDeleted = true;
            account.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation(
                "Account deleted. AccountId={AccountId} OldValues={@OldValues} NewValues={@NewValues}",
                account.Id,
                oldValues,
                BuildAccountSnapshot(account));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete account {AccountId}", accountId);
            throw;
        }
    }

    /// <summary>
    /// Check whether an account is referenced by any transactions.
    /// </summary>
    public async Task<bool> HasTransactionsAsync(int accountId)
    {
        return await _context.Transactions.AnyAsync(t => t.AccountId == accountId);
    }

    private static object BuildAccountSnapshot(Account account)
    {
        return new
        {
            account.Id,
            account.Name,
            account.Type,
            account.Icon,
            InitialBalance = MaskAmount(account.InitialBalance),
            account.Currency,
            account.IsDeleted
        };
    }

    private static string MaskAmount(decimal amount)
    {
        var suffix = (decimal.Truncate(decimal.Abs(amount)) % 100m).ToString("00", CultureInfo.InvariantCulture);
        return $"***{suffix}";
    }
}
