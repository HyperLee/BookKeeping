using System.ComponentModel.DataAnnotations;
using BookKeeping.Models;

namespace BookKeeping.ViewModels;

/// <summary>
/// Input model for creating or editing transactions
/// </summary>
public class TransactionInputModel
{
    [Required(ErrorMessage = "請選擇日期")]
    public DateOnly Date { get; set; }

    [Required(ErrorMessage = "請輸入金額")]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "金額必須大於零")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "請選擇類型")]
    public TransactionType Type { get; set; }

    [Required(ErrorMessage = "請選擇分類")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "請選擇帳戶")]
    public int AccountId { get; set; }

    [MaxLength(500, ErrorMessage = "備註最多 500 字")]
    public string? Note { get; set; }
}

/// <summary>
/// DTO for displaying transaction information
/// </summary>
public class TransactionDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string CategoryName { get; set; } = "";
    public string CategoryIcon { get; set; } = "";
    public string AccountName { get; set; } = "";
    public string? Note { get; set; }
}

/// <summary>
/// Filter for transaction queries
/// </summary>
public class TransactionFilter
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? CategoryId { get; set; }
    public int? AccountId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Keyword { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// View model for transaction list page
/// </summary>
public class TransactionListViewModel
{
    public List<TransactionDto> Transactions { get; set; } = [];
    public TransactionFilter Filter { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public List<CategoryDto> Categories { get; set; } = [];
    public List<AccountDto> Accounts { get; set; } = [];
}

/// <summary>
/// DTO for category information
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public TransactionType Type { get; set; }
}

/// <summary>
/// DTO for account information
/// </summary>
public class AccountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
}
