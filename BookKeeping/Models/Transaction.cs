using System.ComponentModel.DataAnnotations;

namespace BookKeeping.Models;

/// <summary>
/// Represents a financial transaction (income or expense).
/// </summary>
public class Transaction : ISoftDeletable, IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier for the transaction.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the transaction date (ISO 8601: YYYY-MM-DD).
    /// </summary>
    [Required]
    public DateOnly Date { get; set; }

    /// <summary>
    /// Gets or sets the transaction amount (must be positive).
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the type of transaction (Income or Expense).
    /// </summary>
    [Required]
    public TransactionType Type { get; set; }

    /// <summary>
    /// Gets or sets the category ID (foreign key).
    /// </summary>
    [Required]
    public int CategoryId { get; set; }

    /// <summary>
    /// Navigation property to the related category.
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// Gets or sets the account ID (foreign key).
    /// </summary>
    [Required]
    public int AccountId { get; set; }

    /// <summary>
    /// Navigation property to the related account.
    /// </summary>
    public Account? Account { get; set; }

    /// <summary>
    /// Gets or sets optional notes for the transaction.
    /// </summary>
    [MaxLength(500)]
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this transaction is soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this transaction was deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this transaction was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this transaction was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
