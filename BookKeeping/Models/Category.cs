using System.ComponentModel.DataAnnotations;

namespace BookKeeping.Models;

/// <summary>
/// Represents a transaction category (income or expense).
/// </summary>
public class Category : ISoftDeletable, IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier for the category.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category icon (emoji).
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category type (Income or Expense).
    /// </summary>
    [Required]
    public TransactionType Type { get; set; }

    /// <summary>
    /// Gets or sets the color for chart visualization (HEX format, e.g., #FF6384).
    /// </summary>
    [MaxLength(7)]
    public string? Color { get; set; }

    /// <summary>
    /// Gets or sets the sort order for display.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a system default category.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this category is soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this category was deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this category was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this category was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Default expense categories seed data.
    /// </summary>
    public static readonly (string Name, string Icon, string Color)[] DefaultExpenseCategories = new[]
    {
        ("餐飲", "🍽️", "#FF6384"),
        ("交通", "🚗", "#36A2EB"),
        ("娛樂", "🎮", "#FFCE56"),
        ("購物", "🛒", "#4BC0C0"),
        ("居住", "🏠", "#9966FF"),
        ("醫療", "🏥", "#FF9F40"),
        ("教育", "📚", "#C9CBCF"),
        ("其他", "📎", "#7C8798")
    };

    /// <summary>
    /// Default income categories seed data.
    /// </summary>
    public static readonly (string Name, string Icon, string Color)[] DefaultIncomeCategories = new[]
    {
        ("薪資", "💰", "#4CAF50"),
        ("獎金", "🎁", "#8BC34A"),
        ("投資收益", "📈", "#00BCD4"),
        ("其他收入", "💵", "#009688")
    };
}
