using System.Globalization;
using System.Text;
using BookKeeping.Data;
using BookKeeping.Models;
using BookKeeping.ViewModels;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Services;

/// <summary>
/// Service implementation for importing and exporting RFC 4180 compatible CSV data.
/// </summary>
public class CsvService : ICsvService
{
    private const string HeaderRow = "日期,類型,金額,分類,帳戶,備註";
    private const string CsvNewLine = "\r\n";
    private const long MaxImportFileSizeBytes = 5 * 1024 * 1024;
    private const int MaxImportRowCount = 10000;
    private readonly BookKeepingDbContext _context;
    private readonly HtmlSanitizer _htmlSanitizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvService"/> class.
    /// </summary>
    /// <param name="context">Database context.</param>
    /// <param name="htmlSanitizer">HTML sanitizer for imported text fields.</param>
    public CsvService(BookKeepingDbContext context, HtmlSanitizer htmlSanitizer)
    {
        _context = context;
        _htmlSanitizer = htmlSanitizer;
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportTransactionsAsync(DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Account)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(t => t.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.Date <= endDate.Value);
        }

        var transactions = await query
            .OrderBy(t => t.Date)
            .ThenBy(t => t.Id)
            .ToListAsync();

        var csvContent = BuildCsvContent(transactions);
        return EncodeWithUtf8Bom(csvContent);
    }

    /// <inheritdoc />
    public async Task<ImportResultViewModel> ImportTransactionsAsync(
        Stream csvStream,
        long fileSizeBytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(csvStream);

        var result = new ImportResultViewModel();
        if (fileSizeBytes > MaxImportFileSizeBytes)
        {
            result.Errors.Add(new ImportError
            {
                LineNumber = 0,
                ErrorMessage = "CSV 檔案大小不可超過 5MB"
            });

            return result;
        }

        var accountLookup = await _context.Accounts
            .AsNoTracking()
            .ToDictionaryAsync(
                account => NormalizeLookupValue(account.Name),
                account => account.Id,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);

        var categoryLookup = (await _context.Categories.ToListAsync(cancellationToken))
            .ToDictionary(
                category => BuildCategoryLookupKey(category.Type, category.Name),
                category => category,
                StringComparer.OrdinalIgnoreCase);

        var transactionsToCreate = new List<Transaction>();
        using var reader = new StreamReader(
            csvStream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: true);

        var (headerRow, headerLineCount) = await ReadCsvRecordAsync(reader, cancellationToken);
        if (headerRow is null)
        {
            result.Errors.Add(new ImportError
            {
                LineNumber = 0,
                ErrorMessage = "無有效資料"
            });

            return result;
        }

        var currentLineNumber = headerLineCount;
        while (true)
        {
            var (record, lineCount) = await ReadCsvRecordAsync(reader, cancellationToken);
            if (record is null)
            {
                break;
            }

            currentLineNumber += lineCount;
            if (string.IsNullOrWhiteSpace(record))
            {
                continue;
            }

            result.TotalRows++;
            if (result.TotalRows > MaxImportRowCount)
            {
                result.SuccessCount = 0;
                result.FailedCount = result.TotalRows;
                result.Errors.Add(new ImportError
                {
                    LineNumber = currentLineNumber,
                    ErrorMessage = "CSV 匯入筆數不可超過 10,000 筆"
                });

                return result;
            }

            var columns = ParseCsvRow(record);
            if (columns.Count != 6)
            {
                result.FailedCount++;
                result.Errors.Add(new ImportError
                {
                    LineNumber = currentLineNumber,
                    ErrorMessage = "欄位數量不正確，應為 6 欄"
                });

                continue;
            }

            if (!DateOnly.TryParseExact(
                    columns[0].Trim(),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
            {
                result.FailedCount++;
                result.Errors.Add(new ImportError
                {
                    LineNumber = currentLineNumber,
                    ErrorMessage = "日期格式無效，請使用 YYYY-MM-DD"
                });

                continue;
            }

            if (!TryParseTransactionType(columns[1], out var transactionType))
            {
                result.FailedCount++;
                result.Errors.Add(new ImportError
                {
                    LineNumber = currentLineNumber,
                    ErrorMessage = "交易類型無效，請使用收入或支出"
                });

                continue;
            }

            if (!decimal.TryParse(
                    columns[2].Trim(),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var amount)
                || amount <= 0)
            {
                result.FailedCount++;
                result.Errors.Add(new ImportError
                {
                    LineNumber = currentLineNumber,
                    ErrorMessage = "金額必須大於 0"
                });

                continue;
            }

            var categoryName = SanitizeText(columns[3]);
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                result.FailedCount++;
                result.Errors.Add(new ImportError
                {
                    LineNumber = currentLineNumber,
                    ErrorMessage = "分類不可為空"
                });

                continue;
            }

            var accountName = SanitizeText(columns[4]);
            if (string.IsNullOrWhiteSpace(accountName))
            {
                result.FailedCount++;
                result.Errors.Add(new ImportError
                {
                    LineNumber = currentLineNumber,
                    ErrorMessage = "帳戶不可為空"
                });

                continue;
            }

            if (!accountLookup.TryGetValue(NormalizeLookupValue(accountName), out var accountId))
            {
                result.FailedCount++;
                result.Errors.Add(new ImportError
                {
                    LineNumber = currentLineNumber,
                    ErrorMessage = $"找不到帳戶：{accountName}"
                });

                continue;
            }

            var categoryKey = BuildCategoryLookupKey(transactionType, categoryName);
            if (!categoryLookup.TryGetValue(categoryKey, out var category))
            {
                category = new Category
                {
                    Name = categoryName,
                    Type = transactionType,
                    Icon = transactionType == TransactionType.Income ? "💰" : "📎",
                    Color = transactionType == TransactionType.Income ? "#4CAF50" : "#7C8798",
                    IsDefault = false
                };

                _context.Categories.Add(category);
                categoryLookup[categoryKey] = category;
            }

            var note = SanitizeText(columns[5]);
            transactionsToCreate.Add(new Transaction
            {
                Date = date,
                Type = transactionType,
                Amount = amount,
                Category = category,
                AccountId = accountId,
                Note = string.IsNullOrWhiteSpace(note) ? null : note
            });
            result.SuccessCount++;
        }

        if (result.TotalRows == 0)
        {
            result.Errors.Add(new ImportError
            {
                LineNumber = 0,
                ErrorMessage = "無有效資料"
            });

            return result;
        }

        if (result.SuccessCount > 0)
        {
            _context.Transactions.AddRange(transactionsToCreate);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Builds CSV text with a fixed header and transaction rows.
    /// </summary>
    /// <param name="transactions">Transactions to export.</param>
    /// <returns>RFC 4180 compatible CSV content.</returns>
    private static string BuildCsvContent(IEnumerable<Transaction> transactions)
    {
        var builder = new StringBuilder();
        builder.Append(HeaderRow).Append(CsvNewLine);

        foreach (var transaction in transactions)
        {
            var row = string.Join(',',
                EscapeCsvField(transaction.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                EscapeCsvField(GetTransactionTypeLabel(transaction.Type)),
                EscapeCsvField(transaction.Amount.ToString(CultureInfo.InvariantCulture)),
                EscapeCsvField(transaction.Category?.Name ?? string.Empty),
                EscapeCsvField(transaction.Account?.Name ?? string.Empty),
                EscapeCsvField(transaction.Note ?? string.Empty));

            builder.Append(row).Append(CsvNewLine);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Converts transaction type values to localized CSV labels.
    /// </summary>
    /// <param name="type">Transaction type.</param>
    /// <returns>Localized label.</returns>
    private static string GetTransactionTypeLabel(TransactionType type)
    {
        return type switch
        {
            TransactionType.Income => "收入",
            TransactionType.Expense => "支出",
            _ => type.ToString()
        };
    }

    /// <summary>
    /// Escapes a CSV field according to RFC 4180.
    /// </summary>
    /// <param name="value">Original field value.</param>
    /// <returns>Escaped field value.</returns>
    private static string EscapeCsvField(string value)
    {
        var escapedValue = value.Replace("\"", "\"\"");

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{escapedValue}\"";
        }

        return escapedValue;
    }

    /// <summary>
    /// Parses transaction type values from CSV text.
    /// </summary>
    /// <param name="typeText">Raw transaction type text.</param>
    /// <param name="type">Parsed transaction type.</param>
    /// <returns>True when parsing succeeds.</returns>
    private static bool TryParseTransactionType(string typeText, out TransactionType type)
    {
        var normalizedType = typeText.Trim();
        if (normalizedType.Equals("收入", StringComparison.OrdinalIgnoreCase)
            || normalizedType.Equals("Income", StringComparison.OrdinalIgnoreCase))
        {
            type = TransactionType.Income;
            return true;
        }

        if (normalizedType.Equals("支出", StringComparison.OrdinalIgnoreCase)
            || normalizedType.Equals("Expense", StringComparison.OrdinalIgnoreCase))
        {
            type = TransactionType.Expense;
            return true;
        }

        type = default;
        return false;
    }

    /// <summary>
    /// Reads a full CSV record from the stream reader, supporting embedded newlines in quoted fields.
    /// </summary>
    /// <param name="reader">CSV stream reader.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Record content and physical line count consumed.</returns>
    private static async Task<(string? Record, int LineCount)> ReadCsvRecordAsync(
        StreamReader reader,
        CancellationToken cancellationToken)
    {
        var firstLine = await reader.ReadLineAsync(cancellationToken);
        if (firstLine is null)
        {
            return (null, 0);
        }

        var lineCount = 1;
        var valueBuilder = new StringBuilder(firstLine);
        while (HasUnclosedQuote(valueBuilder))
        {
            var nextLine = await reader.ReadLineAsync(cancellationToken);
            if (nextLine is null)
            {
                break;
            }

            valueBuilder.Append('\n').Append(nextLine);
            lineCount++;
        }

        return (valueBuilder.ToString(), lineCount);
    }

    /// <summary>
    /// Determines whether a CSV value has unclosed quotation marks.
    /// </summary>
    /// <param name="valueBuilder">CSV value builder.</param>
    /// <returns>True when the value has unclosed quotes.</returns>
    private static bool HasUnclosedQuote(StringBuilder valueBuilder)
    {
        var quoteCount = 0;
        foreach (var character in valueBuilder.ToString())
        {
            if (character == '"')
            {
                quoteCount++;
            }
        }

        return quoteCount % 2 != 0;
    }

    /// <summary>
    /// Parses a CSV row into column values according to RFC 4180 quote escaping rules.
    /// </summary>
    /// <param name="row">Raw CSV row text.</param>
    /// <returns>Parsed columns.</returns>
    private static List<string> ParseCsvRow(string row)
    {
        var columns = new List<string>();
        var fieldBuilder = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < row.Length; index++)
        {
            var character = row[index];
            if (character == '"')
            {
                if (inQuotes && index + 1 < row.Length && row[index + 1] == '"')
                {
                    fieldBuilder.Append('"');
                    index++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (character == ',' && !inQuotes)
            {
                columns.Add(fieldBuilder.ToString());
                fieldBuilder.Clear();
                continue;
            }

            fieldBuilder.Append(character);
        }

        columns.Add(fieldBuilder.ToString());
        return columns;
    }

    /// <summary>
    /// Builds a lookup key for category matching.
    /// </summary>
    /// <param name="type">Transaction type.</param>
    /// <param name="categoryName">Category name.</param>
    /// <returns>Lookup key.</returns>
    private static string BuildCategoryLookupKey(TransactionType type, string categoryName)
    {
        return $"{(int)type}:{NormalizeLookupValue(categoryName)}";
    }

    /// <summary>
    /// Normalizes imported lookup values.
    /// </summary>
    /// <param name="value">Raw imported text.</param>
    /// <returns>Normalized text.</returns>
    private static string NormalizeLookupValue(string value)
    {
        return value.Trim();
    }

    /// <summary>
    /// Sanitizes imported text fields to prevent script injection.
    /// </summary>
    /// <param name="value">Raw imported text.</param>
    /// <returns>Sanitized text.</returns>
    private string SanitizeText(string value)
    {
        return _htmlSanitizer.Sanitize(value).Trim();
    }

    /// <summary>
    /// Encodes text using UTF-8 and prepends BOM bytes for Excel compatibility.
    /// </summary>
    /// <param name="content">CSV text content.</param>
    /// <returns>UTF-8 BOM encoded bytes.</returns>
    private static byte[] EncodeWithUtf8Bom(string content)
    {
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        var preamble = encoding.GetPreamble();
        var contentBytes = encoding.GetBytes(content);
        var result = new byte[preamble.Length + contentBytes.Length];

        Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
        Buffer.BlockCopy(contentBytes, 0, result, preamble.Length, contentBytes.Length);

        return result;
    }
}
