using System.ComponentModel.DataAnnotations;

namespace BookKeeping.Models;

/// <summary>
/// Represents a financial account.
/// </summary>
public class Account : ISoftDeletable, IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier for the account.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the account name.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account type.
    /// </summary>
    [Required]
    public AccountType Type { get; set; }

    /// <summary>
    /// Gets or sets the account icon (emoji).
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the initial balance of the account.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Initial balance cannot be negative")]
    public decimal InitialBalance { get; set; }

    /// <summary>
    /// Gets or sets the currency code (ISO 4217, default: TWD).
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "TWD";

    /// <summary>
    /// Gets or sets a value indicating whether this account is soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this account was deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this account was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Default accounts seed data.
    /// </summary>
    public static readonly (string Name, AccountType Type, string Icon)[] DefaultAccounts = new[]
    {
        ("現金", AccountType.Cash, "💵"),
        ("銀行帳戶", AccountType.Bank, "🏦"),
        ("信用卡", AccountType.CreditCard, "💳")
    };
}
