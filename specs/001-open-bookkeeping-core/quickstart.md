# 快速入門指南：Open BookKeeping — 開源個人記帳理財工具

**分支**: `001-open-bookkeeping-core` | **日期**: 2026-02-20

## 前置需求

| 工具 | 版本 | 安裝方式 |
|------|------|----------|
| .NET SDK | 10.0+ | [https://dot.net/download](https://dot.net/download) |
| Git | 2.x+ | `brew install git`（macOS）或 `apt install git`（Linux） |
| VS Code（建議） | Latest | [https://code.visualstudio.com](https://code.visualstudio.com) |

**不需要額外安裝**：SQLite 內嵌於 EF Core SQLite provider，無需獨立安裝資料庫伺服器。

---

## 快速啟動

### 1. 複製專案

```bash
git clone https://github.com/HyperLee/BookKeeping.git
cd BookKeeping
```

### 2. 切換至功能分支

```bash
git checkout 001-open-bookkeeping-core
```

### 3. 還原相依套件

```bash
dotnet restore
```

### 4. 建構專案

```bash
dotnet build
```

### 5. 執行 EF Core 資料庫遷移

```bash
# 安裝 EF Core CLI（首次需要）
dotnet tool install --global dotnet-ef

# 執行遷移，建立 SQLite 資料庫
cd BookKeeping
dotnet ef database update
```

執行後會在 `BookKeeping/` 目錄下產生 `bookkeeping.db` 檔案，並自動填入預設分類與帳戶種子資料。

### 6. 啟動應用程式

```bash
dotnet run --project BookKeeping
```

應用程式會啟動在 `https://localhost:5001` 和 `http://localhost:5000`。

### 7. 開啟瀏覽器

前往 [https://localhost:5001](https://localhost:5001) 開始使用。

---

## 執行測試

### 全部測試

```bash
dotnet test
```

### 僅單元測試

```bash
dotnet test --filter "Category=Unit"
```

### 僅整合測試

```bash
dotnet test --filter "Category=Integration"
```

### 含覆蓋率報告

```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 專案結構概覽

```
BookKeeping/                  # ASP.NET Core Razor Pages 主專案
├── Data/                     # EF Core DbContext + 遷移
├── Models/                   # 領域實體（Transaction、Category、Account、Budget）
├── Services/                 # 業務邏輯服務層
├── ViewModels/               # 頁面 ViewModel / DTO
├── Validation/               # 驗證邏輯
├── Pages/                    # Razor Pages（UI）
│   ├── Index.cshtml          # Dashboard
│   ├── Transactions/         # 收支紀錄 CRUD
│   ├── Reports/              # 月度報表 + 圖表
│   ├── Budgets/              # 預算管理
│   ├── Settings/             # 分類管理 + 帳戶管理
│   ├── Import/               # CSV 匯入
│   └── Shared/               # 共用佈局 + Partial Views
├── wwwroot/                  # 靜態資源（CSS、JS、第三方函式庫）
├── Program.cs                # 應用進入點 + DI 配置
└── appsettings.json          # 應用設定

BookKeeping.Tests/            # 測試專案
├── Unit/                     # 單元測試（Services、Models）
├── Integration/              # 整合測試（Pages、Data）
└── Helpers/                  # 測試輔助工具
```

---

## 關鍵配置說明

### appsettings.json

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=bookkeeping.db"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "WriteTo": [{
      "Name": "File",
      "Args": {
        "path": "logs/bookkeeping-.json",
        "rollingInterval": "Day",
        "retainedFileCountLimit": 30
      }
    }]
  }
}
```

### 開發環境設定（appsettings.Development.json）

```jsonc
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

---

## 常見操作

### 新增 EF Core 遷移

```bash
cd BookKeeping
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

### 重設資料庫

```bash
cd BookKeeping
rm bookkeeping.db
dotnet ef database update
```

### 新增前端函式庫（LibMan）

```bash
dotnet tool install --global Microsoft.Web.LibraryManager.Cli
cd BookKeeping
libman install chart.js@4 --provider cdnjs --destination wwwroot/lib/chart.js
```

---

## NuGet 套件清單

### 主專案（BookKeeping.csproj）

| 套件 | 用途 |
|------|------|
| `Microsoft.EntityFrameworkCore.Sqlite` | SQLite 資料庫 provider |
| `Microsoft.EntityFrameworkCore.Design` | EF Core CLI 遷移工具 |
| `Serilog.AspNetCore` | 結構化日誌 |
| `Serilog.Formatting.Compact` | JSON 日誌格式化 |
| `HtmlSanitizer` | XSS 防護（CSV 匯入 sanitize） |

### 測試專案（BookKeeping.Tests.csproj）

| 套件 | 用途 |
|------|------|
| `Microsoft.AspNetCore.Mvc.Testing` | WebApplicationFactory 整合測試 |
| `xunit` | 測試框架 |
| `xunit.runner.visualstudio` | VS / VS Code 測試執行器 |
| `Moq` | Mock 框架 |
| `Microsoft.EntityFrameworkCore.InMemory` | 測試用 InMemory 資料庫 |
| `coverlet.collector` | 程式碼覆蓋率收集 |

---

## 疑難排解

| 問題 | 解決方式 |
|------|----------|
| `dotnet ef` 找不到命令 | 執行 `dotnet tool install --global dotnet-ef` |
| HTTPS 憑證警告 | 執行 `dotnet dev-certs https --trust` |
| SQLite 資料庫鎖定 | 確認沒有其他程式正在存取 `bookkeeping.db` |
| 前端函式庫缺失 | 執行 `libman restore`（需先安裝 LibMan CLI） |
| 測試執行失敗 | 確認已執行 `dotnet restore` 並且 SDK 版本為 10.0+ |

---

## Phase 11 驗證紀錄（2026-02-21）

### T069 快速啟動與基本驗證

- `dotnet build BookKeeping.sln`：成功（僅 NU1902 警告：HtmlSanitizer 8.1.870）
- `dotnet test BookKeeping.sln --no-build`：成功（62/62 通過）
- `dotnet ef database update --project BookKeeping`：成功
- 啟動服務：`dotnet run --project BookKeeping --no-build --no-launch-profile --urls http://127.0.0.1:5070`
- 種子資料驗證：`Categories=12`、`Accounts=3`

| 頁面 | 狀態碼 |
|------|--------|
| `/` | 200 |
| `/Transactions` | 200 |
| `/Transactions/Create` | 200 |
| `/Reports` | 200 |
| `/Budgets` | 200 |
| `/Settings/Categories` | 200 |
| `/Settings/Accounts` | 200 |
| `/Import` | 200 |
| `/Privacy` | 200 |

### T069 功能流程抽查

- 交易 CRUD：Create/Update/Delete POST 皆回傳 `302`，刪除後資料列 `IsDeleted=1`
- CSV 匯出：`/Transactions?handler=Export` 回傳 200，UTF-8 BOM 存在，標題列正確
- CSV 匯入：`/Import` 回傳 200，測試列成功寫入（`imported_rows=1`）
- Chart.js 資料：`/Reports?handler=ChartData&year=2026&month=2` 回傳 200，`categoryExpenses` 與 `dailyTrends` 皆有資料

### T086 效能驗證（本機基準）

| 指標 | 實測 | 目標 | 結果 |
|------|------|------|------|
| SC-001 新增交易 UX flow | 0.042s | < 30s | ✅ |
| SC-002 100 筆月報表載入（頁面+圖表資料） | 0.003s | < 2s | ✅ |
| SC-003 1,000 筆 CSV 匯出 | 0.009s | < 5s | ✅ |
| SC-007 10,000 筆列表篩選首屏回應 | 0.011s | 維持流暢 | ✅ |

> 說明：SC-007 以伺服器端首屏回應時間作為 CLI 可量測代理指標。

### T087 隱私與相依套件自評清單

- [x] 檢視主專案與測試專案 NuGet 套件清單（含 transitive）
- [x] 程式碼掃描未發現 `HttpClient`/`WebClient` 等主動外呼實作
- [x] 前端 `fetch` 僅呼叫站內相對路徑（報表資料、預算狀態）
- [x] 執行中應用（PID 監看）無對外已建立 TCP 連線
- [x] 例外與請求日誌含 RequestPath/TraceId，無敏感金額明文輸出（採遮罩）
- [ ] 已知相依套件風險追蹤：NU1902（HtmlSanitizer 8.1.870）

### T088 UX walkthrough 摘要

- 首次新增交易流程可完成，欄位標籤完整（日期/金額/類型/分類/帳戶/備註）
- 成功提交後可看到 Toast alert（成功訊息 + 可關閉按鈕）
- 行動版底部導覽存在，頁面具語義化導覽 landmark 與 `aria-current`
- 空狀態文案已覆蓋 Dashboard/交易清單/報表/預算/設定頁
