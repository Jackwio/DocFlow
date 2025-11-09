## 🏗️ 專案層級內容描述 (依據規範整理)

### 1. `DocFlow.Domain`
**核心領域層 (Domain Layer)**

* **實體 (Entities):**
    * 存放所有的 **Entity** 類別（實體，如：`Document`, `User` 等）。
    * 位置：**`DocFlow.Domain/Entities`** 資料夾下。
* **自訂倉儲介面 (Custom Repository Interfaces):**
    * 若需要**自訂**的 Repository 方法，其**介面**應放在此處。
    * 例如：`IDocumentRepository`。
* **領域服務 (Domain Services):**
    * **命名規範：** 必須為 `Manager` 結尾（例如：`DocumentManager`）。
    * 處理涉及多個實體或複雜業務邏輯的領域服務（如果有的話）。

### 2. `DocFlow.Domain.Shared`
**共享領域層 (Domain Shared Layer)**

* **列舉 (Enums):**
    * 存放所有專案共享的 **Enum** 定義。
    * 位置：**`DocFlow.Domain.Shared/Enums`** 資料夾下。

### 3. `DocFlow.Application`
**應用服務層 (Application Layer)**

* **應用服務 (Application Services):**
    * 存放實現業務邏輯的 **Service 類別**。
    * **命名規範：** 必須為 `ApplicationService` 結尾（例如：`DocumentApplicationService`）。
    * **資料夾規範：** 必須放在以 **Management** 結尾的資料夾下（例如：`DocFlow.Application/DocumentManagement/DocumentApplicationService.cs`）。

### 4. `DocFlow.Application.Contracts`
**應用服務合約層 (Application Contracts Layer)**

* **資料傳輸物件 (Dtos):**
    * 存放所有的 **DTO (Data Transfer Object)** 類別，用於應用服務層與展示層之間傳輸資料。
    * 位置：**`DocFlow.Application.Contracts/Dtos`** 資料夾下。
* **應用服務介面 (Application Service Interfaces):**
    * 如果應用服務有定義介面，也應放在此處。

### 5. `DocFlow.EntityFrameworkCore`
**基礎設施層 - 資料存取 (Infrastructure - Data Access)**

* **自訂倉儲實作 (Custom Repository Implementations):**
    * 存放對應於 `DocFlow.Domain` 中自訂 Repository 介面的 **實作** 類別（使用 Entity Framework Core）。
    * 例如：`EfCoreDocumentRepository` 實作 `IDocumentRepository`。
* **DbContext:**
    * 專案的 EF Core `DbContext` 類別。
* **`DocFlow.DbMigrator` 專案會使用此層來進行資料庫遷移。**

### 6. `DocFlow.HttpApi.Host`
**展示層 - API 宿主 (Presentation Layer - API Host)**

* **控制器 (Controllers):**
    * 存放公開給外部呼叫的 **Web API Controller** 類別。
    * 位置：**`DocFlow.HttpApi.Host/Controllers`** 資料夾內。
* **應用程式啟動與配置：**
    * 包含 `Program.cs`、`Startup.cs`（或新的 `Program.cs`）以及相關的 Web Host 配置。
