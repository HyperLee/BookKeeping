# 研究文件：Open BookKeeping — 開源個人記帳理財工具

**分支**: `001-open-bookkeeping-core` | **日期**: 2026-02-20

## 研究摘要

本文件記錄 Phase 0 研究階段解決的所有技術決策與未知項目。每項決策包含選項評估、最終決定與理由。

---

## 研究項目

### R-001：圖表函式庫選擇 — Chart.js vs D3.js

**決定**: 採用 **Chart.js 4.x**

**理由**:
- Chart.js 提供開箱即用的圓餅圖（Pie/Doughnut）和折線圖（Line），完全覆蓋規格需求（FR-013 分類圓餅圖、FR-014 趨勢圖）
- Chart.js 壓縮後約 70KB（含所有圖表類型），D3.js 壓縮後約 90KB 且需額外撰寫大量渲染程式碼
- Chart.js 內建響應式設計（`responsive: true`），自動適配容器大小，符合 Mobile-first 需求（FR-042、FR-043）
- Chart.js 內建觸控互動支援（tooltip、hover），無需額外實作
- 學習曲線低，配置式 API 適合本專案規模，不需要 D3.js 的自訂視覺化能力

**排除的替代方案**:
- **D3.js**: 過度強大，適合需要高度自訂視覺化的場景。本專案僅需標準圖表，D3 的底層 API 會增加大量開發成本
- **ApexCharts**: 功能完整但套件較大（約 130KB），且社群生態不如 Chart.js 活躍
- **ECharts**: 功能強大但偏向企業級用途，bundle size 較大（約 400KB+），不適合輕量 MVP

---

### R-002：SQLite + Entity Framework Core 整合最佳實踐

**決定**: 使用 `Microsoft.EntityFrameworkCore.Sqlite` NuGet 套件 + Code-First 遷移

**理由**:
- EF Core 10.0 對 SQLite 的支援成熟穩定，是 .NET 生態中 SQLite 的首選 ORM
- Code-First 遷移符合憲章要求（FR-033）：`dotnet ef migrations add` / `dotnet ef database update`
- SQLite 為單檔案資料庫，簡化部署（無需額外資料庫伺服器），適合個人記帳場景

**最佳實踐重點**:
1. **連線字串**: `Data Source=bookkeeping.db` 存放於 `appsettings.json`，不同環境使用不同檔案路徑
2. **WAL 模式**: 啟用 Write-Ahead Logging（`PRAGMA journal_mode=WAL`）提升並行讀取效能
3. **連線池**: SQLite 預設為單連線，EF Core 自動管理。使用 `AddDbContext` 時設定 `ServiceLifetime.Scoped`
4. **decimal 精確度**: SQLite 不原生支援 `decimal`，需在 `OnModelCreating` 中明確設定 `HasColumnType("TEXT")` 並以 `decimal` 型別存取（EF Core 自動轉換）
5. **遷移策略**: 開發時使用 `EnsureCreated()` 快速建庫；正式環境使用 `Migrate()` 執行遷移腳本
6. **備份**: SQLite 單檔案特性支援簡單的檔案複製備份

**排除的替代方案**:
- **Dapper**: 輕量 micro-ORM，但缺乏遷移管理、變更追蹤等功能，長期維護性不如 EF Core
- **LiteDB**: NoSQL 文件資料庫，不適合需要關聯查詢的記帳場景

---

### R-003：CSV 解析策略

**決定**: 使用自行實作的輕量 CSV 解析器（搭配 `StreamReader`）

**理由**:
- 規格定義的 CSV 格式為固定六欄位（日期、類型、金額、分類、帳戶、備註），結構簡單穩定
- 自行實作可完全掌控驗證邏輯（FR-025 逐行驗證 + 行號錯誤回報）、檔案大小限制（FR-034）和 HTML sanitize（FR-035）
- 減少外部相依性，符合專案輕量化原則

**實作要點**:
1. 使用 `StreamReader` 逐行讀取，支援 UTF-8 編碼（含 BOM）
2. 遵循 RFC 4180：引號包覆欄位、引號內逗號不分割、雙引號轉義
3. 匯出使用 `StreamWriter`，對含逗號/引號/換行的欄位自動加引號
4. 檔案大小先行檢查（< 5MB），行數計數（< 10,000 筆）
5. 所有文字欄位經 HtmlSanitizer 處理後再存入資料庫

**排除的替代方案**:
- **CsvHelper**: 功能完整的 CSV 函式庫，但對本專案的簡單固定格式而言過度工程化，且增加額外相依性
- **TextFieldParser**: .NET 內建但功能有限，不支援 CSV RFC 4180 完整規範

---

### R-004：Serilog 結構化日誌配置

**決定**: 使用 Serilog + Serilog.Sinks.File + Serilog.Formatting.Compact

**理由**:
- 憲章（原則 V）與規格（FR-039、FR-040、FR-041）明確要求 Serilog 結構化日誌
- Serilog.Formatting.Compact 產生 JSON 格式日誌，便於未來整合 ELK、Seq 等監控平台
- Serilog.Sinks.File 支援日期輪替（`rollingInterval: RollingInterval.Day`）與保留天數（`retainedFileCountLimit: 30`）

**配置方案**:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    },
    "WriteTo": [{
      "Name": "File",
      "Args": {
        "path": "logs/bookkeeping-.json",
        "rollingInterval": "Day",
        "retainedFileCountLimit": 30,
        "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
      }
    }]
  }
}
```

**需安裝的 NuGet 套件**:
- `Serilog.AspNetCore`
- `Serilog.Formatting.Compact`

**金融資料防護**: 日誌中不記錄金額明細，僅記錄操作類型、交易 ID 和使用者操作（符合憲章原則 VI — 金融資料防護）

---

### R-005：HTML Sanitize 方案

**決定**: 使用 **Ganss.Xss.HtmlSanitizer** NuGet 套件

**理由**:
- 規格 FR-035 要求對 CSV 匯入的所有文字欄位執行 HTML sanitize
- HtmlSanitizer 是 .NET 生態中最成熟的 XSS 防護函式庫
- 白名單機制（而非黑名單），預設移除所有 HTML 標籤，僅保留安全內容
- 積極維護且相容 .NET 10.0

**使用場景**:
1. CSV 匯入時：對備註、分類名稱、帳戶名稱欄位 sanitize
2. 使用者手動輸入時：Razor 引擎已提供內建 HTML 編碼，但作為額外防護層也對備註欄位 sanitize

**排除的替代方案**:
- **手動正則過濾**: 容易遺漏邊界情況，不如專業函式庫可靠
- **AntiXSS Library**: 微軟已停止維護，不建議用於新專案

---

### R-006：軟刪除策略

**決定**: 使用 EF Core Global Query Filter 實作軟刪除

**理由**:
- 憲章原則 VII 建議帳目刪除採用軟刪除策略，保留歷史記錄可追溯
- EF Core Global Query Filter 可在 `OnModelCreating` 中統一配置，所有查詢自動過濾已刪除記錄
- 需要存取已刪除記錄時可使用 `IgnoreQueryFilters()` 繞過

**實作方案**:
1. 所有實體繼承 `ISoftDeletable` 介面，包含 `IsDeleted` 和 `DeletedAt` 屬性
2. 在 `BookKeepingDbContext.OnModelCreating` 中配置 Global Query Filter：`.HasQueryFilter(e => !e.IsDeleted)`
3. 覆寫 `SaveChangesAsync` 攔截 `Delete` 操作，自動轉為設定 `IsDeleted = true` + `DeletedAt = DateTime.UtcNow`
4. 硬刪除僅在資料庫維護腳本中使用，應用層不暴露硬刪除功能

**排除的替代方案**:
- **手動 WHERE 條件過濾**: 容易遺漏，每個查詢都需記得加 `!IsDeleted` 條件
- **獨立歷史表**: 增加複雜度，V1 不需要完整的稽核軌跡表（V2 可考慮）

---

### R-007：前端靜態資源管理策略

**決定**: 使用 LibMan（Library Manager）管理前端相依性

**理由**:
- ASP.NET Core 專案慣例使用 LibMan 管理 `wwwroot/lib/` 下的前端函式庫
- 現有專案結構已在 `wwwroot/lib/` 下包含 Bootstrap、jQuery、jQuery Validation
- LibMan 輕量、無需 Node.js 環境、與 Visual Studio / VS Code 整合良好

**新增函式庫清單**:
| 函式庫 | 版本 | 用途 |
|--------|------|------|
| Chart.js | 4.x | 圓餅圖 + 趨勢圖（FR-013、FR-014） |

**靜態資源管線**:
- 使用 `MapStaticAssets()` + `WithStaticAssets()` 配置靜態檔案管線（ASP.NET Core 10.0 新 API）
- CSS/JS 壓縮由部署管線處理（可選），開發時使用未壓縮版本

---

### R-008：帳戶餘額計算策略

**決定**: 使用即時查詢計算（Computed Balance），不儲存餘額欄位

**理由**:
- 規格 FR-036 定義帳戶餘額 = 初始餘額 + 該帳戶收入總和 - 該帳戶支出總和
- V1 資料量小（預估年累積 600-2,400 筆），即時聚合查詢效能充足
- 避免儲存餘額欄位導致的資料不一致風險（若忘記更新 cached balance）
- EF Core LINQ 可在資料庫層面執行 SUM 聚合，單次查詢即可取得餘額

**實作方式**:
```csharp
// AccountService.cs
decimal balance = account.InitialBalance
    + await dbContext.Transactions
        .Where(t => t.AccountId == accountId && t.Type == TransactionType.Income)
        .SumAsync(t => t.Amount)
    - await dbContext.Transactions
        .Where(t => t.AccountId == accountId && t.Type == TransactionType.Expense)
        .SumAsync(t => t.Amount);
```

**規模考量**: 當紀錄超過 10,000 筆時，可在 `Transactions` 表上為 `AccountId` + `Type` 建立複合索引以加速查詢。若未來效能不足，可改為 CQRS 模式（維護 materialized balance）。

---

### R-009：日期與時區處理

**決定**: 內部統一使用 `DateOnly`（C# 10+）儲存日期，`DateTime` UTC 儲存時間戳

**理由**:
- 規格定義日期格式為 ISO 8601 (YYYY-MM-DD)，`DateOnly` 完全匹配
- 交易紀錄的「日期」是純日期概念（不含時間），使用 `DateOnly` 語意更精確
- `CreatedAt` / `UpdatedAt` 等時間戳使用 `DateTime` UTC 儲存
- EF Core 10.0 + SQLite 支援 `DateOnly` 映射

**SQLite 相容性**:
- `DateOnly` 在 SQLite 中以 TEXT 格式儲存（`YYYY-MM-DD`）
- `DateTime` 在 SQLite 中以 TEXT 格式儲存（ISO 8601）
- 查詢時 EF Core 自動處理型別轉換

---

### R-010：防止重複提交策略

**決定**: 前端 + 後端雙重防護

**理由**:
- 規格邊界條件要求：「使用者在同一秒內快速連按兩次儲存時，應防止重複建立紀錄」

**實作方案**:
1. **前端**: 點擊「儲存」按鈕後立即 `disabled`，待 AJAX 回應後重新啟用（或頁面重新載入）
2. **後端**: 在 `TransactionService.CreateAsync` 中加入冪等性檢查——若 5 秒內存在相同日期 + 金額 + 分類 + 帳戶的紀錄，返回警告提示而非直接建立

---

## NEEDS CLARIFICATION 解決狀態

| 項目 | 原始狀態 | 解決方案 | 參考 |
|------|----------|----------|------|
| 圖表庫選擇 | 待決定 | Chart.js 4.x | R-001 |
| SQLite EF Core 整合 | 待研究 | Microsoft.EntityFrameworkCore.Sqlite + Code-First Migrations | R-002 |
| CSV 解析方式 | 待決定 | 自行實作輕量解析器 | R-003 |
| 日誌系統配置 | 待研究 | Serilog + Compact JSON Formatter + 日期輪替 | R-004 |
| HTML Sanitize 方案 | 待決定 | Ganss.Xss.HtmlSanitizer | R-005 |
| 軟刪除策略 | 功能建議 | EF Core Global Query Filter | R-006 |
| 前端套件管理 | 待決定 | LibMan | R-007 |
| 帳戶餘額計算 | 待研究 | 即時查詢計算（不快取） | R-008 |
| 日期處理 | 待研究 | DateOnly + DateTime UTC | R-009 |
| 防重複提交 | 待研究 | 前端 disabled + 後端冪等性檢查 | R-010 |

**狀態**: ✅ 所有 NEEDS CLARIFICATION 項目已解決
