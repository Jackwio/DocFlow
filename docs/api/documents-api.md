# Documents API Documentation

## 基本資訊

**Base URL:** `/api/documents`

**認證方式:** Bearer Token (JWT)

**Content-Type:** 
- `application/json` (一般請求)
- `multipart/form-data` (檔案上傳)

---

## API 端點列表

### 1. 上傳單一檔案

**功能:** 上傳單一文件並自動觸發分類流程 (US1)

**端點:** `POST /api/documents/upload`

**Content-Type:** `multipart/form-data`

**請求參數:**

| 參數 | 類型 | 必填 | 說明 |
|------|------|------|------|
| File | IFormFile | 是 | 要上傳的檔案 |
| FileName | string | 否 | 自訂檔案名稱（預設為原始檔案名） |
| Description | string | 否 | 文件描述 |

**請求範例 (JavaScript/Fetch):**

```javascript
const formData = new FormData();
formData.append('File', fileInput.files[0]);
formData.append('FileName', 'invoice-2024-001.pdf');
formData.append('Description', 'Monthly invoice for January');

const response = await fetch('/api/documents/upload', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`
  },
  body: formData
});

const document = await response.json();
```

**請求範例 (cURL):**

```bash
curl -X POST "https://api.docflow.com/api/documents/upload" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "File=@/path/to/file.pdf" \
  -F "FileName=invoice-2024-001.pdf" \
  -F "Description=Monthly invoice for January"
```

**回應範例 (200 OK):**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "invoice-2024-001.pdf",
  "fileSize": 1048576,
  "mimeType": "application/pdf",
  "description": "Monthly invoice for January",
  "status": "Pending",
  "uploadedAt": "2024-11-10T10:30:00Z",
  "blobUri": "https://storage.blob.core.windows.net/documents/3fa85f64...",
  "classificationResults": [],
  "tags": [],
  "routingHistory": []
}
```

---

### 2. 批次上傳檔案

**功能:** 一次上傳多個文件 (US1)

**端點:** `POST /api/documents/upload-batch`

**Content-Type:** `multipart/form-data`

**請求參數:**

| 參數 | 類型 | 必填 | 說明 |
|------|------|------|------|
| files | IFormFileCollection | 是 | 要上傳的多個檔案 |

**請求範例 (JavaScript/Fetch):**

```javascript
const formData = new FormData();
for (let file of fileInput.files) {
  formData.append('files', file);
}

const response = await fetch('/api/documents/upload-batch', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`
  },
  body: formData
});

const documents = await response.json();
```

**請求範例 (cURL):**

```bash
curl -X POST "https://api.docflow.com/api/documents/upload-batch" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "files=@/path/to/file1.pdf" \
  -F "files=@/path/to/file2.pdf"
```

**回應範例 (200 OK):**

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fileName": "file1.pdf",
    "status": "Pending",
    "uploadedAt": "2024-11-10T10:30:00Z"
  },
  {
    "id": "7fb95f64-6828-5673-c4gd-3d074g77bgb7",
    "fileName": "file2.pdf",
    "status": "Pending",
    "uploadedAt": "2024-11-10T10:30:01Z"
  }
]
```

---

### 3. 取得單一文件資訊

**功能:** 根據文件 ID 取得詳細資訊 (US2)

**端點:** `GET /api/documents/{id}`

**請求參數:**

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|------|------|
| id | Path | Guid | 是 | 文件 ID |

**請求範例:**

```javascript
const response = await fetch('/api/documents/3fa85f64-5717-4562-b3fc-2c963f66afa6', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

const document = await response.json();
```

**回應範例 (200 OK):**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "invoice-2024-001.pdf",
  "fileSize": 1048576,
  "mimeType": "application/pdf",
  "description": "Monthly invoice for January",
  "status": "Classified",
  "uploadedAt": "2024-11-10T10:30:00Z",
  "classifiedAt": "2024-11-10T10:30:15Z",
  "blobUri": "https://storage.blob.core.windows.net/documents/3fa85f64...",
  "classificationResults": [
    {
      "ruleId": "rule-001",
      "ruleName": "Invoice Detection",
      "tagName": "Invoice",
      "confidenceScore": 0.95,
      "appliedAt": "2024-11-10T10:30:15Z"
    }
  ],
  "tags": [
    {
      "name": "Invoice",
      "source": "Automatic",
      "addedAt": "2024-11-10T10:30:15Z"
    }
  ],
  "routingHistory": [
    {
      "fromQueueId": null,
      "toQueueId": "queue-accounting",
      "routedAt": "2024-11-10T10:30:16Z"
    }
  ]
}
```

---

### 4. 取得文件列表 (分頁與篩選)

**功能:** 取得文件列表，支援狀態篩選和日期範圍 (US2, US4)

**端點:** `GET /api/documents`

**請求參數:**

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|------|------|
| status | Query | DocumentStatus | 否 | 文件狀態篩選 (Pending/Classified/Failed/Routed) |
| uploadedAfter | Query | DateTime | 否 | 上傳時間起始 (ISO 8601) |
| uploadedBefore | Query | DateTime | 否 | 上傳時間結束 (ISO 8601) |
| skipCount | Query | int | 否 | 跳過筆數 (預設: 0) |
| maxResultCount | Query | int | 否 | 每頁筆數 (預設: 10) |

**請求範例:**

```javascript
const params = new URLSearchParams({
  status: 'Classified',
  uploadedAfter: '2024-11-01T00:00:00Z',
  uploadedBefore: '2024-11-10T23:59:59Z',
  skipCount: 0,
  maxResultCount: 20
});

const response = await fetch(`/api/documents?${params}`, {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

const result = await response.json();
```

**請求範例 (cURL):**

```bash
curl -X GET "https://api.docflow.com/api/documents?status=Classified&skipCount=0&maxResultCount=20" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json"
```

**回應範例 (200 OK):**

```json
{
  "totalCount": 150,
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fileName": "invoice-2024-001.pdf",
      "fileSize": 1048576,
      "status": "Classified",
      "uploadedAt": "2024-11-10T10:30:00Z",
      "classifiedAt": "2024-11-10T10:30:15Z",
      "tags": ["Invoice", "Accounting"]
    },
    {
      "id": "7fb95f64-6828-5673-c4gd-3d074g77bgb7",
      "fileName": "contract-2024-002.pdf",
      "fileSize": 2097152,
      "status": "Classified",
      "uploadedAt": "2024-11-10T11:00:00Z",
      "classifiedAt": "2024-11-10T11:00:20Z",
      "tags": ["Contract", "Legal"]
    }
  ]
}
```

---

### 5. 重試分類失敗的文件

**功能:** 手動觸發重新分類 (US3)

**端點:** `POST /api/documents/{id}/retry`

**請求參數:**

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|------|------|
| id | Path | Guid | 是 | 文件 ID |

**請求範例:**

```javascript
const response = await fetch('/api/documents/3fa85f64-5717-4562-b3fc-2c963f66afa6/retry', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

const document = await response.json();
```

**請求範例 (cURL):**

```bash
curl -X POST "https://api.docflow.com/api/documents/3fa85f64-5717-4562-b3fc-2c963f66afa6/retry" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json"
```

**回應範例 (200 OK):**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "invoice-2024-001.pdf",
  "status": "Pending",
  "uploadedAt": "2024-11-10T10:30:00Z",
  "classificationResults": []
}
```

---

### 6. 進階搜尋文件

**功能:** 使用複雜條件搜尋文件 (US4)

**端點:** `POST /api/documents/search`

**Content-Type:** `application/json`

**請求 Body (DocumentSearchDto):**

```typescript
{
  fileName?: string;           // 檔案名稱 (部分匹配)
  status?: DocumentStatus;      // 文件狀態
  tags?: string[];              // 標籤列表 (OR 條件)
  uploadedAfter?: DateTime;     // 上傳時間起始
  uploadedBefore?: DateTime;    // 上傳時間結束
  minFileSize?: number;         // 最小檔案大小 (bytes)
  maxFileSize?: number;         // 最大檔案大小 (bytes)
  skipCount?: number;           // 跳過筆數 (預設: 0)
  maxResultCount?: number;      // 每頁筆數 (預設: 10)
}
```

**請求範例:**

```javascript
const searchCriteria = {
  fileName: 'invoice',
  status: 'Classified',
  tags: ['Invoice', 'Accounting'],
  uploadedAfter: '2024-11-01T00:00:00Z',
  minFileSize: 1024,
  maxFileSize: 10485760,
  skipCount: 0,
  maxResultCount: 20
};

const response = await fetch('/api/documents/search', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify(searchCriteria)
});

const result = await response.json();
```

**請求範例 (cURL):**

```bash
curl -X POST "https://api.docflow.com/api/documents/search" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "fileName": "invoice",
    "status": "Classified",
    "tags": ["Invoice", "Accounting"],
    "skipCount": 0,
    "maxResultCount": 20
  }'
```

**回應範例 (200 OK):**

```json
{
  "totalCount": 45,
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fileName": "invoice-2024-001.pdf",
      "fileSize": 1048576,
      "status": "Classified",
      "uploadedAt": "2024-11-10T10:30:00Z",
      "tags": ["Invoice", "Accounting"]
    }
  ]
}
```

---

### 7. 新增手動標籤

**功能:** 為文件手動新增標籤 (US10)

**端點:** `POST /api/documents/{id}/tags`

**Content-Type:** `application/json`

**請求參數:**

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|------|------|
| id | Path | Guid | 是 | 文件 ID |

**請求 Body (AddManualTagDto):**

```typescript
{
  tagName: string;    // 標籤名稱
}
```

**請求範例:**

```javascript
const response = await fetch('/api/documents/3fa85f64-5717-4562-b3fc-2c963f66afa6/tags', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    tagName: 'Urgent'
  })
});

const document = await response.json();
```

**請求範例 (cURL):**

```bash
curl -X POST "https://api.docflow.com/api/documents/3fa85f64-5717-4562-b3fc-2c963f66afa6/tags" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"tagName": "Urgent"}'
```

**回應範例 (200 OK):**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "invoice-2024-001.pdf",
  "status": "Classified",
  "tags": [
    {
      "name": "Invoice",
      "source": "Automatic",
      "addedAt": "2024-11-10T10:30:15Z"
    },
    {
      "name": "Urgent",
      "source": "Manual",
      "addedAt": "2024-11-10T14:30:00Z",
      "addedBy": "user@example.com"
    }
  ]
}
```

---

### 8. 移除手動標籤

**功能:** 移除文件的手動標籤 (US10)

**端點:** `DELETE /api/documents/{id}/tags/{tagName}`

**請求參數:**

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|------|------|
| id | Path | Guid | 是 | 文件 ID |
| tagName | Path | string | 是 | 要移除的標籤名稱 |

**請求範例:**

```javascript
const response = await fetch('/api/documents/3fa85f64-5717-4562-b3fc-2c963f66afa6/tags/Urgent', {
  method: 'DELETE',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

// 204 No Content (成功)
```

**請求範例 (cURL):**

```bash
curl -X DELETE "https://api.docflow.com/api/documents/3fa85f64-5717-4562-b3fc-2c963f66afa6/tags/Urgent" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**回應:** `204 No Content` (成功)

---

### 9. 取得分類歷史記錄

**功能:** 取得文件的完整分類歷史（稽核軌跡）(US12)

**端點:** `GET /api/documents/{id}/history`

**請求參數:**

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|------|------|
| id | Path | Guid | 是 | 文件 ID |

**請求範例:**

```javascript
const response = await fetch('/api/documents/3fa85f64-5717-4562-b3fc-2c963f66afa6/history', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

const document = await response.json();
```

**請求範例 (cURL):**

```bash
curl -X GET "https://api.docflow.com/api/documents/3fa85f64-5717-4562-b3fc-2c963f66afa6/history" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json"
```

**回應範例 (200 OK):**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "invoice-2024-001.pdf",
  "status": "Routed",
  "uploadedAt": "2024-11-10T10:30:00Z",
  "classificationResults": [
    {
      "ruleId": "rule-001",
      "ruleName": "Invoice Detection",
      "tagName": "Invoice",
      "confidenceScore": 0.95,
      "appliedAt": "2024-11-10T10:30:15Z"
    },
    {
      "ruleId": "rule-005",
      "ruleName": "Accounting Department",
      "tagName": "Accounting",
      "confidenceScore": 0.88,
      "appliedAt": "2024-11-10T10:30:15Z"
    }
  ],
  "tags": [
    {
      "name": "Invoice",
      "source": "Automatic",
      "addedAt": "2024-11-10T10:30:15Z"
    },
    {
      "name": "Accounting",
      "source": "Automatic",
      "addedAt": "2024-11-10T10:30:15Z"
    },
    {
      "name": "Urgent",
      "source": "Manual",
      "addedAt": "2024-11-10T14:30:00Z",
      "addedBy": "user@example.com"
    }
  ],
  "routingHistory": [
    {
      "fromQueueId": null,
      "toQueueId": "queue-accounting",
      "toQueueName": "Accounting Queue",
      "routedAt": "2024-11-10T10:30:16Z",
      "routedBy": "system"
    }
  ]
}
```

---

## 資料模型定義

### DocumentDto

```typescript
{
  id: string;                           // 文件 ID (Guid)
  fileName: string;                     // 檔案名稱
  fileSize: number;                     // 檔案大小 (bytes)
  mimeType: string;                     // MIME 類型 (e.g., "application/pdf")
  description?: string;                 // 文件描述
  status: DocumentStatus;               // 文件狀態
  uploadedAt: string;                   // 上傳時間 (ISO 8601)
  classifiedAt?: string;                // 分類完成時間
  blobUri: string;                      // Blob 儲存位置
  classificationResults: ClassificationResultDto[];
  tags: TagDto[];
  routingHistory: RoutingHistoryDto[];
}
```

### DocumentListDto

```typescript
{
  id: string;
  fileName: string;
  fileSize: number;
  status: DocumentStatus;
  uploadedAt: string;
  classifiedAt?: string;
  tags: string[];                       // 簡化版標籤列表
}
```

### ClassificationResultDto

```typescript
{
  ruleId: string;                       // 規則 ID
  ruleName: string;                     // 規則名稱
  tagName: string;                      // 標籤名稱
  confidenceScore: number;              // 信心分數 (0-1)
  appliedAt: string;                    // 套用時間
}
```

### TagDto

```typescript
{
  name: string;                         // 標籤名稱
  source: TagSource;                    // 來源 (Automatic/Manual)
  addedAt: string;                      // 新增時間
  addedBy?: string;                     // 新增者 (手動標籤才有)
}
```

### RoutingHistoryDto

```typescript
{
  fromQueueId?: string;                 // 來源佇列 ID
  fromQueueName?: string;               // 來源佇列名稱
  toQueueId: string;                    // 目標佇列 ID
  toQueueName: string;                  // 目標佇列名稱
  routedAt: string;                     // 路由時間
  routedBy: string;                     // 路由者
}
```

### DocumentStatus (Enum)

```typescript
enum DocumentStatus {
  Pending = 0,        // 待處理
  Classified = 1,     // 已分類
  Failed = 2,         // 分類失敗
  Routed = 3          // 已路由
}
```

### TagSource (Enum)

```typescript
enum TagSource {
  Automatic = 0,      // 自動標籤
  Manual = 1          // 手動標籤
}
```

---

## 錯誤回應格式

### 400 Bad Request

```json
{
  "error": {
    "code": "ValidationError",
    "message": "File is required",
    "details": null
  }
}
```

### 401 Unauthorized

```json
{
  "error": {
    "code": "Unauthorized",
    "message": "Invalid or expired token",
    "details": null
  }
}
```

### 404 Not Found

```json
{
  "error": {
    "code": "NotFound",
    "message": "Document with ID '3fa85f64-5717-4562-b3fc-2c963f66afa6' not found",
    "details": null
  }
}
```

### 500 Internal Server Error

```json
{
  "error": {
    "code": "InternalServerError",
    "message": "An unexpected error occurred",
    "details": "Stack trace or additional info (only in development)"
  }
}
```

---

## 分頁說明

所有列表 API 皆使用 ABP Framework 的標準分頁格式：

**請求參數:**
- `skipCount`: 跳過筆數（從 0 開始）
- `maxResultCount`: 每頁最大筆數

**回應格式:**
```json
{
  "totalCount": 150,      // 總筆數
  "items": [...]          // 當前頁資料
}
```

**前端分頁計算:**
```javascript
const pageNumber = 1; // 第 1 頁（從 1 開始）
const pageSize = 20;
const skipCount = (pageNumber - 1) * pageSize; // 0

const totalPages = Math.ceil(totalCount / pageSize);
```

---

## 認證與授權

### 取得 Token

請參考 ABP Framework 的認證文件，或使用以下端點：

```
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password&
username=admin&
password=1q2w3E*&
client_id=DocFlow_App&
scope=DocFlow
```

### 使用 Token

所有 API 請求都需要在 Header 中包含 Bearer Token：

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 檔案上傳限制

- **最大檔案大小:** 50 MB (可在伺服器端設定調整)
- **支援的檔案類型:** PDF, DOCX, XLSX, PNG, JPG, etc.
- **編碼:** 使用 `multipart/form-data` 進行上傳

### 前端範例：檔案上傳進度追蹤

```javascript
const uploadDocument = async (file, onProgress) => {
  const formData = new FormData();
  formData.append('File', file);

  const xhr = new XMLHttpRequest();

  return new Promise((resolve, reject) => {
    xhr.upload.addEventListener('progress', (e) => {
      if (e.lengthComputable) {
        const percentComplete = (e.loaded / e.total) * 100;
        onProgress(percentComplete);
      }
    });

    xhr.addEventListener('load', () => {
      if (xhr.status === 200) {
        resolve(JSON.parse(xhr.responseText));
      } else {
        reject(new Error(`Upload failed: ${xhr.status}`));
      }
    });

    xhr.addEventListener('error', () => reject(new Error('Upload failed')));

    xhr.open('POST', '/api/documents/upload');
    xhr.setRequestHeader('Authorization', `Bearer ${token}`);
    xhr.send(formData);
  });
};

// 使用範例
await uploadDocument(file, (progress) => {
  console.log(`Upload progress: ${progress}%`);
});
```

---

## 常見使用情境

### 情境 1: 上傳文件並追蹤狀態

```javascript
// 1. 上傳文件
const document = await uploadDocument(file);
const documentId = document.id;

// 2. 輪詢狀態（每 2 秒檢查一次）
const pollStatus = async () => {
  const doc = await fetch(`/api/documents/${documentId}`).then(r => r.json());
  
  if (doc.status === 'Classified') {
    console.log('分類完成！', doc.tags);
    return doc;
  } else if (doc.status === 'Failed') {
    console.error('分類失敗');
    return doc;
  } else {
    setTimeout(pollStatus, 2000);
  }
};

await pollStatus();
```

### 情境 2: 搜尋特定標籤的文件

```javascript
const searchInvoices = async () => {
  const result = await fetch('/api/documents/search', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      tags: ['Invoice'],
      status: 'Classified',
      uploadedAfter: '2024-11-01T00:00:00Z',
      skipCount: 0,
      maxResultCount: 50
    })
  }).then(r => r.json());

  console.log(`找到 ${result.totalCount} 份發票`);
  return result.items;
};
```

### 情境 3: 批次上傳並顯示進度

```javascript
const uploadMultipleDocuments = async (files) => {
  const results = [];
  
  for (let i = 0; i < files.length; i++) {
    console.log(`上傳中 ${i + 1}/${files.length}: ${files[i].name}`);
    
    try {
      const doc = await uploadDocument(files[i]);
      results.push({ success: true, document: doc });
    } catch (error) {
      results.push({ success: false, error: error.message });
    }
  }
  
  const successCount = results.filter(r => r.success).length;
  console.log(`成功上傳 ${successCount}/${files.length} 份文件`);
  
  return results;
};
```

---

## 速率限制

- **每分鐘最多 60 次請求**（每個使用者）
- **檔案上傳：每分鐘最多 10 次**

超過限制將回應：

```json
{
  "error": {
    "code": "TooManyRequests",
    "message": "Rate limit exceeded. Please try again later.",
    "details": "Retry after 30 seconds"
  }
}
```

---

## WebSocket 即時通知 (未來功能)

未來版本將支援 WebSocket 進行即時狀態推送：

```javascript
// 預計功能 (未實作)
const ws = new WebSocket('wss://api.docflow.com/ws/documents');

ws.onmessage = (event) => {
  const notification = JSON.parse(event.data);
  
  if (notification.type === 'DocumentClassified') {
    console.log('文件分類完成：', notification.documentId);
  }
};
```

---

## 版本資訊

- **API 版本:** v1
- **文件版本:** 1.0.0
- **最後更新:** 2024-11-10

---

## 聯絡資訊

如有 API 相關問題，請聯繫：
- **技術支援:** support@docflow.com
- **開發團隊:** dev@docflow.com

---

## 附錄：完整 TypeScript 介面定義

```typescript
// ========== API Request Models ==========

export interface UploadDocumentWithFileDto {
  file: File;
  fileName?: string;
  description?: string;
}

export interface UploadDocumentDto {
  fileName?: string;
  description?: string;
}

export interface AddManualTagDto {
  tagName: string;
}

export interface DocumentSearchDto {
  fileName?: string;
  status?: DocumentStatus;
  tags?: string[];
  uploadedAfter?: string;
  uploadedBefore?: string;
  minFileSize?: number;
  maxFileSize?: number;
  skipCount?: number;
  maxResultCount?: number;
}

// ========== API Response Models ==========

export interface DocumentDto {
  id: string;
  fileName: string;
  fileSize: number;
  mimeType: string;
  description?: string;
  status: DocumentStatus;
  uploadedAt: string;
  classifiedAt?: string;
  blobUri: string;
  classificationResults: ClassificationResultDto[];
  tags: TagDto[];
  routingHistory: RoutingHistoryDto[];
}

export interface DocumentListDto {
  id: string;
  fileName: string;
  fileSize: number;
  status: DocumentStatus;
  uploadedAt: string;
  classifiedAt?: string;
  tags: string[];
}

export interface ClassificationResultDto {
  ruleId: string;
  ruleName: string;
  tagName: string;
  confidenceScore: number;
  appliedAt: string;
}

export interface TagDto {
  name: string;
  source: TagSource;
  addedAt: string;
  addedBy?: string;
}

export interface RoutingHistoryDto {
  fromQueueId?: string;
  fromQueueName?: string;
  toQueueId: string;
  toQueueName: string;
  routedAt: string;
  routedBy: string;
}

export interface PagedResultDto<T> {
  totalCount: number;
  items: T[];
}

// ========== Enums ==========

export enum DocumentStatus {
  Pending = 0,
  Classified = 1,
  Failed = 2,
  Routed = 3
}

export enum TagSource {
  Automatic = 0,
  Manual = 1
}

// ========== API Client Service ==========

export class DocumentsApiClient {
  constructor(private baseUrl: string, private getToken: () => string) {}

  async uploadDocument(file: File, fileName?: string, description?: string): Promise<DocumentDto> {
    const formData = new FormData();
    formData.append('File', file);
    if (fileName) formData.append('FileName', fileName);
    if (description) formData.append('Description', description);

    const response = await fetch(`${this.baseUrl}/api/documents/upload`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${this.getToken()}` },
      body: formData
    });

    if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    return response.json();
  }

  async uploadBatch(files: File[]): Promise<DocumentDto[]> {
    const formData = new FormData();
    files.forEach(file => formData.append('files', file));

    const response = await fetch(`${this.baseUrl}/api/documents/upload-batch`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${this.getToken()}` },
      body: formData
    });

    if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    return response.json();
  }

  async getDocument(id: string): Promise<DocumentDto> {
    const response = await fetch(`${this.baseUrl}/api/documents/${id}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${this.getToken()}`,
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    return response.json();
  }

  async getDocumentList(
    status?: DocumentStatus,
    uploadedAfter?: string,
    uploadedBefore?: string,
    skipCount: number = 0,
    maxResultCount: number = 10
  ): Promise<PagedResultDto<DocumentListDto>> {
    const params = new URLSearchParams();
    if (status !== undefined) params.append('status', status.toString());
    if (uploadedAfter) params.append('uploadedAfter', uploadedAfter);
    if (uploadedBefore) params.append('uploadedBefore', uploadedBefore);
    params.append('skipCount', skipCount.toString());
    params.append('maxResultCount', maxResultCount.toString());

    const response = await fetch(`${this.baseUrl}/api/documents?${params}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${this.getToken()}`,
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    return response.json();
  }

  async retryClassification(id: string): Promise<DocumentDto> {
    const response = await fetch(`${this.baseUrl}/api/documents/${id}/retry`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.getToken()}`,
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    return response.json();
  }

  async searchDocuments(criteria: DocumentSearchDto): Promise<PagedResultDto<DocumentListDto>> {
    const response = await fetch(`${this.baseUrl}/api/documents/search`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.getToken()}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(criteria)
    });

    if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    return response.json();
  }

  async addManualTag(id: string, tagName: string): Promise<DocumentDto> {
    const response = await fetch(`${this.baseUrl}/api/documents/${id}/tags`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.getToken()}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ tagName })
    });

    if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    return response.json();
  }

  async removeManualTag(id: string, tagName: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}/api/documents/${id}/tags/${tagName}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${this.getToken()}`,
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);
  }

  async getClassificationHistory(id: string): Promise<DocumentDto> {
    const response = await fetch(`${this.baseUrl}/api/documents/${id}/history`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${this.getToken()}`,
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    return response.json();
  }
}
```

---

**文件結束**
