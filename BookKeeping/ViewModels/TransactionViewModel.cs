using System.ComponentModel.DataAnnotations;
using BookKeeping.Models;

namespace BookKeeping.ViewModels;

/// <summary>
/// Input model for creating or editing transactions.
/// </summary>
/// <example>
/// <code>
/// var input = new TransactionInputModel
/// {
///     Date = DateOnly.FromDateTime(DateTime.Today),
///     Amount = 250m,
///     Type = TransactionType.Expense,
///     CategoryId = 1,
///     AccountId = 1
/// };
/// </code>
/// </example>
public class TransactionInputModel
{
    /// <summary>
    /// Gets or sets transaction date.
    /// </summary>
    [Required(ErrorMessage = "請選擇日期")]
    public DateOnly Date { get; set; }

    /// <summary>
    /// Gets or sets transaction amount.
    /// </summary>
    [Required(ErrorMessage = "請輸入金額")]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "金額必須大於零")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets transaction type.
    /// </summary>
    [Required(ErrorMessage = "請選擇類型")]
    public TransactionType Type { get; set; }

    /// <summary>
    /// Gets or sets selected category identifier.
    /// </summary>
    [Required(ErrorMessage = "請選擇分類")]
    public int CategoryId { get; set; }

    /// <summary>
    /// Gets or sets selected account identifier.
    /// </summary>
    [Required(ErrorMessage = "請選擇帳戶")]
    public int AccountId { get; set; }

    /// <summary>
    /// Gets or sets optional note text.
    /// </summary>
    [MaxLength(500, ErrorMessage = "備註最多 500 字")]
    public string? Note { get; set; }
}

/// <summary>
/// DTO for displaying transaction information.
/// </summary>
public class TransactionDto
{
    /// <summary>
    /// Gets or sets transaction identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets transaction date.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Gets or sets transaction amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets transaction type.
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Gets or sets category display name.
    /// </summary>
    public string CategoryName { get; set; } = "";

    /// <summary>
    /// Gets or sets category icon.
    /// </summary>
    public string CategoryIcon { get; set; } = "";

    /// <summary>
    /// Gets or sets account display name.
    /// </summary>
    public string AccountName { get; set; } = "";

    /// <summary>
    /// Gets or sets optional note text.
    /// </summary>
    public string? Note { get; set; }
}

/// <summary>
/// Filter for transaction queries.
/// </summary>
public class TransactionFilter
{
    /// <summary>
    /// Gets or sets optional inclusive start date.
    /// </summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// Gets or sets optional inclusive end date.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Gets or sets optional category identifier.
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Gets or sets optional account identifier.
    /// </summary>
    public int? AccountId { get; set; }

    /// <summary>
    /// Gets or sets optional minimum amount.
    /// </summary>
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// Gets or sets optional maximum amount.
    /// </summary>
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// Gets or sets optional keyword for note search.
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// Gets or sets current page number.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets page size.
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// View model for transaction list page.
/// </summary>
public class TransactionListViewModel
{
    /// <summary>
    /// Gets or sets paged transaction rows.
    /// </summary>
    public List<TransactionDto> Transactions { get; set; } = [];

    /// <summary>
    /// Gets or sets current filter values.
    /// </summary>
    public TransactionFilter Filter { get; set; } = new();

    /// <summary>
    /// Gets or sets total records count.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets total pages count.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Gets or sets available categories for filter dropdown.
    /// </summary>
    public List<CategoryDto> Categories { get; set; } = [];

    /// <summary>
    /// Gets or sets available accounts for filter dropdown.
    /// </summary>
    public List<AccountDto> Accounts { get; set; } = [];
}

/// <summary>
/// DTO for category information.
/// </summary>
public class CategoryDto
{
    /// <summary>
    /// Gets or sets category identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets category display name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets category icon.
    /// </summary>
    public string Icon { get; set; } = "";

    /// <summary>
    /// Gets or sets transaction type.
    /// </summary>
    public TransactionType Type { get; set; }
}

/// <summary>
/// DTO for account information.
/// </summary>
public class AccountDto
{
    /// <summary>
    /// Gets or sets account identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets account display name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets account icon.
    /// </summary>
    public string Icon { get; set; } = "";
}
