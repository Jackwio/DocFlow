# DocFlow AI 分類功能文檔

## 概述

AI 分類功能使用 OpenAI 的 GPT 模型實現文件的自動分類和標籤。此功能幫助操作人員快速分類文件，提供 AI 建議的標籤、生成摘要，並提高文件處理效率。

## 功能特性

### 1. AI 驅動的標籤建議
- 分析文件內容以建議相關標籤
- 為每個建議提供信心分數 (0.0 到 1.0)
- 包含每個建議標籤的推理說明
- 支援在應用前進行人工審核

### 2. 一鍵套用
- 通過單一 API 調用應用所有 AI 建議
- 將建議的標籤轉換為實際應用的標籤
- 更新文件狀態為「已分類」
- 透過領域事件維護審計追蹤

### 3. 文件摘要生成
- 生成文件內容的簡短摘要
- 可配置摘要長度（預設：500 字元）
- 幫助操作人員快速了解文件內容

### 4. 租戶級別控制
- 通過配置啟用/停用 AI 功能
- 檢查 AI 可用性狀態
- 未來：資料庫中的每租戶 AI 設定

## 配置

在 `appsettings.json` 中添加以下配置：

```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key-here",
    "Model": "gpt-4o-mini",
    "Enabled": true,
    "AutoApplySuggestions": false,
    "MinConfidenceThreshold": 0.7
  }
}
```

### 配置參數

| 參數 | 類型 | 預設值 | 說明 |
|------|------|--------|------|
| `ApiKey` | string | "test" | 您的 OpenAI API 金鑰 |
| `Model` | string | "gpt-4o-mini" | 要使用的 OpenAI 模型 |
| `Enabled` | bool | true | 啟用/停用 AI 功能 |
| `AutoApplySuggestions` | bool | false | 自動應用高信心度建議 |
| `MinConfidenceThreshold` | double | 0.7 | 自動應用的最低信心分數 |

## API 端點

### 1. 生成 AI 建議

**端點：** `POST /api/documents/ai/{documentId}/suggestions`

**說明：** 分析文件並生成 AI 驅動的分類建議。

**請求：**
```http
POST /api/documents/ai/3fa85f64-5717-4562-b3fc-2c963f66afa6/suggestions
Authorization: Bearer {token}
```

**回應：**
```json
{
  "suggestedTags": [
    {
      "tagName": "發票",
      "confidence": 0.95,
      "reasoning": "文件包含發票號碼和付款條款"
    },
    {
      "tagName": "會計",
      "confidence": 0.90,
      "reasoning": "包含金額的財務文件"
    }
  ],
  "suggestedQueueId": null,
  "confidence": 0.92,
  "summary": "這是一份服務發票...",
  "generatedAt": "2025-11-10T23:15:00Z"
}
```

### 2. 套用 AI 建議（一鍵套用）

**端點：** `POST /api/documents/ai/apply-suggestions`

**說明：** 將 AI 建議的標籤套用到文件。

**請求：**
```http
POST /api/documents/ai/apply-suggestions
Content-Type: application/json
Authorization: Bearer {token}

{
  "documentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**回應：**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "invoice-2025.pdf",
  "status": "Classified",
  "tags": ["發票", "會計"],
  "aiSuggestion": {
    "suggestedTags": [...],
    "confidence": 0.92,
    "summary": "...",
    "generatedAt": "2025-11-10T23:15:00Z"
  }
}
```

### 3. 生成文件摘要

**端點：** `POST /api/documents/ai/summary`

**說明：** 生成文件內容的簡短摘要。

**請求：**
```http
POST /api/documents/ai/summary
Content-Type: application/json
Authorization: Bearer {token}

{
  "documentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "maxLength": 500
}
```

**回應：**
```json
{
  "documentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "summary": "此發票日期為 2025 年 1 月 15 日，要求支付 2024 年第四季度提供的諮詢服務費用。應付總額為 5,000 美元，付款條件為 30 天內付清。",
  "generatedAt": "2025-11-10T23:15:00Z"
}
```

### 4. 檢查 AI 狀態

**端點：** `GET /api/documents/ai/status`

**說明：** 檢查 AI 服務是否啟用且可用。

**請求：**
```http
GET /api/documents/ai/status
Authorization: Bearer {token}
```

**回應：**
```json
true
```

## 使用流程

### 操作人員流程

1. **上傳文件**
   - 通過 `/api/documents/upload` 上傳文件
   - 文件以「待處理」狀態存儲

2. **請求 AI 建議**
   - 調用 `/api/documents/ai/{documentId}/suggestions`
   - 審核建議的標籤和信心分數
   - 閱讀 AI 為每個標籤生成的推理

3. **套用建議（一鍵套用）**
   - 調用 `/api/documents/ai/apply-suggestions`
   - 所有建議的標籤以「AI 套用」來源應用
   - 文件狀態更改為「已分類」

4. **手動調整**（可選）
   - 通過 `/api/documents/{id}/tags` 添加手動標籤
   - 如需要，移除不正確的標籤
   - 手動標籤標記為「手動」來源

### 管理員流程

1. **啟用 AI 功能**
   - 在配置中設定 `OpenAI:Enabled = true`
   - 配置 OpenAI API 金鑰
   - 選擇適當的模型（建議使用 gpt-4o-mini 以節省成本）

2. **監控使用情況**
   - 檢查 AI 操作的領域事件
   - 審核信心分數和準確性
   - 根據需要調整 `MinConfidenceThreshold`

3. **停用 AI**（緊急情況）
   - 設定 `OpenAI:Enabled = false`
   - 系統繼續在沒有 AI 的情況下工作
   - 手動分類仍然可用

## 實現功能清單

### 已實現功能 ✅

1. ✅ **作為操作人員，我想在上傳後看到建議標籤並可一鍵套用以節省分類時間**
   - 提供 `/api/documents/ai/{documentId}/suggestions` 端點生成建議
   - 提供 `/api/documents/ai/apply-suggestions` 端點一鍵套用
   - 建議包含標籤名稱、信心分數和推理說明

2. ✅ **作為操作人員，我想要求文件簡短摘要以快速了解內容**
   - 提供 `/api/documents/ai/summary` 端點生成摘要
   - 可配置摘要最大長度
   - 摘要基於文件內容使用 GPT 生成

3. ✅ **作為租戶管理者，我想一鍵關閉所有 AI 相關行為（即時生效）以應急合規事件**
   - 配置 `OpenAI:Enabled` 開關
   - 提供 `/api/documents/ai/status` 端點檢查狀態
   - AI 關閉時系統繼續正常運作（優雅降級）

### 技術實現要點

- ✅ 使用 OpenAI API (`Azure.AI.OpenAI` 套件)
- ✅ OpenAI API Key 假定為 "test"（可在配置中更改）
- ✅ 建議標籤與已應用標籤分離存儲
- ✅ 支援手動審核後應用（一鍵套用）
- ✅ 支援自動應用（可配置，預設關閉）
- ✅ 領域事件追蹤所有 AI 操作
- ✅ 單元測試覆蓋核心邏輯

## 資料庫結構

### DocumentAiSuggestedTags 表

存儲應用前的 AI 建議標籤。

| 欄位 | 類型 | 說明 |
|------|------|------|
| DocumentId | UUID | 文件外鍵 |
| Id | int | 自增 ID |
| TagName | varchar(50) | 建議的標籤名稱 |
| Confidence | double | 信心分數 (0.0-1.0) |
| Reasoning | varchar(500) | AI 建議推理 |

### Documents 表（新增欄位）

| 欄位 | 類型 | 說明 |
|------|------|------|
| AiConfidence | double | 整體 AI 信心度 |
| AiGeneratedAt | timestamp | 建議生成時間 |
| AiSuggestedQueueId | UUID | 建議的路由隊列 |
| AiSummary | varchar(2000) | AI 生成的摘要 |

### DocumentTags 表（修改）

| 欄位 | 類型 | 說明 |
|------|------|------|
| ConfidenceScore | double | AI 標籤的信心分數 |

## 安全考慮

1. **API 金鑰保護**
   - 將 OpenAI API 金鑰存儲在安全配置中（如 Azure Key Vault）
   - 切勿將 API 金鑰提交到源代碼控制
   - 開發和生產使用不同的金鑰

2. **速率限制**
   - 實施速率限制以防止濫用
   - 監控 OpenAI API 使用和成本
   - 設定適當的超時值

3. **輸入驗證**
   - 在發送到 AI 之前驗證文件內容
   - 存儲前清理 AI 回應
   - 限制 AI 處理的文件大小

4. **隱私**
   - 注意文件內容會發送到 OpenAI
   - 考慮資料駐留要求
   - 審閱 OpenAI 的隱私政策

## 成本優化

1. **模型選擇**
   - 使用 `gpt-4o-mini` 進行經濟高效的處理
   - 為複雜文件保留 `gpt-4o`
   - 根據準確性要求調整模型

2. **內容優化**
   - 限制發送到 AI 的文字（前 4000 字元）
   - 僅提取相關內容（跳過頁首/頁尾）
   - 快取結果以避免冗餘 API 調用

## 故障排除

### AI 服務不可用

**問題：** `/api/documents/ai/status` 返回 `false`

**解決方案：**
- 檢查 `OpenAI:ApiKey` 配置
- 驗證 `OpenAI:Enabled = true`
- 直接使用 OpenAI 測試 API 金鑰
- 檢查網路連接

### 信心分數低

**問題：** AI 建議的信心分數持續偏低

**解決方案：**
- 改善文件品質（OCR、掃描）
- 在提示中提供更多上下文
- 使用更高級的模型 (gpt-4o)
- 針對您的領域微調提示

### 建議不正確

**問題：** AI 建議不相關的標籤

**解決方案：**
- 審核和優化系統提示
- 在提示中添加特定領域的範例
- 增加 `MinConfidenceThreshold`
- 使用反饋改進提示

## 未來增強功能

- [ ] 資料庫中的持久化租戶級 AI 設定
- [ ] 基於信心閾值的自動應用
- [ ] 從更正中學習的反饋循環
- [ ] 隊列建議映射
- [ ] 多語言支援
- [ ] 自訂 AI 模型微調
- [ ] 批次處理優化
- [ ] 成本追蹤和報告

## 參考資料

- [OpenAI API 文檔](https://platform.openai.com/docs)
- [Azure.AI.OpenAI 套件](https://www.nuget.org/packages/Azure.AI.OpenAI)
- [DocFlow 架構文檔](../README.md)
