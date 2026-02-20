using System.ComponentModel.DataAnnotations;

using BookKeeping.Models;

namespace BookKeeping.ViewModels;

/// <summary>
/// Input model for creating and updating accounts.
/// </summary>
public class AccountInputModel
{
    [Required(ErrorMessage = "請輸入帳戶名稱")]
    [MaxLength(50, ErrorMessage = "帳戶名稱最多 50 字")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "請選擇帳戶類型")]
    public AccountType Type { get; set; }

    [Required(ErrorMessage = "請選擇圖示")]
    [MaxLength(10)]
    public string Icon { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "初始餘額不可為負數")]
    public decimal InitialBalance { get; set; }
}
