# 實作計畫：Open BookKeeping — 開源個人記帳理財工具

**分支**: `001-open-bookkeeping-core` | **日期**: 2026-02-20 | **規格**: [spec.md](spec.md)
**輸入**: `/specs/001-open-bookkeeping-core/spec.md` 功能規格

## 摘要

建構一個開源個人記帳理財 Web 應用程式的 MVP 版本。採用 ASP.NET Core 10.0 Razor Pages 架構，以伺服器端渲染（SSR）為主，搭配 SQLite 資料庫（透過 Entity Framework Core）進行資料持久化。核心功能包含：收支紀錄 CRUD、分類與帳戶管理、月度摘要報表（含 Chart.js 圖表）、預算追蹤、CSV 匯出匯入、搜尋篩選。前端使用 Bootstrap 5 + jQuery 實現 Mobile-first 響應式設計，Serilog 提供結構化日誌。V1 為單人使用情境，不包含認證系統。

## 技術上下文

**語言/版本**: C# 14 / .NET 10.0  
**主要相依性**: ASP.NET Core 10.0（Razor Pages）、Bootstrap 5、jQuery 3.x、jQuery Validation、Chart.js 4.x、Entity Framework Core 10.0（SQLite provider）、Serilog、HtmlSanitizer  
**儲存**: SQLite（透過 Entity Framework Core + Microsoft.EntityFrameworkCore.Sqlite）  
**測試**: xUnit + Moq（單元測試）+ WebApplicationFactory（整合測試）  
**目標平台**: 桌面瀏覽器（Chrome、Edge、Firefox、Safari）+ 行動裝置瀏覽器（響應式）  
**專案類型**: Web（單一 ASP.NET Core Razor Pages 專案 + 測試專案）  
**效能目標**: FCP < 1.5 秒、LCP < 2.5 秒；10,000 筆紀錄查詢 < 3 秒；100 筆月報產生 < 2 秒  
**限制條件**: CSV 匯入 < 5MB / 10,000 筆上限；金額一律使用 `decimal` 型別；V1 單一幣別  
**規模/範圍**: 單人使用、約 8 個頁面、預估年累積 600-2,400 筆紀錄、極端情境 10,000+ 筆

## 憲章檢查

*閘門：Phase 0 研究前必須通過。Phase 1 設計後重新檢查。*

### Pre-Phase 0 檢查

| # | 原則 | 狀態 | 說明 |
|---|------|------|------|
| I | 程式碼品質至上 (NON-NEGOTIABLE) | ✅ 通過 | 採用 C# 14、檔案範圍命名空間、Nullable Reference Types、XML 文件註解、.editorconfig 格式化 |
| II | 測試優先開發 (NON-NEGOTIABLE) | ✅ 通過 | 採用 xUnit + Moq + WebApplicationFactory；金額計算一律使用 `decimal`；每個 User Story 獨立可測試 |
| III | 使用者體驗一致性 | ✅ 通過 | Bootstrap 5 統一設計語言；jQuery Validation 即時驗證；Toast 通知機制；Mobile-first 響應式設計 |
| IV | 效能與延展性 | ✅ 通過 | FCP < 1.5s / LCP < 2.5s 目標；async/await I/O；靜態資源壓縮；Chart.js 輕量圖表 |
| V | 可觀察性與監控 | ✅ 通過 | Serilog 結構化 JSON 日誌；檔案輪替 30 天；關鍵業務操作 Information 級別記錄 |
| VI | 安全優先 | ✅ 通過 | Razor 引擎 HTML 編碼；Anti-Forgery Token；HtmlSanitizer 處理 CSV 匯入文字；HTTPS + HSTS |
| VII | 資料完整性 (NON-NEGOTIABLE) | ✅ 通過 | `decimal` 金額；EF Core 交易原子性；EF Core Migrations 版本化遷移；軟刪除策略 |

**閘門結果**: ✅ 全部通過，無違規項目。

## 專案結構

### 文件（本功能）

```text
specs/001-open-bookkeeping-core/
├── plan.md              # 本文件（/speckit.plan 輸出）
├── research.md          # Phase 0 輸出（/speckit.plan）
├── data-model.md        # Phase 1 輸出（/speckit.plan）
├── quickstart.md        # Phase 1 輸出（/speckit.plan）
├── contracts/           # Phase 1 輸出（/speckit.plan）
│   └── api-endpoints.md # Razor Pages 端點與 AJAX API 定義
└── tasks.md             # Phase 2 輸出（/speckit.tasks — 非本命令建立）
```

### 原始碼（Repository 根目錄）

```text
BookKeeping/                          # ASP.NET Core Razor Pages 主專案
├── Data/
│   ├── BookKeepingDbContext.cs        # EF Core DbContext
│   ├── Migrations/                   # EF Core 自動產生的遷移檔
│   └── Seed/
│       └── DefaultDataSeeder.cs      # 預設分類、帳戶種子資料
├── Models/
│   ├── Transaction.cs                # 交易紀錄實體
│   ├── Category.cs                   # 分類實體
│   ├── Account.cs                    # 帳戶實體
│   └── Budget.cs                     # 預算實體
├── Services/
│   ├── ITransactionService.cs        # 交易服務介面
│   ├── TransactionService.cs
│   ├── ICategoryService.cs           # 分類服務介面
│   ├── CategoryService.cs
│   ├── IAccountService.cs            # 帳戶服務介面
│   ├── AccountService.cs
│   ├── IBudgetService.cs             # 預算服務介面
│   ├── BudgetService.cs
│   ├── ICsvService.cs                # CSV 匯出匯入服務介面
│   ├── CsvService.cs
│   ├── IReportService.cs             # 報表服務介面
│   └── ReportService.cs
├── Pages/
│   ├── Index.cshtml                  # Dashboard（月摘要/帳戶餘額/預算進度/最近紀錄）
│   ├── Index.cshtml.cs
│   ├── Transactions/
│   │   ├── Index.cshtml              # 明細列表（搜尋/篩選）
│   │   ├── Create.cshtml             # 新增紀錄
│   │   └── Edit.cshtml               # 編輯紀錄
│   ├── Reports/
│   │   └── Index.cshtml              # 月度報表（圓餅圖 + 趨勢圖）
│   ├── Budgets/
│   │   └── Index.cshtml              # 預算管理
│   ├── Settings/
│   │   ├── Categories.cshtml         # 分類管理
│   │   └── Accounts.cshtml           # 帳戶管理
│   ├── Import/
│   │   └── Index.cshtml              # CSV 匯入
│   └── Shared/
│       ├── _Layout.cshtml            # 主佈局（含導覽列）
│       ├── _Layout.cshtml.css        # 佈局 CSS Isolation
│       ├── _Toast.cshtml             # Toast 通知 Partial View
│       └── _ValidationScriptsPartial.cshtml
├── ViewModels/
│   ├── DashboardViewModel.cs         # Dashboard 頁面 ViewModel
│   ├── TransactionViewModel.cs       # 交易紀錄 ViewModel
│   ├── MonthlyReportViewModel.cs     # 月報 ViewModel
│   └── BudgetProgressViewModel.cs    # 預算進度 ViewModel
├── Validation/
│   └── TransactionValidator.cs       # 交易紀錄驗證邏輯
├── wwwroot/
│   ├── css/site.css                  # 全域自訂樣式
│   ├── js/
│   │   ├── site.js                   # 全域 JavaScript
│   │   ├── toast.js                  # Toast 通知邏輯
│   │   └── charts.js                 # Chart.js 圖表初始化
│   └── lib/
│       ├── bootstrap/
│       ├── jquery/
│       ├── jquery-validation/
│       ├── jquery-validation-unobtrusive/
│       └── chart.js/                 # Chart.js 函式庫
├── Program.cs                        # 應用程式進入點與 DI 配置
├── appsettings.json
├── appsettings.Development.json
└── BookKeeping.csproj

BookKeeping.Tests/                    # 測試專案
├── Unit/
│   ├── Services/
│   │   ├── TransactionServiceTests.cs
│   │   ├── CategoryServiceTests.cs
│   │   ├── AccountServiceTests.cs
│   │   ├── BudgetServiceTests.cs
│   │   ├── CsvServiceTests.cs
│   │   └── ReportServiceTests.cs
│   └── Models/
│       └── TransactionValidationTests.cs
├── Integration/
│   ├── Pages/
│   │   ├── DashboardPageTests.cs
│   │   ├── TransactionPagesTests.cs
│   │   └── ReportPagesTests.cs
│   └── Data/
│       ├── DbContextTests.cs
│       └── SeedDataTests.cs
├── Helpers/
│   └── TestWebApplicationFactory.cs  # 自訂 WebApplicationFactory
└── BookKeeping.Tests.csproj
```

**結構決策**: 採用 ASP.NET Core Razor Pages 單一專案架構（非前後端分離）。這與現有專案結構一致，並符合規格書的「單一 Razor Pages 專案」需求。額外建立一個獨立的 xUnit 測試專案。程式碼按關注點分層為 Models、Data、Services、ViewModels、Validation、Pages。

## 複雜度追蹤

> 無憲章違規項目，不需填寫。

---

## Post-Phase 1 憲章覆核

*Phase 1 設計完成後的第二次閘門檢查。*

| # | 原則 | 狀態 | 設計驗證 |
|---|------|------|----------|
| I | 程式碼品質至上 | ✅ 通過 | 明確的分層架構（Models/Services/ViewModels/Pages）；介面定義清晰（`ITransactionService` 等）；所有實體有 XML 文件註解需求 |
| II | 測試優先開發 | ✅ 通過 | 測試專案結構包含 Unit + Integration 目錄；Services 透過介面可 Mock；TestWebApplicationFactory 支援整合測試  |
| III | 使用者體驗一致性 | ✅ 通過 | 統一 Toast 通知機制（`_Toast.cshtml`）；Bootstrap 5 一致設計語言；jQuery Validation 即時回饋；Mobile-first 響應式佈局 |
| IV | 效能與延展性 | ✅ 通過 | Chart.js 輕量（70KB）；資料庫索引策略已定義（4 個複合索引）；AJAX 圖表資料載入避免阻塞；分頁查詢（PageSize=20） |
| V | 可觀察性與監控 | ✅ 通過 | Serilog JSON 日誌配置完成（research.md R-004）；日期輪替 30 天；三級日誌等級（Info/Warning/Error） |
| VI | 安全優先 | ✅ 通過 | Anti-Forgery Token（Razor Pages 預設）；HtmlSanitizer 處理 CSV 匯入；CSV 5MB 上限；Razor 引擎 HTML 編碼 |
| VII | 資料完整性 | ✅ 通過 | 所有金額使用 `decimal`（data-model.md）；Foreign Key Restrict 防止串聯刪除；軟刪除 + Global Query Filter；EF Core Migrations 版本化遷移 |

**Post-Phase 1 閘門結果**: ✅ 全部通過，設計方案與憲章七大原則完全對齊。
