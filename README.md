<!-- prettier-ignore -->
<div align="center">

# 📒 BookKeeping

**開源個人記帳理財工具**

[![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![SQLite](https://img.shields.io/badge/SQLite-003B57?style=flat-square&logo=sqlite&logoColor=white)](https://www.sqlite.org/)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)](LICENSE)

[功能特色](#功能特色) • [快速開始](#快速開始) • [專案結構](#專案結構) • [技術架構](#技術架構) • [開發指南](#開發指南)

</div>

BookKeeping 是一個自架式（self-hosted）的個人記帳工具，所有資料完整保留在你自己的伺服器上，不傳送至任何第三方服務。採用 ASP.NET Core Razor Pages 搭配 SQLite，輕量部署、零外部相依。

> [!NOTE]
> 本專案目前處於 MVP 階段（V1），聚焦於核心記帳功能。多幣別、帳戶間轉帳、使用者登入等進階功能將在後續版本中加入。

## 功能特色

- **收支紀錄管理** — 快速新增、編輯、刪除收入與支出紀錄，支援分類、帳戶、備註
- **分類系統** — 內建預設收支分類（餐飲、交通、娛樂等），並可自訂新增分類
- **帳戶追蹤** — 管理多個資金來源（現金、銀行、信用卡、電子支付），即時計算帳戶餘額
- **月度報表與圖表** — 月度收支摘要、分類佔比圓餅圖、每日收支趨勢圖（Chart.js）
- **預算管理** — 為支出分類設定月預算，自動追蹤使用率並在接近/超出預算時發出提醒
- **CSV 匯出匯入** — 將紀錄匯出為 CSV 備份，或從 CSV 批次匯入紀錄
- **搜尋與篩選** — 依日期範圍、分類、帳戶、關鍵字快速篩選紀錄
- **響應式設計** — Mobile-first，手機與桌面瀏覽器皆可流暢使用
- **隱私至上** — 所有資料僅存於自架伺服器的 SQLite 資料庫，完全由你掌控

## 快速開始

### 環境需求

- [.NET 10 SDK](https://dotnet.microsoft.com/download)（或更新版本）

### 安裝與執行

1. 複製專案：

   ```bash
   git clone https://github.com/HyperLee/BookKeeping.git
   cd BookKeeping
   ```

2. 還原相依套件並建構：

   ```bash
   dotnet build BookKeeping/BookKeeping.csproj
   ```

3. 執行應用程式：

   ```bash
   dotnet run --project BookKeeping/BookKeeping.csproj
   ```

4. 開啟瀏覽器前往 `http://localhost:5051`，即可開始使用。

> [!TIP]
> 首次啟動時，應用程式會自動執行資料庫遷移並寫入預設分類與帳戶資料，無需額外設定。

## 專案結構

```
BookKeeping/
├── BookKeeping/              # 主應用程式
│   ├── Data/                 # DbContext、Migrations、Seed
│   ├── Models/               # 領域模型（Transaction, Category, Account, Budget）
│   ├── Services/             # 業務邏輯服務層
│   ├── Validation/           # 驗證邏輯
│   ├── ViewModels/           # 頁面視圖模型
│   ├── Pages/                # Razor Pages（UI）
│   │   ├── Transactions/     #   交易紀錄 CRUD
│   │   ├── Reports/          #   月度報表與圖表
│   │   ├── Budgets/          #   預算管理
│   │   ├── Import/           #   CSV 匯入
│   │   └── Settings/         #   分類與帳戶設定
│   └── wwwroot/              # 靜態資源（CSS, JS, Chart.js）
├── BookKeeping.Tests/        # 單元測試與整合測試（xUnit）
└── specs/                    # 功能規格文件
```

## 技術架構

| 層級 | 技術 |
|------|------|
| Web 框架 | ASP.NET Core 10 Razor Pages |
| 資料庫 | SQLite（透過 Entity Framework Core） |
| 前端 | Bootstrap 5 + Chart.js 4 |
| 日誌 | Serilog（JSON 結構化日誌，依日期輪替） |
| 測試 | xUnit + Moq + Microsoft.AspNetCore.Mvc.Testing |
| 安全性 | CSP Nonce、CSRF Token、HtmlSanitizer、軟刪除 |

## 開發指南

### 執行測試

```bash
dotnet test BookKeeping.Tests/BookKeeping.Tests.csproj
```

### 資料庫遷移

新增遷移：

```bash
dotnet ef migrations add <MigrationName> --project BookKeeping/BookKeeping.csproj
```

應用遷移（通常由應用程式啟動時自動執行）：

```bash
dotnet ef database update --project BookKeeping/BookKeeping.csproj
```

### 前端函式庫管理

本專案使用 [LibMan](https://learn.microsoft.com/aspnet/core/client-side/libman/) 管理前端函式庫。還原函式庫：

```bash
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
libman restore
```
