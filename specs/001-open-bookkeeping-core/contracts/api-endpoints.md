# API 端點與契約：Open BookKeeping

**分支**: `001-open-bookkeeping-core` | **日期**: 2026-02-20

## 概述

本專案採用 ASP.NET Core Razor Pages 架構。頁面導覽使用標準 Razor Pages 路由；表單提交使用 POST handler；需要即時回饋的功能（如預算檢查、圖表資料）透過 AJAX 呼叫 Razor Pages 的 Named Handler 或獨立的 Minimal API 端點。

---

## Razor Pages 路由

### 頁面清單

| 頁面 | 路由 | 說明 | 對應 FR |
|------|------|------|---------|
| Dashboard | `/` (`/Index`) | 首頁：月摘要、帳戶餘額、預算進度、最近紀錄 | FR-032 |
| 交易明細 | `/Transactions` | 紀錄列表（含搜尋/篩選） | FR-029, FR-030 |
| 新增交易 | `/Transactions/Create` | 新增收支紀錄表單 | FR-001, FR-004 |
| 編輯交易 | `/Transactions/Edit/{id}` | 編輯收支紀錄表單 | FR-002 |
| 月度報表 | `/Reports` | 月度摘要 + 圓餅圖 + 趨勢圖 | FR-012~FR-015 |
| 預算管理 | `/Budgets` | 預算設定與進度追蹤 | FR-016~FR-020 |
| 分類管理 | `/Settings/Categories` | 分類 CRUD | FR-006~FR-009 |
| 帳戶管理 | `/Settings/Accounts` | 帳戶 CRUD | FR-010, FR-011 |
| CSV 匯入 | `/Import` | CSV 檔案上傳與匯入 | FR-024, FR-025, FR-034, FR-035 |

---

## Page Handlers 詳細定義

### 1. Dashboard（`/Index`）

#### GET `/`

**PageModel**: `IndexModel`

**OnGetAsync()** — 載入 Dashboard 資料

**回應 ViewModel** (`DashboardViewModel`):
```csharp
public class DashboardViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }  // TotalIncome - TotalExpense
    public List<AccountBalanceDto> AccountBalances { get; set; } = [];
    public List<BudgetProgressDto> BudgetProgress { get; set; } = [];
    public List<TransactionDto> RecentTransactions { get; set; } = [];
}

public class AccountBalanceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public decimal CurrentBalance { get; set; }
}

public class BudgetProgressDto
{
    public int BudgetId { get; set; }
    public string CategoryName { get; set; } = "";
    public string CategoryIcon { get; set; } = "";
    public decimal BudgetAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal UsageRate { get; set; }  // percentage (0-100+)
    public string Status { get; set; } = ""; // "normal" | "warning" | "exceeded"
}
```

---

### 2. 交易明細（`/Transactions`）

#### GET `/Transactions`

**OnGetAsync(TransactionFilter filter)** — 載入交易列表（含篩選/搜尋）

**查詢參數** (`TransactionFilter`):
```csharp
public class TransactionFilter
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? CategoryId { get; set; }
    public int? AccountId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Keyword { get; set; }     // 搜尋備註欄位
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
```

**回應**: 分頁交易列表
```csharp
public class TransactionListViewModel
{
    public List<TransactionDto> Transactions { get; set; } = [];
    public TransactionFilter Filter { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public List<CategoryDto> Categories { get; set; } = [];  // 篩選下拉選單用
    public List<AccountDto> Accounts { get; set; } = [];      // 篩選下拉選單用
}

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
```

---

### 3. 新增交易（`/Transactions/Create`）

#### GET `/Transactions/Create`

**OnGetAsync()** — 載入空白表單 + 分類/帳戶選項

#### POST `/Transactions/Create`

**OnPostAsync(TransactionInputModel input)** — 儲存新交易

**輸入模型** (`TransactionInputModel`):
```csharp
public class TransactionInputModel
{
    [Required(ErrorMessage = "請選擇日期")]
    public DateOnly Date { get; set; }  // MUST NOT be a future date (constitution §VII)

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
```

**成功回應**: Redirect to `/Transactions` + Toast 成功訊息（TempData）
**失敗回應**: 返回表單頁面 + 行內驗證錯誤 + Toast 錯誤訊息

---

### 4. 編輯交易（`/Transactions/Edit`）

#### GET `/Transactions/Edit/{id}`

**OnGetAsync(int id)** — 載入既有交易資料

#### POST `/Transactions/Edit/{id}`

**OnPostAsync(int id, TransactionInputModel input)** — 更新交易

**回應**: 同新增交易

---

### 5. 刪除交易

#### POST `/Transactions?handler=Delete`

**OnPostDeleteAsync(int id)** — 軟刪除交易（需 Anti-Forgery Token）

**回應**: Redirect to `/Transactions` + Toast 成功訊息

---

### 6. 月度報表（`/Reports`）

#### GET `/Reports?year={year}&month={month}`

**OnGetAsync(int? year, int? month)** — 載入月度摘要（預設當月）

**回應 ViewModel** (`MonthlyReportViewModel`):
```csharp
public class MonthlyReportViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }
    public bool HasData { get; set; }  // 是否有紀錄（FR-015 空白狀態）
    public List<CategoryExpenseDto> CategoryExpenses { get; set; } = [];   // 圓餅圖用
    public List<DailyTrendDto> DailyTrends { get; set; } = [];             // 趨勢圖用
}

public class CategoryExpenseDto
{
    public string CategoryName { get; set; } = "";
    public string CategoryColor { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }  // 佔比 (0-100)
}

public class DailyTrendDto
{
    public DateOnly Date { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
}
```

---

### 7. 預算管理（`/Budgets`）

#### GET `/Budgets`

**OnGetAsync()** — 載入所有預算設定 + 使用進度

#### POST `/Budgets?handler=Create`

**OnPostCreateAsync(BudgetInputModel input)** — 新增預算

**輸入模型** (`BudgetInputModel`):
```csharp
public class BudgetInputModel
{
    [Required(ErrorMessage = "請選擇分類")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "請輸入預算金額")]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "預算金額必須大於零")]
    public decimal Amount { get; set; }

    public BudgetPeriod Period { get; set; } = BudgetPeriod.Monthly;

    // StartDate 由系統自動設定為當月首日（若未指定）；
    // 使用者可選擇性指定起始月份
    public DateOnly? StartDate { get; set; }
}
```

#### POST `/Budgets?handler=Update`

**OnPostUpdateAsync(int id, BudgetInputModel input)** — 更新預算

#### POST `/Budgets?handler=Delete`

**OnPostDeleteAsync(int id)** — 刪除預算

---

### 8. 分類管理（`/Settings/Categories`）

#### GET `/Settings/Categories`

**OnGetAsync()** — 載入所有分類（收入 + 支出分開列示）

#### POST `/Settings/Categories?handler=Create`

**OnPostCreateAsync(CategoryInputModel input)** — 新增分類

**輸入模型** (`CategoryInputModel`):
```csharp
public class CategoryInputModel
{
    [Required(ErrorMessage = "請輸入分類名稱")]
    [MaxLength(50, ErrorMessage = "分類名稱最多 50 字")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "請選擇圖示")]
    [MaxLength(10)]
    public string Icon { get; set; } = "";

    [Required(ErrorMessage = "請選擇類型")]
    public TransactionType Type { get; set; }

    [MaxLength(7)]
    public string? Color { get; set; }
}
```

#### POST `/Settings/Categories?handler=Update`

**OnPostUpdateAsync(int id, CategoryInputModel input)** — 更新分類

#### POST `/Settings/Categories?handler=Delete`

**OnPostDeleteAsync(int id)** — 刪除分類（需檢查關聯紀錄 FR-009）

#### POST `/Settings/Categories?handler=DeleteAndMigrate`

**OnPostDeleteAndMigrateAsync(int id, int targetCategoryId)** — 刪除分類並遷移關聯紀錄

---

### 9. 帳戶管理（`/Settings/Accounts`）

#### GET `/Settings/Accounts`

**OnGetAsync()** — 載入所有帳戶（含即時餘額）

#### POST `/Settings/Accounts?handler=Create`

**OnPostCreateAsync(AccountInputModel input)** — 新增帳戶

**輸入模型** (`AccountInputModel`):
```csharp
public class AccountInputModel
{
    [Required(ErrorMessage = "請輸入帳戶名稱")]
    [MaxLength(50, ErrorMessage = "帳戶名稱最多 50 字")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "請選擇帳戶類型")]
    public AccountType Type { get; set; }

    [Required(ErrorMessage = "請選擇圖示")]
    [MaxLength(10)]
    public string Icon { get; set; } = "";

    [Range(0, double.MaxValue, ErrorMessage = "初始餘額不可為負數")]
    public decimal InitialBalance { get; set; }
}
```

#### POST `/Settings/Accounts?handler=Update`

**OnPostUpdateAsync(int id, AccountInputModel input)** — 更新帳戶

#### POST `/Settings/Accounts?handler=Delete`

**OnPostDeleteAsync(int id)** — 刪除帳戶（需檢查關聯交易）

---

### 10. CSV 匯出

#### GET `/Transactions?handler=Export&startDate={}&endDate={}`

**OnGetExportAsync(DateOnly? startDate, DateOnly? endDate)** — 匯出 CSV

**回應**: `FileContentResult`
- Content-Type: `text/csv; charset=utf-8`
- Content-Disposition: `attachment; filename="bookkeeping-export-{date}.csv"`
- CSV 標頭列: `日期,類型,金額,分類,帳戶,備註`
- 字元編碼: UTF-8 with BOM（確保 Excel 正確開啟）

---

### 11. CSV 匯入（`/Import`）

#### GET `/Import`

**OnGetAsync()** — 載入匯入頁面（含格式說明）

#### POST `/Import`

**OnPostAsync(IFormFile csvFile)** — 處理 CSV 匯入

**驗證**:
- 檔案大小 ≤ 5MB（FR-034）
- 副檔名為 `.csv`
- 內容行數 ≤ 10,000（FR-034）

**回應 ViewModel** (`ImportResultViewModel`):
```csharp
public class ImportResultViewModel
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<ImportError> Errors { get; set; } = [];
}

public class ImportError
{
    public int LineNumber { get; set; }
    public string ErrorMessage { get; set; } = "";
}
```

---

## AJAX Named Handlers（JSON API）

以下端點透過 AJAX 呼叫，回傳 JSON 資料，用於前端即時更新場景。

### 預算狀態檢查

```
GET /Budgets?handler=CheckStatus&categoryId={id}
```

使用時機：新增支出紀錄後，前端即時檢查預算狀態（FR-020）

**回應** (JSON):
```json
{
  "categoryName": "餐飲",
  "budgetAmount": 5000,
  "spentAmount": 4500,
  "usageRate": 90,
  "status": "warning",
  "message": "餐飲本月已使用 90%（$4,500 / $5,000）"
}
```

### 圖表資料端點

```
GET /Reports?handler=ChartData&year={year}&month={month}
```

使用時機：報表頁面使用 AJAX 載入圖表資料（避免同步渲染阻塞）

**回應** (JSON):
```json
{
  "categoryExpenses": [
    { "label": "餐飲", "value": 3500, "color": "#FF6384" },
    { "label": "交通", "value": 1200, "color": "#36A2EB" }
  ],
  "dailyTrends": [
    { "date": "2026-02-01", "income": 0, "expense": 150 },
    { "date": "2026-02-02", "income": 50000, "expense": 320 }
  ]
}
```

---

## 共用 DTO

```csharp
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public TransactionType Type { get; set; }
    public string? Color { get; set; }
}

public class AccountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public AccountType Type { get; set; }
    public decimal CurrentBalance { get; set; }
}
```

---

## 安全性契約

所有 POST handler 必須包含以下防護：

1. **Anti-Forgery Token**: 所有表單 POST 使用 `[ValidateAntiForgeryToken]`（Razor Pages 預設啟用）
2. **Model Validation**: 所有 Input Model 使用 Data Annotations 驗證，`ModelState.IsValid` 檢查
3. **HTML Sanitize**: CSV 匯入的文字欄位經 `HtmlSanitizer.Sanitize()` 處理
4. **檔案上傳限制**: CSV 匯入限制 5MB，僅接受 `.csv` 副檔名

---

## 回應狀態碼

| 狀態碼 | 使用場景 |
|--------|----------|
| 200 OK | GET 請求成功、AJAX JSON 回應 |
| 302 Found | POST 成功後 Redirect（PRG 模式） |
| 400 Bad Request | 驗證失敗（AJAX 端點） |
| 404 Not Found | 交易/分類/帳戶/預算不存在 |
| 500 Internal Server Error | 未預期錯誤（記錄至 Serilog） |

---

## Toast 通知契約

所有使用者操作回饋透過 `TempData` 傳遞 Toast 訊息：

```csharp
// PageModel 中設定
TempData["ToastMessage"] = "紀錄已成功儲存";
TempData["ToastType"] = "success";  // "success" | "warning" | "error"
```

前端 `_Toast.cshtml` Partial View 讀取 `TempData` 並渲染 Bootstrap Toast。
