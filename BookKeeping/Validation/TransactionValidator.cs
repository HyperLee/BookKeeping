using System.ComponentModel.DataAnnotations;

namespace BookKeeping.Validation;

/// <summary>
/// Validator for Transaction entity business rules.
/// </summary>
public static class TransactionValidator
{
    /// <summary>
    /// Validates a transaction entity.
    /// </summary>
    /// <param name="amount">Transaction amount.</param>
    /// <param name="date">Transaction date.</param>
    /// <param name="categoryId">Category ID.</param>
    /// <param name="accountId">Account ID.</param>
    /// <param name="note">Optional note.</param>
    /// <returns>Validation result with errors if any.</returns>
    public static (bool IsValid, List<string> Errors) Validate(
        decimal amount,
        DateOnly date,
        int categoryId,
        int accountId,
        string? note)
    {
        var errors = new List<string>();

        // Amount must be greater than 0
        if (amount <= 0)
        {
            errors.Add("Amount must be greater than 0");
        }

        // Date must not be in the future
        if (date > DateOnly.FromDateTime(DateTime.Today))
        {
            errors.Add("Transaction date cannot be in the future");
        }

        // CategoryId and AccountId must be positive
        if (categoryId <= 0)
        {
            errors.Add("Category is required");
        }

        if (accountId <= 0)
        {
            errors.Add("Account is required");
        }

        // Note max length 500 chars
        if (!string.IsNullOrEmpty(note) && note.Length > 500)
        {
            errors.Add("Note cannot exceed 500 characters");
        }

        return (errors.Count == 0, errors);
    }
}
