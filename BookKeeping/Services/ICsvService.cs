using BookKeeping.ViewModels;

namespace BookKeeping.Services;

/// <summary>
/// Service for importing and exporting transaction data as CSV.
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

    /// <summary>
    /// Imports transactions from a CSV stream.
    /// </summary>
    /// <param name="csvStream">CSV file stream.</param>
    /// <param name="fileSizeBytes">Uploaded file size in bytes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Import result summary with row-level errors.</returns>
    Task<ImportResultViewModel> ImportTransactionsAsync(
        Stream csvStream,
        long fileSizeBytes,
        CancellationToken cancellationToken = default);
}
