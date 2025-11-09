# Quickstart Guide: Document Intake & Classification System

**Feature**: 001-document-intake-classification  
**Branch**: `001-document-intake-classification`  
**Last Updated**: 2025-11-09

This guide walks you through setting up a local development environment for the Document Intake & Classification feature.

---

## Prerequisites

Ensure the following tools are installed on your development machine:

### Required Software

1. **.NET 8 SDK** (8.0.100 or later)
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation: `dotnet --version`

2. **PostgreSQL 16** (or later)
   - Download: https://www.postgresql.org/download/
   - Verify installation: `psql --version`
   - **Important**: Install **pgvector extension** (optional, for future AI features)

3. **Redis 7+**
   - Download: https://redis.io/download
   - Windows: Use Redis for Windows or Docker
   - Verify installation: `redis-cli --version`

4. **Git**
   - Download: https://git-scm.com/downloads
   - Verify installation: `git --version`

5. **Visual Studio 2022** (17.8+) OR **Visual Studio Code** with C# extension
   - Visual Studio: https://visualstudio.microsoft.com/downloads/
   - VS Code: https://code.visualstudio.com/ + C# Dev Kit extension

### Optional (Recommended)

- **Docker Desktop** (for running PostgreSQL and Redis in containers)
  - Download: https://www.docker.com/products/docker-desktop
- **Postman** or **Insomnia** (for API testing)
- **pgAdmin 4** (PostgreSQL GUI, included with PostgreSQL installer)

---

## Step 1: Clone Repository & Checkout Feature Branch

```powershell
# Navigate to your workspace
cd d:\GitHub\DocFlow

# Ensure you're in the aspnet-core directory
cd aspnet-core

# Fetch latest changes
git fetch origin

# Checkout the feature branch
git checkout 001-document-intake-classification

# Verify you're on the correct branch
git branch --show-current
# Expected output: 001-document-intake-classification
```

---

## Step 2: Start PostgreSQL & Redis

### Option A: Using Docker (Recommended)

Create a `docker-compose.yml` file in the project root:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    container_name: docflow-postgres
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: DocFlow
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    container_name: docflow-redis
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data

volumes:
  postgres-data:
  redis-data:
```

Start the containers:

```powershell
docker-compose up -d

# Verify containers are running
docker ps
```

### Option B: Using Local Installations

**PostgreSQL:**
```powershell
# Start PostgreSQL service (Windows)
Start-Service postgresql-x64-16

# Or using pg_ctl
pg_ctl -D "C:\Program Files\PostgreSQL\16\data" start
```

**Redis:**
```powershell
# Start Redis server (Windows)
redis-server

# Or run as Windows service
redis-server --service-start
```

---

## Step 3: Configure Application Settings

### 3.1 Update `appsettings.json`

Navigate to `src/DocFlow.HttpApi.Host/appsettings.json` and update:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=DocFlow;Username=postgres;Password=postgres;Include Error Detail=true"
  },
  "Redis": {
    "Configuration": "localhost:6379",
    "InstanceName": "DocFlow:"
  },
  "BackgroundJobs": {
    "Hangfire": {
      "DashboardEnabled": true,
      "ServerEnabled": true,
      "WorkerCount": 5
    }
  },
  "BlobStoring": {
    "Provider": "FileSystem",
    "FileSystem": {
      "BasePath": "C:\\DocFlow\\BlobStorage"
    }
  },
  "App": {
    "SelfUrl": "https://localhost:44300",
    "CorsOrigins": "https://localhost:44300",
    "MaxUploadFileSizeBytes": 52428800
  }
}
```

### 3.2 Create Blob Storage Directory

```powershell
# Create blob storage directory
New-Item -ItemType Directory -Path "C:\DocFlow\BlobStorage" -Force
```

### 3.3 (Optional) Configure Secrets for Sensitive Data

For production-like environments, use User Secrets:

```powershell
cd src\DocFlow.HttpApi.Host

# Initialize user secrets
dotnet user-secrets init

# Set connection string
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=DocFlow;Username=postgres;Password=YOUR_SECURE_PASSWORD"

# Set Redis connection
dotnet user-secrets set "Redis:Configuration" "localhost:6379"
```

---

## Step 4: Restore Dependencies & Build Solution

```powershell
# Navigate to solution root
cd d:\GitHub\DocFlow\aspnet-core

# Restore NuGet packages
dotnet restore DocFlow.sln

# Build the entire solution
dotnet build DocFlow.sln --configuration Debug

# Verify build succeeded
# Expected: Build succeeded. 0 Warning(s). 0 Error(s).
```

---

## Step 5: Run Database Migrations

### 5.1 Apply Initial ABP Migrations

```powershell
# Navigate to DbMigrator project
cd src\DocFlow.DbMigrator

# Run migrations
dotnet run

# Expected output:
# [INFO] Successfully applied X migrations
# [INFO] Seeded initial data
# [INFO] Database migration completed successfully
```

### 5.2 (Optional) Verify Database Schema

Connect to PostgreSQL using pgAdmin or psql:

```powershell
psql -U postgres -d DocFlow -h localhost

# List tables
\dt

# Expected tables (ABP framework + DocFlow):
# - AbpUsers
# - AbpTenants
# - Documents
# - ClassificationRules
# - RoutingQueues
# - WebhookDeliveries
# - DocumentTags
# - DocumentClassificationHistory
# - ... (other ABP tables)

# Exit psql
\q
```

---

## Step 6: Seed Initial Test Data

### 6.1 Create a Seeder Script (Optional)

Create `src/DocFlow.Domain/Data/DocumentIntakeDataSeeder.cs`:

```csharp
public class DocumentIntakeDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Inbox, InboxId> _inboxRepository;
    private readonly IRepository<RoutingQueue, RoutingQueueId> _queueRepository;
    
    public DocumentIntakeDataSeeder(
        IRepository<Inbox, InboxId> inboxRepository,
        IRepository<RoutingQueue, RoutingQueueId> queueRepository)
    {
        _inboxRepository = inboxRepository;
        _queueRepository = queueRepository;
    }
    
    public async Task SeedAsync(DataSeedContext context)
    {
        // Seed inboxes
        if (await _inboxRepository.CountAsync() == 0)
        {
            await _inboxRepository.InsertAsync(
                Inbox.Create(InboxId.NewId(), InboxName.Create("Accounting Inbox"), null),
                autoSave: true);
            
            await _inboxRepository.InsertAsync(
                Inbox.Create(InboxId.NewId(), InboxName.Create("Legal Inbox"), null),
                autoSave: true);
        }
        
        // Seed routing queues
        if (await _queueRepository.CountAsync() == 0)
        {
            await _queueRepository.InsertAsync(
                RoutingQueue.CreateFolderQueue(
                    RoutingQueueId.NewId(),
                    QueueName.Create("Accounting Folder"),
                    QueueDescription.Create("File-based routing for accounting documents"),
                    FolderPath.Create("C:\\DocFlow\\Routes\\Accounting"),
                    null),
                autoSave: true);
        }
    }
}
```

Run seeder again:

```powershell
cd src\DocFlow.DbMigrator
dotnet run
```

---

## Step 7: Run the Application

### 7.1 Start the HTTP API Host

```powershell
# Navigate to HTTP API Host project
cd src\DocFlow.HttpApi.Host

# Run the application
dotnet run

# Expected output:
# [INFO] Starting DocFlow.HttpApi.Host...
# [INFO] Now listening on: https://localhost:44300
# [INFO] Application started. Press Ctrl+C to shut down.
```

### 7.2 Verify Health Checks

Open a browser or use curl:

```powershell
# Check health endpoint
Invoke-RestMethod -Uri https://localhost:44300/health/ready

# Expected output:
# Status: Healthy
# TotalDuration: 00:00:00.150
```

### 7.3 Access Hangfire Dashboard

Navigate to: https://localhost:44300/hangfire

- **Default credentials**: (configured in ABP authorization)
- View background jobs, queues, and job history

---

## Step 8: Test Document Upload API

### 8.1 Obtain JWT Token (Authentication)

Use Postman or curl to authenticate:

```powershell
# Request token from ABP authentication endpoint
$body = @{
    username = "admin"
    password = "1q2w3E*"
    client_id = "DocFlow_App"
    grant_type = "password"
    scope = "DocFlow"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:44300/connect/token" `
    -Method POST `
    -ContentType "application/json" `
    -Body $body

$token = $response.access_token
Write-Host "Access Token: $token"
```

### 8.2 Upload a Test Document

Create a test PDF file or use an existing one:

```powershell
# Upload document using API
$headers = @{
    "Authorization" = "Bearer $token"
}

$filePath = "C:\TestDocuments\invoice_sample.pdf"
$inboxId = "INBOX_GUID_HERE" # Replace with actual inbox ID from database

$form = @{
    file = Get-Item -Path $filePath
    inboxId = $inboxId
}

$uploadResponse = Invoke-RestMethod -Uri "https://localhost:44300/api/documents/upload" `
    -Method POST `
    -Headers $headers `
    -Form $form

Write-Host "Document uploaded successfully!"
Write-Host "Document ID: $($uploadResponse.id)"
Write-Host "Status: $($uploadResponse.status)"
```

### 8.3 Verify Document in Database

```powershell
psql -U postgres -d DocFlow -h localhost -c "SELECT \"Id\", \"FileName\", \"Status\" FROM \"Documents\";"
```

Expected output:
```
                  Id                  |       FileName        | Status
--------------------------------------+-----------------------+--------
 3fa85f64-5717-4562-b3fc-2c963f66afa6 | invoice_sample.pdf    | Pending
```

---

## Step 9: Run Automated Tests

### 9.1 Run Unit Tests

```powershell
cd test\DocFlow.Domain.Tests
dotnet test --configuration Debug --verbosity normal

# Expected: All tests pass
```

### 9.2 Run Integration Tests

```powershell
cd test\DocFlow.EntityFrameworkCore.Tests
dotnet test --configuration Debug --verbosity normal

# Note: Integration tests use Testcontainers for PostgreSQL
# Docker Desktop must be running
```

### 9.3 Run Application Tests

```powershell
cd test\DocFlow.Application.Tests
dotnet test --configuration Debug --verbosity normal
```

### 9.4 View Test Coverage (Optional)

```powershell
# Install coverage tool
dotnet tool install --global coverlet.console

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generate HTML report using ReportGenerator
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator -reports:**/coverage.opencover.xml -targetdir:./coverage-report

# Open coverage-report/index.html in browser
```

---

## Step 10: Access Swagger API Documentation

Navigate to: https://localhost:44300/swagger

- Explore all available API endpoints
- Test APIs directly from Swagger UI
- View request/response schemas

**Endpoints available:**
- `POST /api/documents/upload` - Upload single document
- `POST /api/documents/batch-upload` - Upload multiple documents
- `GET /api/documents` - List documents with filtering
- `POST /api/documents/{id}/retry` - Retry failed document
- `GET /api/rules` - List classification rules
- `POST /api/rules` - Create classification rule
- `POST /api/rules/{id}/dry-run` - Test rule against document
- `GET /api/queues` - List routing queues
- `GET /api/webhooks/deliveries` - List webhook deliveries
- `GET /api/documents/search` - Advanced document search

---

## Troubleshooting

### Issue: "Unable to connect to PostgreSQL"

**Solution:**
```powershell
# Check if PostgreSQL is running
Get-Service postgresql-x64-16

# Or check Docker container
docker ps | Select-String postgres

# Test connection manually
psql -U postgres -h localhost -c "SELECT version();"
```

### Issue: "Redis connection timeout"

**Solution:**
```powershell
# Check if Redis is running
redis-cli ping
# Expected: PONG

# Or check Docker container
docker ps | Select-String redis
```

### Issue: "File upload fails with 'File too large'"

**Solution:**
Update `appsettings.json`:
```json
{
  "App": {
    "MaxUploadFileSizeBytes": 52428800
  }
}
```

And configure Kestrel limits in `Program.cs`:
```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 52428800; // 50MB
});
```

### Issue: "Hangfire dashboard shows 'No jobs found'"

**Solution:**
- Classification jobs are triggered after document upload
- Upload a document first, then check Hangfire dashboard
- Verify Hangfire is configured in `appsettings.json`:
  ```json
  {
    "BackgroundJobs": {
      "Hangfire": {
        "ServerEnabled": true
      }
    }
  }
  ```

### Issue: "Migration fails with 'relation already exists'"

**Solution:**
```powershell
# Drop and recreate database
psql -U postgres -h localhost -c "DROP DATABASE IF EXISTS \"DocFlow\";"
psql -U postgres -h localhost -c "CREATE DATABASE \"DocFlow\";"

# Re-run migrations
cd src\DocFlow.DbMigrator
dotnet run
```

### Issue: "Build fails with 'The type or namespace name could not be found'"

**Solution:**
```powershell
# Clean solution
dotnet clean DocFlow.sln

# Restore NuGet packages
dotnet restore DocFlow.sln

# Rebuild
dotnet build DocFlow.sln --configuration Debug
```

---

## Next Steps

After successfully running the application locally:

1. **Create Classification Rules**:
   - Use `POST /api/rules` to create a rule that matches invoice filenames
   - Test the rule with `POST /api/rules/{id}/dry-run`

2. **Upload Test Documents**:
   - Upload PDFs with different filenames
   - Watch Hangfire dashboard for classification jobs
   - Verify documents are classified and routed

3. **Explore Webhooks**:
   - Create a webhook queue pointing to a test endpoint (use webhook.site)
   - Upload a document that routes to the webhook queue
   - View webhook delivery records in `GET /api/webhooks/deliveries`

4. **Test Search**:
   - Use `GET /api/documents/search` with various filters
   - Try tag filtering, date ranges, and full-text search

5. **Review Logs**:
   - Check console output for structured logs with correlation IDs
   - Logs include: document upload, classification, routing, webhook delivery

---

## Environment Variables Reference

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Development` | Environment name (Development, Staging, Production) |
| `DOTNET_ENVIRONMENT` | `Development` | .NET environment name |
| `ConnectionStrings__Default` | (see appsettings.json) | PostgreSQL connection string |
| `Redis__Configuration` | `localhost:6379` | Redis connection string |
| `App__SelfUrl` | `https://localhost:44300` | Application base URL |
| `App__MaxUploadFileSizeBytes` | `52428800` | Max file upload size (50MB) |

---

## Useful Commands Summary

```powershell
# Start Docker services
docker-compose up -d

# Run migrations
cd src\DocFlow.DbMigrator; dotnet run

# Start API
cd src\DocFlow.HttpApi.Host; dotnet run

# Run all tests
dotnet test DocFlow.sln

# Watch mode (auto-restart on file changes)
cd src\DocFlow.HttpApi.Host
dotnet watch run

# Check database
psql -U postgres -d DocFlow -h localhost

# Check Redis cache
redis-cli KEYS "DocFlow:*"

# View logs
Get-Content src\DocFlow.HttpApi.Host\Logs\log-*.txt -Tail 50 -Wait
```

---

## Additional Resources

- **ABP Framework Documentation**: https://docs.abp.io/
- **DocFlow Project README**: `../../README.md`
- **Feature Specification**: `spec.md`
- **Data Model**: `data-model.md`
- **Research Document**: `research.md`
- **API Contracts**: `contracts/*.yaml`
- **Constitution**: `.specify/memory/constitution.md`

---

## Support

If you encounter issues not covered in this guide:

1. Check the **Troubleshooting** section above
2. Review **ABP Framework logs** in `src/DocFlow.HttpApi.Host/Logs/`
3. Consult the **data-model.md** for entity relationships
4. Review the **research.md** for technical decisions
5. Contact the development team

---

**Congratulations!** You now have a fully functional local development environment for the Document Intake & Classification system. 🎉
