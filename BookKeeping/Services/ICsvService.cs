namespace BookKeeping.Services;

/// <summary>
/// Service for exporting transaction data as CSV.
/// </summary>
public interface ICsvService
{
    /// <summary>
    /// Exports transactions to UTF-8 BOM encoded CSV with optional date range filtering.
    /// </summary>
    /// <param name="startDate">Optional inclusive start date.</param>
    /// <param name="endDate">Optional inclusive end date.</param>
    /// <returns>CSV file bytes encoded as UTF-8 with BOM.</returns>
    Task<byte[]> ExportTransactionsAsync(DateOnly? startDate = null, DateOnly? endDate = null);
}
