# DocFlow - 程式碼品質與標準文件

本文件概述 DocFlow 專案的程式碼品質、測試、使用者體驗一致性和效能要求的全面標準。

## 目錄
- [程式碼品質標準](#程式碼品質標準)
- [測試標準](#測試標準)
- [使用者體驗一致性](#使用者體驗一致性)
- [效能要求](#效能要求)

---

## 程式碼品質標準

### 1. 架構原則

#### Clean Architecture (整潔架構)
DocFlow 遵循整潔架構原則，以確保可維護性、可擴展性和關注點分離。

**層級結構：**
- **領域層** (`DocFlow.Domain`)：核心業務邏輯、實體、值物件、領域事件
  - 不依賴其他層級
  - 包含聚合、實體、值物件和領域事件
  - 自訂儲存庫介面（如有需要）

- **領域共享層** (`DocFlow.Domain.Shared`)：共享領域類型
  - 列舉和共享常數
  - 位於 `DocFlow.Domain.Shared/Enums` 資料夾

- **應用層** (`DocFlow.Application`)：用例、命令、查詢
  - 僅依賴領域層
  - 應用服務必須以 `ApplicationService` 結尾
  - 必須放置在以 `Management` 結尾的資料夾中
  - 範例：`DocFlow.Application/DocumentManagement/DocumentApplicationService.cs`

- **應用合約層** (`DocFlow.Application.Contracts`)：DTO 和服務介面
  - 資料傳輸物件 (DTOs) 在 `Dtos` 資料夾
  - 應用服務介面

- **基礎設施層** (`DocFlow.EntityFrameworkCore`)：外部服務實作
  - 依賴應用層和領域層
  - 自訂儲存庫實作（例如 `EfCoreDocumentRepository`）
  - 用於資料庫存取的 DbContext

- **展示層** (`DocFlow.HttpApi.Host`)：API 端點
  - 僅依賴基礎設施層
  - 控制器在 `Controllers` 資料夾
  - 請求/回應對應

**依賴規則：**
- 領域層：無依賴
- 應用層：僅依賴領域層
- 基礎設施層：依賴應用層和領域層
- API 層：僅依賴基礎設施層
- 不允許循環依賴

#### Domain-Driven Design (領域驅動設計)

**聚合 (Aggregates)：**
- 代表一致性邊界，與業務概念 1:1 對應
- 必須透過靜態工廠方法建立（例如 `Order.Create(...)`）
- 使用私有/受保護的建構函式來強制使用工廠方法
- 不從基礎類別繼承（除非有業務需求才使用繼承）
- 可以包含實體或值物件，但絕不包含其他聚合
- 負責強制執行所有業務規則和不變條件

**實體 (Entities)：**
- 由聚合擁有，不應在其外部使用
- 透過聚合根工廠方法建立
- 具有唯一識別碼並封裝狀態（沒有公開設定器）
- 使用私有/受保護的建構函式
- 生命週期由聚合根管理

**值物件 (Value Objects)：**
- 不可變且僅由其屬性定義
- 沒有識別
- 透過建構函式或靜態工廠方法建立
- 必須實作值相等性（覆寫 Equals 和 GetHashCode）
- 範例：`Address`、`Money`

**強型別 ID (Strongly Typed IDs)：**
- 使用強型別 ID 而非原始型別（例如 `OrderId`、`CustomerId`）
- 實作為值物件
- 增加型別安全性並防止混淆

**儲存庫 (Repositories)：**
- 介面定義在領域層或應用層
- 僅為聚合根實作
- 使用以業務為導向的方法名稱（例如 `FindOrderByNumber`、`PlaceOrder`）
- 避免 CRUD 風格的方法名稱（無 Set、Create、Update、Delete、Get）
- 實作屬於基礎設施層

**方法命名：**
- 使用以業務為導向、意圖明確的名稱
- 範例：`PlaceOrder`、`ActivateAccount`、`MarkAsShipped`
- 避免通用 CRUD 動詞

### 2. 程式碼風格 (C#)

**命名慣例：**
- 類別、方法和屬性使用 `PascalCase`
- 區域變數和方法參數使用 `camelCase`
- 常數使用 `ALL_CAPS`
- 介面以 `I` 為前綴（例如 `IOrderService`）
- 使用有意義、描述性的名稱；避免縮寫

**格式化：**
- 使用 4 個空格縮排（不使用 tab）
- 使用檔案範圍命名空間
- 一個檔案一個類型
- 方法和類型的左大括號另起一行
- 方法定義之間加空白行

**現代 C# 功能：**
- 當類型明顯時，對區域變數使用 `var`
- 使用模式匹配和運算式主體成員
- 偏好物件和集合初始化器
- 在例外狀況中使用 `nameof` 表示參數名稱

**範例 - 檔案範圍命名空間：**
```csharp
namespace DocFlow.Domain.Documents;

public sealed class Document
{
    // ...existing code...
}
```

**密封類別：**
- 預設將類別設為 `sealed`
- 如需繼承，明確使用 `virtual`

**程式碼結構：**
- 在檔案頂部、命名空間外部組織 using 指示詞
- 按功能/領域組織檔案
- 僅在必要時使用部分類別

### 3. Object Calisthenics (物件體操，適用於領域程式碼)

這些規則**主要適用於業務領域程式碼**（聚合、實體、值物件、領域服務），**其次適用於應用層**服務。

**例外：** DTO、API 模型/合約、配置類別、基礎設施程式碼

**9 項核心原則：**

1. **每個方法一層縮排**
   - 保持方法簡單，單層縮排
   - 需要時提取輔助方法

2. **不使用 ELSE 關鍵字**
   - 使用早期返回而非 else
   - 應用防禦性程式設計原則與保護子句
   ```csharp
   // 好的做法
   public void ProcessOrder(Order order)
   {
       if (order == null) throw new ArgumentNullException(nameof(order));
       if (!order.IsValid) throw new InvalidOperationException("Invalid order");
       // 處理訂單
   }
   ```

3. **包裝所有原始型別和字串**
   - 將原始型別包裝在類別中以提供有意義的上下文
   - 範例：建立 `Age` 類別而非使用 `int`

4. **一級集合**
   - 包含集合的類別不應有其他屬性
   - 在專用類別中封裝集合行為

5. **每行一個點**
   - 限制方法鏈以提高可讀性
   - 將複雜鏈分解為獨立陳述式

6. **不縮寫**
   - 使用完整、有意義的名稱
   - 避免令人困惑的縮寫

7. **保持實體小型**
   - 每個類別最多 10 個方法
   - 每個類別最多 50 行
   - 每個命名空間/套件最多 10 個類別

8. **類別不超過兩個實例變數**
   - 鼓勵單一職責
   - 記錄器（例如 `ILogger`）不計入限制

9. **領域類別中無 Getter/Setter**
   - 使用私有建構函式和靜態工廠方法
   - **例外：** DTO 可以有公開 getter/setter

### 4. 提交慣例

遵循 [Conventional Commits](https://www.conventionalcommits.org/) 規範：

**格式：**
```
<type>[optional scope]: <description>
```

**類型：**
- `feat`：新功能
- `fix`：錯誤修正
- `docs`：文件變更
- `style`：程式碼風格變更（格式化等）
- `refactor`：程式碼重構
- `perf`：效能改進
- `test`：新增或更新測試
- `build`：建置系統變更
- `ci`：CI/CD 變更
- `chore`：維護任務

**範例：**
- `feat(api): add order endpoint`
- `fix(domain): correct order validation logic`
- `test(order): add unit tests for order creation`
- `docs: update code quality standards`

**分支命名：**
```
<type>/<short-description-with-hyphens>
```
範例：`feat/add-user-login`、`fix/order-calculation`、`docs/update-readme`

---

## 測試標準

### 1. 測試框架和工具

**必要函式庫：**
- **測試框架**：xUnit v3（用於所有測試）
- **模擬**：FakeItEasy（用於單元測試）
- **整合測試**：Testcontainers（用於資料庫和外部依賴）
- **合約測試**：Microcks（用於 SOAP/REST 模擬）

### 2. 測試組織

**單元測試：**
- **位置**：`tests/DocFlow.UnitTests/`
- **範圍**：僅測試領域層和應用層
- **目的**：驗證業務邏輯、用例和驗證
- **依賴**：使用 FakeItEasy 模擬所有依賴
- **無真實服務**：無資料庫或外部服務互動
- **資料夾**：按用例和服務組織

**整合測試：**
- **位置**：`tests/DocFlow.IntegrationTests/`
- **範圍**：測試基礎設施層和 API 層，以及跨層整合
- **目的**：驗證完整業務流程、資料持久化、外部呼叫
- **依賴**：使用 Testcontainers 處理資料庫和外部服務
- **合約測試**：使用 Microcks 進行 SOAP/REST/事件模擬
- **資料夾**：按功能組織（例如 `Features/` 用於 API 端點測試）

**架構測試：**
- **位置**：`tests/DocFlow.IntegrationTests/ArchitectureTests.cs`
- **目的**：強制執行層級依賴和架構規則
- **工具**：NetArchTest
- **驗證**：
  - 強制執行層級之間的允許/禁止依賴
  - 檢查禁止的依賴（例如 API/領域中的 EntityFrameworkCore）
  - 可選：檢查領域不可變性

### 3. 測試驅動開發 (TDD)

**流程：**
1. 在實作前編寫測試
2. 執行測試以驗證失敗（紅燈）
3. 實作最少程式碼以通過測試（綠燈）
4. 重構並保持測試通過（重構）

**最佳實踐：**
- 為有效和無效情境編寫測試
- 使用描述性測試名稱說明目的
- 驗證例外狀況和預期結果
- 在測試檔案中記錄測試案例
- 每次重大變更後執行 `dotnet test`

### 4. 測試範例

**單元測試範例：**
```csharp
using Xunit;
using FakeItEasy;

namespace DocFlow.Application.Tests;

public class DocumentApplicationServiceTests
{
    [Fact]
    public void CreateDocument_WithValidData_ShouldSucceed()
    {
        // Arrange
        var repository = A.Fake<IDocumentRepository>();
        var service = new DocumentApplicationService(repository);
        
        // Act
        var result = service.CreateDocument(name: "Test Doc", content: "Content");
        
        // Assert
        Assert.NotNull(result);
        A.CallTo(() => repository.Insert(A<Document>._)).MustHaveHappened();
    }
}
```

**整合測試範例：**
```csharp
using Xunit;
using Testcontainers.PostgreSql;

namespace DocFlow.IntegrationTests.Features;

public class DocumentApiTests : IAsyncLifetime
{
    private PostgreSqlContainer _dbContainer;
    
    public async Task InitializeAsync()
    {
        _dbContainer = new PostgreSqlBuilder().Build();
        await _dbContainer.StartAsync();
    }
    
    [Fact]
    public async Task CreateDocument_ShouldPersistToDatabase()
    {
        // Arrange & Act & Assert
        // 使用真實資料庫的測試實作
    }
    
    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
```

### 5. 持續測試

- 每次重大變更後執行 `dotnet test`
- 在開發期間使用 `dotnet watch test` 進行持續測試執行
- 提交程式碼前所有測試必須通過
- 在任務/使用者故事中記錄測試結果

---

## 使用者體驗一致性

### 1. UI/UX 標準

**Blazor 元件：**
- 在整個應用程式中遵循一致的元件結構
- 使用 ABP Framework 的內建 Blazor 元件以保持一致性
- 維持一致的樣式和主題

**API 一致性：**
- RESTful API 設計原則
- 一致的錯誤回應格式
- 標準 HTTP 狀態碼
- 發生重大變更時進行 API 版本控制

**命名一致性：**
- 在 UI、API 和領域之間使用一致的術語
- 遵循 DDD 的通用語言
- 維持一致的欄位標籤和訊息

### 2. 無障礙性

- 遵循 WCAG 2.1 AA 指南
- 確保鍵盤導航支援
- 提供適當的 ARIA 標籤
- 支援螢幕閱讀器

### 3. 錯誤處理

**使用者面向錯誤：**
- 提供清晰、可操作的錯誤訊息
- 避免在使用者面向訊息中使用技術術語
- 包含如何解決錯誤的指導

**API 錯誤回應：**
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "提供的輸入無效",
    "details": [
      {
        "field": "documentName",
        "message": "文件名稱為必填"
      }
    ]
  }
}
```

### 4. 國際化 (i18n)

- 使用 ABP 的本地化系統支援多種語言
- 將所有使用者面向文字儲存在資源檔案中
- 根據地區設定格式化日期、數字和貨幣

### 5. 響應式設計

- 支援桌面、平板和行動裝置視窗
- 使用響應式 Blazor 元件
- 在多種螢幕尺寸上測試
- 優化觸控和滑鼠互動

---

## 效能要求

### 1. API 回應時間

**目標：**
- **簡單查詢**（例如按 ID 取得）：< 100ms (p95)
- **列表操作**（分頁）：< 300ms (p95)
- **複雜操作**（含聯接）：< 500ms (p95)
- **寫入操作**：< 200ms (p95)

**監控：**
- 使用 Application Insights 或類似 APM 工具
- 追蹤 p50、p95 和 p99 回應時間
- 設定效能降級警報

### 2. 資料庫效能

**查詢最佳化：**
- 在經常查詢的欄位上使用適當的索引
- 避免 N+1 查詢問題
- 需要時使用 `Include()` 進行積極載入
- 對列表查詢實作分頁（每頁最多 100 項）

**連線池：**
- 使用 Entity Framework Core 的內建連線池
- 根據負載配置適當的池大小

**快取策略：**
- 快取經常存取、很少變更的資料
- 對多實例部署使用分散式快取（Redis）
- 實作快取失效策略

### 3. 可擴展性

**水平擴展：**
- 設計無狀態 API 以支援多個實例
- 使用分散式快取處理會話狀態
- 對長時間執行的操作實作基於佇列的處理

**資源管理：**
- 限制每個使用者/客戶端的並發請求
- 對公開 API 實作速率限制
- 對 I/O 密集型操作使用 async/await

### 4. 資源最佳化

**前端：**
- 縮小 JavaScript 和 CSS
- 最佳化圖片（壓縮、使用適當格式）
- 對元件實作延遲載入
- 對靜態資源使用 CDN

**後端：**
- 啟用回應壓縮（gzip/brotli）
- 最佳化序列化（使用 System.Text.Json）
- 最小化承載大小

### 5. 效能測試

**負載測試：**
- 在預期負載條件下測試
- 在生產前識別瓶頸
- 使用 k6、JMeter 或 Apache Bench 等工具

**要追蹤的指標：**
- 每秒請求數 (RPS)
- 回應時間百分位數（p50、p95、p99）
- 錯誤率
- 資料庫查詢時間
- 記憶體使用量
- CPU 使用率

**效能基準：**
```csharp
// 使用 BenchmarkDotNet 的範例
[Benchmark]
public async Task<Document> GetDocument()
{
    return await _repository.GetAsync(id);
}
```

### 6. 資料庫連線管理

- 使用 `using` 陳述式或 `await using` 以正確處置
- 對暫時性故障實作重試策略
- 監控連線池耗盡
- 設定適當的命令逾時

### 7. 記憶體管理

**指南：**
- 正確處置非託管資源
- 避免長時間執行操作中的記憶體洩漏
- 使用記憶體分析工具識別問題
- 對大檔案操作實作串流

---

## 執行與驗證

### 1. 程式碼審查檢查清單

批准任何 PR 前，驗證：
- [ ] 遵循整潔架構原則
- [ ] 遵守領域程式碼的 DDD 指南
- [ ] 遵循領域/應用程式碼的物件體操
- [ ] 使用正確的 C# 程式碼風格
- [ ] 有適當的單元和/或整合測試
- [ ] 通過所有現有測試
- [ ] 遵循 Conventional Commits 格式
- [ ] 包含效能考量
- [ ] 維持 UX 一致性

### 2. 自動化檢查

**建置管道：**
- 編譯所有專案，無警告
- 執行所有單元和整合測試
- 執行架構驗證測試
- 檢查程式碼覆蓋率（目標：領域/應用層 >80%）
- 執行靜態程式碼分析工具

**預提交掛鉤（建議）：**
- 使用 `dotnet format` 格式化程式碼
- 執行單元測試
- 驗證提交訊息格式

### 3. 文件更新

- 引入新標準時更新本文件
- 在 ADR（架構決策記錄）中記錄架構決策
- 保持 README.md 與專案設定說明同步

---

## 參考資源

### 官方文件
- [ABP Framework 文件](https://abp.io/docs)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/TheCleanArchitecture.html)
- [Domain-Driven Design](https://www.domainlanguage.com/ddd/)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Microsoft .NET C# 編碼慣例](https://learn.microsoft.com/zh-tw/dotnet/csharp/fundamentals/coding-style/coding-conventions)

### 書籍
- "Domain-Driven Design: Tackling Complexity in the Heart of Software" by Eric Evans
- "Implementing Domain-Driven Design" by Vaughn Vernon
- "Clean Code: A Handbook of Agile Software Craftsmanship" by Robert C. Martin
- "Clean Architecture: A Craftsman's Guide to Software Structure and Design" by Robert C. Martin

### 工具
- [xUnit](https://xunit.net/) - 測試框架
- [FakeItEasy](https://fakeiteasy.github.io/) - 模擬函式庫
- [Testcontainers](https://dotnet.testcontainers.org/) - 整合測試
- [NetArchTest](https://github.com/BenMorris/NetArchTest) - 架構測試
- [BenchmarkDotNet](https://benchmarkdotnet.org/) - 效能基準測試

---

## 持續改進

本文件應視為活文件，並在以下情況下更新：
- 發現新的最佳實踐
- 升級工具和框架
- 團隊從回顧中學習
- 效能要求變更

最後更新：2025-11-09
版本：1.0
