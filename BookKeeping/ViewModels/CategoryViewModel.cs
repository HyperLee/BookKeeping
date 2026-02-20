using System.ComponentModel.DataAnnotations;

using BookKeeping.Models;

namespace BookKeeping.ViewModels;

/// <summary>
/// Input model for creating and updating categories.
/// </summary>
public class CategoryInputModel
{
    [Required(ErrorMessage = "請輸入分類名稱")]
    [MaxLength(50, ErrorMessage = "分類名稱最多 50 字")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "請選擇圖示")]
    [MaxLength(10)]
    public string Icon { get; set; } = string.Empty;

    [Required(ErrorMessage = "請選擇類型")]
    public TransactionType Type { get; set; }

    [MaxLength(7)]
    public string? Color { get; set; }
}
