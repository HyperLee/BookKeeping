開源個人記帳理財工具

應用類型
網頁應用程式（Web Application）

是否為混合型：是 — 主體為網頁應用程式，可搭配 CLI 工具做資料匯入匯出

工具名稱
開源個人記帳理財工具（Open BookKeeping）

工具類型
Web 應用程式

目標使用者
個人理財初學者：想要開始記帳但不想付費使用商業工具
注重隱私的使用者：不想將財務資料交給第三方雲端服務
開發者 / Vibe Coder：想透過協作開發一個實用的記帳工具，同時學習全端開發
使用情境：日常記錄收支、分類消費、查看月報、設定預算目標
要解決的問題
商業記帳 App 需要付費或有廣告：市面上好用的記帳 App（如麻布記帳 Moneybook）多為商業軟體，進階功能需要訂閱
隱私疑慮：將銀行帳戶連結第三方 App 讓部分使用者感到不安
資料被鎖死（Vendor Lock-in）：商業 App 的資料格式不開放，難以匯出或遷移
客製化困難：每個人的記帳習慣不同，商業 App 無法滿足所有人的需求
缺乏本地優先的免費方案：缺少一個資料完全在本機的開源記帳工具
使用流程
使用者開啟 Web App
新增收入/支出紀錄（金額、分類、日期、備註）
自動分類並計算收支統計
查看月報/週報視覺化圖表（圓餅圖、趨勢折線圖）
設定每月預算，追蹤花費進度
匯出資料為 CSV / JSON 備份
輸入與輸出
輸入：

格式：Web UI 手動輸入 / CSV 批次匯入
記錄欄位：日期、金額、類型（收入/支出）、分類、帳戶、備註、標籤
匯入格式：CSV（支援常見銀行對帳單格式）
範例：2026-02-18, -350, 餐飲, 現金, 午餐便當
輸出：

格式：Web Dashboard / CSV / JSON 匯出
輸出內容包含：
收支明細列表（可篩選、搜尋）
月報/週報統計圖表
分類消費佔比圓餅圖
預算使用進度
資料備份檔案（CSV / JSON）
錯誤訊息範例：提示：本月「餐飲」類別已超出預算 $2,000（預算 $5,000 / 已花 $7,000）
功能需求
Must（必須有，否則不算完成）：

新增/編輯/刪除收支紀錄（金額、日期、分類、備註）
預設收支分類（餐飲、交通、娛樂、購物、居住、醫療等，可自訂）
月度收支摘要（總收入、總支出、結餘）
基本視覺化圖表（分類佔比圓餅圖、每日/每月趨勢圖）
資料本地儲存（IndexedDB 優先，隱私優先）
CSV 匯出功能
Should（最好有，但可延後）：

多帳戶管理（現金、銀行、信用卡、電子支付）
預算設定與追蹤
CSV 匯入功能（支援銀行對帳單格式）
帳戶間轉帳紀錄
搜尋與進階篩選（日期區間、金額範圍、關鍵字）
PWA 支援（可安裝到手機桌面，離線使用）
Could（加分項）：

定期交易自動記錄（房租、訂閱費等）
多幣別支援
標籤系統（自訂標籤輔助分類）
深色模式
資料加密（本地加密儲存）
圖表匯出為圖片
年度財務報表
驗收標準

給定：使用者首次開啟 App，當：新增一筆 $150 的「餐飲」支出，則：紀錄正確儲存並顯示在明細列表中

給定：有 30 天的收支紀錄，當：查看月報，則：顯示正確的總收入/支出/結餘及分類圓餅圖

給定：使用者設定「餐飲」月預算 $5,000，當：餐飲支出累計超過 $5,000，則：顯示超出預算的提示

給定：使用者有 100 筆紀錄，當：點擊 CSV 匯出，則：下載的 CSV 包含所有紀錄且格式正確（可用 Excel 開啟）

給定：使用者關閉瀏覽器後重新開啟，當：進入 App，則：所有紀錄仍然存在（本地持久化）
範例與測試資料
操作範例（Web UI）：

[+新增紀錄]
  日期：2026/02/18
  類型：⊖ 支出
  金額：350
  分類：🍔 餐飲
  帳戶：現金
  備註：午餐便當
  [儲存]
月報範例：

📊 2026 年 2 月份摘要
─────────────────────
💰 總收入：$52,000
💸 總支出：$38,500
📈 結餘：  +$13,500

📋 支出分類 TOP 5：
  🍔 餐飲     $8,200  (21.3%)  ████████░░ 
  🏠 居住     $12,000 (31.2%)  ████████████░░
  🚗 交通     $3,500  (9.1%)   ████░░░░░░
  🛍️ 購物     $5,800  (15.1%)  ██████░░░░
  🎮 娛樂     $4,000  (10.4%)  █████░░░░░
CSV 匯出範例：

日期,類型,金額,分類,帳戶,備註
2026-02-18,支出,-350,餐飲,現金,午餐便當
2026-02-18,支出,-60,交通,悠遊卡,捷運
2026-02-17,收入,52000,薪資,銀行,2月薪水
限制與偏好
平台：
Web（跨平台）
語言/框架偏好：（不保證採用）

前端：純 HTML/CSS/JS（輕量）或 Angular / React（依專案慣例）
後端: .Net Core 10 c#14
圖表：Chart.js 或 D3.js
資料儲存：IndexedDB（客戶端本地儲存）
風格：適合 Vibe Coding 的輕快開發節奏
環境需求：

現代瀏覽器（Chrome、Firefox、Safari、Edge）
無需後端伺服器（純前端 + 本地儲存）
其他限制：


需要離線運作 — 完全離線，資料不上傳

需要儲存資料（持久化） — 使用 IndexedDB 本地儲存

需要帳號登入 — 不需要

其他：資料隱私為最高優先，所有資料僅存在使用者裝置上

1. 系統架構總覽
%%{init: {'theme': 'dark'}}%%
graph TB
    subgraph UI["🖥️ 前端 UI"]
        Dashboard["📊 Dashboard"]
        AddForm["➕ 新增紀錄"]
        Report["📈 報表頁面"]
        Settings["⚙️ 設定頁面"]
        Budget["💰 預算管理"]
    end

    subgraph Logic["⚙️ 商業邏輯層"]
        TxManager["💳 交易管理器"]
        CategoryMgr["🏷️ 分類管理器"]
        BudgetEngine["📊 預算引擎"]
        ReportEngine["📈 報表引擎"]
        ImportExport["📥📤 匯入匯出"]
    end

    subgraph Storage["💾 本地儲存層"]
        IndexedDB["🗃️ IndexedDB"]
        LocalStorage["📦 LocalStorage<br/>(設定)"]
    end

    subgraph External["📤 外部"]
        CSV_Out["📄 CSV 匯出"]
        CSV_In["📄 CSV 匯入"]
        PWA["📱 PWA 離線"]
    end

    Dashboard --> TxManager
    AddForm --> TxManager
    Report --> ReportEngine
    Budget --> BudgetEngine
    Settings --> CategoryMgr
    
    TxManager --> IndexedDB
    CategoryMgr --> IndexedDB
    BudgetEngine --> IndexedDB
    ReportEngine --> IndexedDB
    ImportExport --> CSV_Out
    ImportExport --> CSV_In
    Settings --> LocalStorage
    UI -.-> PWA


2. 使用者操作流程
%%{init: {'theme': 'dark'}}%%
flowchart TD
    Start(["🚀 開啟 App"]) --> FirstTime{"🆕 首次使用？"}
    
    FirstTime -->|是| Setup["⚙️ 初始設定<br/>• 幣別選擇<br/>• 預設帳戶建立<br/>• 分類客製化"]
    FirstTime -->|否| Home["🏠 首頁 Dashboard"]
    Setup --> Home
    
    Home --> Action{"選擇操作"}
    
    Action --> Add["➕ 新增紀錄"]
    Action --> View["📋 查看明細"]
    Action --> Report["📈 查看報表"]
    Action --> BudgetView["💰 預算追蹤"]
    Action --> Export["📤 匯出資料"]
    
    Add --> SelectType{"💱 類型"}
    SelectType -->|支出| Expense["💸 填寫支出<br/>金額、分類、帳戶、備註"]
    SelectType -->|收入| Income["💰 填寫收入<br/>金額、分類、帳戶、備註"]
    SelectType -->|轉帳| Transfer["🔄 帳戶轉帳<br/>從 → 到、金額"]
    
    Expense --> Save["💾 儲存"]
    Income --> Save
    Transfer --> Save
    Save --> CheckBudget{"⚠️ 超出預算？"}
    CheckBudget -->|是| Warn["🔔 顯示預算超出提示"]
    CheckBudget -->|否| Success["✅ 儲存成功"]
    Warn --> Success
    Success --> Home

    style Warn fill:#e74c3c,color:#fff
    style Success fill:#2ecc71,color:#fff
    style Home fill:#3498db,color:#fff


3. 資料模型（ER Diagram）

%%{init: {'theme': 'dark'}}%%
erDiagram
    TRANSACTION {
        string id PK "UUID"
        date date "交易日期"
        number amount "金額"
        string type "income | expense | transfer"
        string category_id FK "分類 ID"
        string account_id FK "帳戶 ID"
        string to_account_id FK "轉入帳戶(轉帳用)"
        string note "備註"
        string[] tags "標籤"
        datetime created_at "建立時間"
        datetime updated_at "更新時間"
    }
    
    CATEGORY {
        string id PK "UUID"
        string name "分類名稱"
        string icon "圖示 emoji"
        string type "income | expense"
        string color "顯示顏色"
        number sort_order "排序"
        boolean is_default "是否為預設"
    }
    
    ACCOUNT {
        string id PK "UUID"
        string name "帳戶名稱"
        string type "cash | bank | credit | e-payment"
        string icon "圖示"
        number initial_balance "初始餘額"
        string currency "幣別"
    }
    
    BUDGET {
        string id PK "UUID"
        string category_id FK "分類 ID"
        number amount "預算金額"
        string period "monthly | weekly"
        date start_date "起始日"
    }

    TRANSACTION }o--|| CATEGORY : "屬於分類"
    TRANSACTION }o--|| ACCOUNT : "使用帳戶"
    BUDGET }o--|| CATEGORY : "設定預算"


4. 記帳操作互動流程

%%{init: {'theme': 'dark'}}%%
sequenceDiagram
    actor User as 👤 使用者
    participant UI as 🖥️ 介面
    participant Logic as ⚙️ 邏輯層
    participant DB as 🗃️ IndexedDB
    participant Budget as 📊 預算引擎

    User->>UI: 點擊 ➕ 新增支出
    UI->>UI: 顯示新增表單
    User->>UI: 填寫：$350, 餐飲, 現金, 午餐
    User->>UI: 點擊 💾 儲存
    
    UI->>Logic: createTransaction(data)
    Logic->>Logic: 驗證資料完整性
    Logic->>DB: 儲存交易紀錄
    DB-->>Logic: ✅ 儲存成功
    
    Logic->>Budget: checkBudget("餐飲", 2月)
    Budget->>DB: 查詢本月餐飲總支出
    DB-->>Budget: 累計 $7,000
    Budget->>Budget: 比對預算 $5,000
    Budget-->>Logic: ⚠️ 超出 $2,000
    
    Logic-->>UI: 儲存成功 + 預算警告
    UI-->>User: ✅ 已記錄<br/>⚠️ 餐飲已超出預算 $2,000

    User->>UI: 切換到 📈 月報
    UI->>Logic: getMonthlyReport(2026, 2)
    Logic->>DB: 查詢 2 月所有紀錄
    DB-->>Logic: 交易紀錄列表
    Logic->>Logic: 計算統計、分類彙總
    Logic-->>UI: 報表資料
    UI-->>User: 📊 顯示圖表 + 摘要


5. 預算追蹤流程
%%{init: {'theme': 'dark'}}%%
flowchart TD
    NewTx["💳 新增一筆支出"] --> GetCategory["🏷️ 取得分類"]
    GetCategory --> HasBudget{"💰 該分類有<br/>設定預算？"}
    
    HasBudget -->|❌ 無| SaveOnly["💾 直接儲存"]
    HasBudget -->|✅ 有| CalcTotal["🧮 計算本月累計"]
    
    CalcTotal --> Compare{"📊 比較使用率"}
    
    Compare -->|"< 80%"| Safe["🟢 安全<br/>繼續花費"]
    Compare -->|"80% ~ 100%"| Warning["🟡 接近上限<br/>提醒節制"]
    Compare -->|"> 100%"| Over["🔴 超出預算<br/>強烈提醒"]
    
    Safe --> SaveOnly
    Warning --> NotifyWarn["🔔 顯示接近上限提示"]
    Over --> NotifyOver["🚨 顯示超出預算警告"]
    
    NotifyWarn --> SaveOnly
    NotifyOver --> SaveOnly
    SaveOnly --> UpdateDash["📊 更新 Dashboard"]

    style Safe fill:#27ae60,color:#fff
    style Warning fill:#f39c12,color:#fff
    style Over fill:#c0392b,color:#fff

6. 功能模組心智圖

%%{init: {'theme': 'dark'}}%%
mindmap
  root((💰 Open<br/>Moneybook))
    📝 紀錄管理
      ➕ 新增收支
      ✏️ 編輯紀錄
      🗑️ 刪除紀錄
      🔍 搜尋篩選
      🔄 帳戶轉帳
    🏷️ 分類系統
      📋 預設分類
      ✨ 自訂分類
      🎨 圖示與顏色
      📊 分類統計
    💰 預算管理
      🎯 設定月預算
      📊 使用進度
      🔔 超支提醒
      📈 趨勢分析
    📈 報表分析
      📊 月報 / 週報
      🥧 分類圓餅圖
      📉 趨勢折線圖
      💹 收支平衡表
    🏦 帳戶管理
      💵 現金
      🏦 銀行帳戶
      💳 信用卡
      📱 電子支付
    📥📤 資料管理
      📤 CSV 匯出
      📥 CSV 匯入
      💾 JSON 備份
      🔄 資料還原
    ⚙️ 設定
      🌙 深色模式
      💱 幣別設定
      📱 PWA 安裝
      🔒 資料加密




7. 頁面導覽結構

%%{init: {'theme': 'dark'}}%%
graph TD
    App["🏠 App"] --> Nav["📱 底部導覽列"]
    
    Nav --> Home["🏠 首頁<br/>Dashboard"]
    Nav --> Records["📋 明細<br/>交易列表"]
    Nav --> AddBtn["➕ 新增<br/>快速記帳"]
    Nav --> Charts["📈 報表<br/>圖表分析"]
    Nav --> More["⚙️ 更多<br/>設定"]
    
    Home --> Summary["💰 本月摘要"]
    Home --> Recent["📝 最近紀錄"]
    Home --> BudgetBar["📊 預算進度條"]
    Home --> QuickAdd["⚡ 快速記帳入口"]
    
    Records --> ListView["📋 列表檢視"]
    Records --> CalendarView["📅 日曆檢視"]
    Records --> SearchFilter["🔍 搜尋篩選"]
    
    Charts --> Monthly["📊 月報"]
    Charts --> Category["🥧 分類統計"]
    Charts --> Trend["📉 趨勢分析"]
    Charts --> Balance["💹 收支表"]
    
    More --> Accounts["🏦 帳戶管理"]
    More --> Categories["🏷️ 分類管理"]
    More --> Budgets["💰 預算設定"]
    More --> ImportExport["📥📤 匯入匯出"]
    More --> Theme["🌙 主題切換"]

    style AddBtn fill:#e74c3c,color:#fff,stroke:#fff,stroke-width:2px
    style Home fill:#3498db,color:#fff
    style Charts fill:#9b59b6,color:#fff


8. 技術架構與資料流

%%{init: {'theme': 'dark'}}%%
graph LR
    subgraph Browser["🌐 瀏覽器"]
        subgraph Frontend["🖥️ 前端"]
            HTML["📄 HTML"]
            CSS["🎨 CSS / Tailwind"]
            JS["⚙️ JavaScript"]
            ChartJS["📊 Chart.js"]
        end
        
        subgraph Storage["💾 儲存"]
            IDB["🗃️ IndexedDB<br/>交易、帳戶、分類"]
            LS["📦 LocalStorage<br/>設定、主題"]
        end
        
        subgraph PWA_Layer["📱 PWA"]
            SW["⚡ Service Worker"]
            Manifest["📋 Manifest"]
            Cache["💾 Cache API"]
        end
    end
    
    subgraph External["🌍 外部（選用）"]
        GHPages["🐙 GitHub Pages<br/>靜態部署"]
    end

    JS --> IDB
    JS --> LS
    JS --> ChartJS
    SW --> Cache
    Frontend --> PWA_Layer
    Frontend -.->|"部署"| GHPages

    style Browser fill:#1a1a2e,color:#fff
    style Storage fill:#16213e,color:#fff


9. 月報圓餅圖概念預覽

%%{init: {'theme': 'dark'}}%%
pie title 2026 年 2 月支出分類
    "🏠 居住 31.2%" : 31.2
    "🍔 餐飲 21.3%" : 21.3
    "🛍️ 購物 15.1%" : 15.1
    "🎮 娛樂 10.4%" : 10.4
    "🚗 交通 9.1%" : 9.1
    "🏥 醫療 5.2%" : 5.2
    "📚 教育 4.5%" : 4.5
    "🔧 其他 3.2%" : 3.2



1. V1 是否包含預算功能？
✅ 是，建議升級為 Must。
理由：驗收標準已包含「超出預算提示」的測試案例，且預算追蹤是記帳工具最核心的差異化功能之一（相比純記錄流水帳），不含預算的記帳工具價值會大打折扣。

2. CSV 匯入規格
V1 先支援「單一標準格式」即可。
建議標準格式：

日期,類型,金額,分類,帳戶,備註
2026-02-18,支出,350,餐飲,現金,午餐便當
日期格式：YYYY-MM-DD
類型：收入 / 支出
金額：正數（見第 3 題）
銀行對帳單匯入（欄位對映）留到 V2
3. 金額與類型的資料規則
建議：type = 收入/支出 + 金額皆為正數。
理由：

對一般使用者更直覺（填 350 而非 -350）
統計計算更單純（支出加總 = SUM where type=支出）
匯出時可選擇是否以負數表示支出（輸出層處理）
4. 確認前端 + 後端 + IndexedDB
✅ 是，確認。

資料儲存：IndexedDB（交易紀錄、帳戶、分類）+ LocalStorage（設定、主題偏好）
後端: .Net Core 10 c#14（提供 API 支援未來擴充，如帳戶轉帳邏輯、複雜報表計算等，V1 先實作核心功能，後端可視需求逐步開發）
隱私優先：零上傳，所有資料留在使用者裝置
5. V1 先排除多帳戶轉帳？
✅ 是，V1 先做單帳戶。

V1 保留「帳戶」欄位（讓使用者標記現金/銀行/信用卡），但不做帳戶間轉帳邏輯
轉帳功能留到 V2（需要處理雙邊記錄、帳戶餘額同步等複雜邏輯）


預算功能升級為 Must
資料規則：type=收入/支出、amount 一律正數
部署定位：純前端、無後端、IndexedDB + LocalStorage、GitHub Pages
V1 先單帳戶記錄，不做帳戶間轉帳
CSV 匯入先支援單一標準格式（欄位對映/銀行格式留到 V2）
外部參考專案僅作功能對照，不納入本案架構（本案維持零登入、零雲端）

驗收對照
新增 $150 餐飲支出後可持久化並顯示
30 天資料可正確計算月摘要與分類圖
預算 $5000 超支時顯示提醒
100 筆匯出 CSV 可被 Excel 開啟
關閉重開瀏覽器資料仍在
匯入標準 CSV 後資料完整映射
