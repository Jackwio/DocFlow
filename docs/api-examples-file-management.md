# Document Management API Examples

This document provides examples of how to use the file-related APIs implemented for the DocFlow system.

## Base URL
```
https://your-domain.com/api/documents
```

## Authentication
All endpoints require authentication. Include the JWT token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

---

## 1. Upload Statistics

Get upload statistics for the current user showing file counts and storage usage.

### Endpoint
```
GET /api/documents/statistics
```

### Request
```bash
curl -X GET "https://your-domain.com/api/documents/statistics" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Response
```json
{
  "filesToday": 24,
  "filesThisWeek": 156,
  "filesTotal": 2847,
  "storageUsedBytes": 26336215040,
  "storageQuotaBytes": 107374182400,
  "storageUsedGB": 24.52,
  "storageQuotaGB": 100.0,
  "storageUsagePercent": 24.52
}
```

### Response Fields
- `filesToday`: Number of files uploaded today
- `filesThisWeek`: Number of files uploaded this week (starting Sunday)
- `filesTotal`: Total number of files uploaded by this user
- `storageUsedBytes`: Total storage used in bytes
- `storageQuotaBytes`: Maximum storage quota in bytes (100 GB default)
- `storageUsedGB`: Storage used in gigabytes (calculated)
- `storageQuotaGB`: Storage quota in gigabytes (calculated)
- `storageUsagePercent`: Percentage of storage used (0-100)

---

## 2. Recent Uploads

Get documents uploaded within the last 24 hours, ordered by upload time (newest first).

### Endpoint
```
GET /api/documents/recent?maxResults={number}
```

### Parameters
- `maxResults` (optional): Maximum number of results to return. Default: 50, Max: 100

### Request
```bash
curl -X GET "https://your-domain.com/api/documents/recent?maxResults=10" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Response
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fileName": "Contract_2025.pdf",
    "fileSizeBytes": 2518016,
    "fileSizeMB": 2.4,
    "uploadedAt": "2025-11-11T00:58:00Z",
    "minutesAgo": 2,
    "isProcessed": true
  },
  {
    "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "fileName": "Invoice_Q4.xlsx",
    "fileSizeBytes": 921600,
    "fileSizeMB": 0.89,
    "uploadedAt": "2025-11-11T00:55:00Z",
    "minutesAgo": 5,
    "isProcessed": true
  },
  {
    "id": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
    "fileName": "Presentation.pptx",
    "fileSizeBytes": 5452800,
    "fileSizeMB": 5.2,
    "uploadedAt": "2025-11-11T00:50:00Z",
    "minutesAgo": 10,
    "isProcessed": false
  },
  {
    "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
    "fileName": "Report_Final.docx",
    "fileSizeBytes": 1887436,
    "fileSizeMB": 1.8,
    "uploadedAt": "2025-11-11T00:45:00Z",
    "minutesAgo": 15,
    "isProcessed": true
  }
]
```

### Response Fields
- `id`: Document unique identifier
- `fileName`: Original file name
- `fileSizeBytes`: File size in bytes
- `fileSizeMB`: File size in megabytes (calculated)
- `uploadedAt`: Upload timestamp (UTC)
- `minutesAgo`: Minutes elapsed since upload (calculated)
- `isProcessed`: Whether the document has been processed (classified)

---

## 3. Upload Document

Upload a new document to the system.

### Endpoint
```
POST /api/documents/upload
```

### Request (multipart/form-data)
```bash
curl -X POST "https://your-domain.com/api/documents/upload" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@/path/to/document.pdf" \
  -F "fileName=My Document.pdf" \
  -F "description=Optional description"
```

### Form Fields
- `file`: The document file (required)
- `fileName`: Custom file name (optional, uses original if not provided)
- `description`: Document description (optional)

### Response
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "My Document.pdf",
  "fileSizeBytes": 2518016,
  "mimeType": "application/pdf",
  "status": "Pending",
  "tags": [],
  "lastError": null,
  "routedToQueueId": null,
  "creationTime": "2025-11-11T01:00:00Z"
}
```

### Supported File Types
- PDF: `application/pdf`
- PNG: `image/png`
- JPEG: `image/jpeg`
- TIFF: `image/tiff`

### File Size Limits
- Maximum file size: 50 MB
- Storage quota: 100 GB per user

---

## 4. List Documents

Get a paginated list of documents with optional filters.

### Endpoint
```
GET /api/documents?status={status}&uploadedAfter={date}&uploadedBefore={date}&skipCount={number}&maxResultCount={number}
```

### Parameters
- `status` (optional): Filter by document status (`Pending`, `Classified`, `Routed`, `Failed`)
- `uploadedAfter` (optional): Filter documents uploaded after this date (ISO 8601 format)
- `uploadedBefore` (optional): Filter documents uploaded before this date
- `skipCount` (optional): Number of items to skip for pagination. Default: 0
- `maxResultCount` (optional): Maximum number of items to return. Default: 10, Max: 100

### Request
```bash
curl -X GET "https://your-domain.com/api/documents?status=Classified&maxResultCount=20" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Response
```json
{
  "totalCount": 156,
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fileName": "Contract_2025.pdf",
      "fileSizeBytes": 2518016,
      "status": "Classified",
      "tagCount": 3,
      "creationTime": "2025-11-11T00:58:00Z"
    }
  ]
}
```

---

## 5. Search Documents

Advanced search with multiple filter criteria.

### Endpoint
```
POST /api/documents/search
```

### Request Body
```json
{
  "status": "Classified",
  "tags": ["Invoice", "Finance"],
  "fileNameContains": "2025",
  "uploadedAfter": "2025-01-01T00:00:00Z",
  "uploadedBefore": "2025-12-31T23:59:59Z",
  "skipCount": 0,
  "maxResults": 20
}
```

### Request
```bash
curl -X POST "https://your-domain.com/api/documents/search" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Classified",
    "tags": ["Invoice"],
    "fileNameContains": "Q4",
    "maxResults": 10
  }'
```

### Response
Same format as List Documents endpoint.

---

## 6. Get Document Details

Get detailed information about a specific document.

### Endpoint
```
GET /api/documents/{id}
```

### Request
```bash
curl -X GET "https://your-domain.com/api/documents/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Response
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "Contract_2025.pdf",
  "fileSizeBytes": 2518016,
  "mimeType": "application/pdf",
  "status": "Routed",
  "tags": ["Contract", "Legal", "2025"],
  "lastError": null,
  "routedToQueueId": "7fa85f64-5717-4562-b3fc-2c963f66afb1",
  "creationTime": "2025-11-11T00:58:00Z"
}
```

---

## 7. Retry Failed Document

Retry classification for a failed document.

### Endpoint
```
POST /api/documents/{id}/retry
```

### Request
```bash
curl -X POST "https://your-domain.com/api/documents/3fa85f64-5717-4562-b3fc-2c963f66afa6/retry" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Response
Returns updated document details with status reset to `Pending`.

---

## 8. Add Manual Tag

Add a manual tag to a document.

### Endpoint
```
POST /api/documents/{id}/tags
```

### Request Body
```json
{
  "tagName": "Important"
}
```

### Request
```bash
curl -X POST "https://your-domain.com/api/documents/3fa85f64-5717-4562-b3fc-2c963f66afa6/tags" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"tagName": "Important"}'
```

### Response
Returns updated document details with the new tag added.

---

## 9. Remove Manual Tag

Remove a manual tag from a document.

### Endpoint
```
DELETE /api/documents/{id}/tags/{tagName}
```

### Request
```bash
curl -X DELETE "https://your-domain.com/api/documents/3fa85f64-5717-4562-b3fc-2c963f66afa6/tags/Important" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Response
Returns 204 No Content on success.

---

## Error Responses

All endpoints may return the following error responses:

### 400 Bad Request
```json
{
  "error": {
    "code": "ValidationError",
    "message": "File size exceeds maximum allowed size of 50MB",
    "details": null
  }
}
```

### 401 Unauthorized
```json
{
  "error": {
    "code": "Unauthorized",
    "message": "Authentication failed",
    "details": null
  }
}
```

### 404 Not Found
```json
{
  "error": {
    "code": "NotFound",
    "message": "Document not found",
    "details": null
  }
}
```

### 500 Internal Server Error
```json
{
  "error": {
    "code": "InternalServerError",
    "message": "An internal error occurred",
    "details": null
  }
}
```

---

## Rate Limiting

API requests may be rate-limited. Check the following response headers:
- `X-RateLimit-Limit`: Maximum requests per window
- `X-RateLimit-Remaining`: Remaining requests in current window
- `X-RateLimit-Reset`: Time when the rate limit resets

---

## Notes

1. All timestamps are in UTC (ISO 8601 format).
2. File sizes are provided in both bytes and human-readable formats (MB/GB).
3. The system maintains tenant isolation - users can only access their own documents.
4. Document classification and routing happen asynchronously after upload.
5. Storage quota enforcement happens at upload time - uploads exceeding quota will be rejected.
