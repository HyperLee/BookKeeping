namespace BookKeeping.Models;

/// <summary>
/// Represents the type of a transaction.
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Income transaction (收入).
    /// </summary>
    Income = 0,

    /// <summary>
    /// Expense transaction (支出).
    /// </summary>
    Expense = 1
}
