namespace BookKeeping.ViewModels;

/// <summary>
/// Represents the result of a CSV import operation.
/// </summary>
public class ImportResultViewModel
{
    /// <summary>
    /// Gets or sets the total number of processed data rows.
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// Gets or sets the number of successfully imported rows.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the number of failed rows.
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Gets or sets the row-level import errors.
    /// </summary>
    public List<ImportError> Errors { get; set; } = [];
}

/// <summary>
/// Represents a row-level CSV import error.
/// </summary>
public class ImportError
{
    /// <summary>
    /// Gets or sets the CSV line number.
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
