using System.ComponentModel.DataAnnotations;

using BookKeeping.Models;

namespace BookKeeping.ViewModels;

/// <summary>
/// Input model for creating and updating categories.
/// </summary>
/// <example>
/// <code>
/// var input = new CategoryInputModel
/// {
///     Name = "交通",
///     Icon = "🚗",
///     Type = TransactionType.Expense,
///     Color = "#36A2EB"
/// };
/// </code>
/// </example>
public class CategoryInputModel
{
    /// <summary>
    /// Gets or sets category name.
    /// </summary>
    [Required(ErrorMessage = "請輸入分類名稱")]
    [MaxLength(50, ErrorMessage = "分類名稱最多 50 字")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets category icon.
    /// </summary>
    [Required(ErrorMessage = "請選擇圖示")]
    [MaxLength(10)]
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets transaction type for the category.
    /// </summary>
    [Required(ErrorMessage = "請選擇類型")]
    public TransactionType Type { get; set; }

    /// <summary>
    /// Gets or sets optional chart color.
    /// </summary>
    [MaxLength(7)]
    public string? Color { get; set; }
}
