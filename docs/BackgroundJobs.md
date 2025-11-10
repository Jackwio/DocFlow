# Background Jobs Documentation

This document describes the background jobs implemented in DocFlow and how to configure and use them.

## Overview

DocFlow implements 7 background jobs to handle various system tasks:

1. **Document Classification Job** - Classifies pending documents
2. **Failed Document Retry Job** - Retries failed classifications with DLQ
3. **Usage Reporting Job** - Reports tenant usage to billing service
4. **Document Cleanup Job** - Removes expired documents
5. **Quota Check Job** - Monitors and enforces quotas
6. **Tenant Suspension Job** - Suspends tenants after payment grace period
7. **Webhook Signature Job** - Signs and sends webhooks

## Architecture

All background jobs are implemented using ABP Framework's BackgroundJobs module, which provides:
- Automatic retry on failure
- Persistence of job state
- Distributed execution support
- Job scheduling

## Background Jobs Detail

### 1. Document Classification Job

**Purpose**: Processes pending documents and assigns classification and routing information.

**Configuration**:
```csharp
await _backgroundJobManager.EnqueueAsync(new DocumentClassificationJobArgs 
{ 
    BatchSize = 100 // Process up to 100 documents per run
});
```

**Execution Flow**:
1. Fetches pending documents from the database
2. Applies classification logic (currently simulated, ready for ML/AI integration)
3. Determines routing destination based on classification
4. Updates document status to 'Classified'

### 2. Failed Document Retry Job

**Purpose**: Retries failed classification operations with limited attempts, then moves to Dead Letter Queue (DLQ).

**Configuration**:
```csharp
await _backgroundJobManager.EnqueueAsync(new FailedDocumentRetryJobArgs 
{ 
    MaxRetries = 3,  // Maximum retry attempts
    BatchSize = 50   // Process up to 50 documents per run
});
```

**Execution Flow**:
1. Fetches documents with 'Failed' status
2. Checks retry count against maximum
3. Re-queues for classification if under limit
4. Moves to DLQ if max retries exceeded

### 3. Usage Reporting Job

**Purpose**: Collects and reports tenant usage metrics to the billing service.

**Configuration**:
```csharp
await _backgroundJobManager.EnqueueAsync(new UsageReportingJobArgs());
```

**Execution Flow**:
1. Iterates through all tenants
2. Calculates document count and storage usage
3. Sends report to billing service (integration point)

**Integration Point**: Implement `SendUsageReportToBillingService` method with actual API call.

### 4. Document Cleanup Job

**Purpose**: Removes documents that have exceeded their retention period.

**Configuration**:
```csharp
await _backgroundJobManager.EnqueueAsync(new DocumentCleanupJobArgs());
```

**Execution Flow**:
1. Queries documents with expired retention dates
2. Marks documents as 'Expired'
3. Deletes physical files from storage (integration point)
4. Logs audit events

**Integration Point**: Implement `DeleteFileFromStorage` method for actual file deletion.

### 5. Quota Check Job

**Purpose**: Monitors tenant quotas and blocks uploads when limits are exceeded.

**Configuration**:
```csharp
await _backgroundJobManager.EnqueueAsync(new QuotaCheckJobArgs());
```

**Execution Flow**:
1. Iterates through all tenants
2. Calculates current document count and storage usage
3. Updates quota records
4. Automatically blocks/unblocks based on quota status

### 6. Tenant Suspension Job

**Purpose**: Converts tenants to read-only mode after payment grace period expires.

**Configuration**:
```csharp
await _backgroundJobManager.EnqueueAsync(new TenantSuspensionJobArgs());
```

**Execution Flow**:
1. Queries tenants with expired grace periods
2. Changes billing status to 'ReadOnly'
3. Sends notification to tenant (integration point)

**Integration Point**: Implement `NotifyTenantAboutSuspension` for actual notifications.

### 7. Webhook Signature Job

**Purpose**: Generates HMAC signatures for webhooks and sends them to configured URLs.

**Configuration**:
```csharp
await _backgroundJobManager.EnqueueAsync(new WebhookSignatureJobArgs 
{ 
    BatchSize = 100 // Process up to 100 webhooks per run
});
```

**Webhook Secret Configuration** (appsettings.json):
```json
{
  "Webhook": {
    "Secret": "your-webhook-secret-key"
  }
}
```

**Execution Flow**:
1. Fetches pending webhooks
2. Generates HMAC-SHA256 signature using secret
3. Attaches signature to webhook
4. Sends HTTP POST to target URL (integration point)
5. Marks as sent or failed with retry support

**Integration Point**: Implement `SendWebhook` method with actual HTTP client call.

## Scheduling Background Jobs

To schedule background jobs to run periodically, you can use the `IBackgroundJobManager` in your application:

### Option 1: Using HostedService (Recommended)

Create a hosted service in your HttpApi.Host project:

```csharp
public class BackgroundJobScheduler : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundJobScheduler> _logger;

    public BackgroundJobScheduler(
        IServiceScopeFactory scopeFactory,
        ILogger<BackgroundJobScheduler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var backgroundJobManager = scope.ServiceProvider
                    .GetRequiredService<IBackgroundJobManager>();

                // Schedule jobs
                await backgroundJobManager.EnqueueAsync(
                    new DocumentClassificationJobArgs { BatchSize = 100 });
                
                await backgroundJobManager.EnqueueAsync(
                    new FailedDocumentRetryJobArgs { MaxRetries = 3, BatchSize = 50 });
                
                await backgroundJobManager.EnqueueAsync(
                    new QuotaCheckJobArgs());

                // Wait before next execution (e.g., every 5 minutes)
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling background jobs");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
```

Register the hosted service in your module:

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    context.Services.AddHostedService<BackgroundJobScheduler>();
}
```

### Option 2: Using Hangfire (Advanced)

For more advanced scheduling with cron expressions and dashboard:

1. Install Hangfire packages
2. Configure recurring jobs:

```csharp
RecurringJob.AddOrUpdate<DocumentClassificationJob>(
    "document-classification",
    job => job.ExecuteAsync(new DocumentClassificationJobArgs { BatchSize = 100 }),
    Cron.Minutely);

RecurringJob.AddOrUpdate<QuotaCheckJob>(
    "quota-check",
    job => job.ExecuteAsync(new QuotaCheckJobArgs()),
    Cron.Hourly);

RecurringJob.AddOrUpdate<DocumentCleanupJob>(
    "document-cleanup",
    job => job.ExecuteAsync(new DocumentCleanupJobArgs()),
    Cron.Daily);
```

## Monitoring and Troubleshooting

### Logging

All background jobs log their execution status using `ILogger`. Check application logs for:
- Job execution start/completion
- Processing statistics
- Errors and exceptions

### Database Tables

Background jobs use the following database tables:
- `AppDocuments` - Document records
- `AppTenantQuotas` - Tenant quota information
- `AppTenantBillingStatuses` - Tenant billing status
- `AppWebhookEvents` - Webhook events
- `AbpBackgroundJobs` - ABP job queue (managed by framework)

### Performance Considerations

- **Batch Size**: Adjust batch sizes based on system load
- **Scheduling Frequency**: Balance between responsiveness and system overhead
- **Database Indexes**: Ensure indexes are present on Status, TenantId, and date fields
- **Parallel Execution**: Jobs can run concurrently on different instances

## Security

### Webhook Signatures

Webhooks use HMAC-SHA256 for payload integrity:
- Secret key stored in configuration
- Signature sent in `X-Webhook-Signature` header (implementation dependent)
- Recipients can verify using the same secret

### Tenant Isolation

- All operations respect multi-tenancy boundaries
- Quota and billing operations are tenant-specific
- Document operations filter by TenantId

## Testing

### Unit Tests

Run domain unit tests:
```bash
dotnet test test/DocFlow.Domain.Tests/
```

57 unit tests cover all business logic and domain rules.

### Integration Tests

Run application integration tests:
```bash
dotnet test test/DocFlow.Application.Tests/
```

Tests validate background job execution with database.

## Migration

Apply the database migration to create required tables:

```bash
cd src/DocFlow.DbMigrator
dotnet run
```

Or using Entity Framework CLI:

```bash
cd src/DocFlow.EntityFrameworkCore
dotnet ef database update
```

## Future Enhancements

### Ready for Integration

The following areas are marked as integration points and ready for implementation:

1. **Document Classification**: Replace simulation with ML/AI service
2. **File Storage**: Implement actual file deletion in cleanup job
3. **Billing Service**: Implement HTTP client for usage reporting
4. **Webhook Delivery**: Implement HTTP client for webhook sending
5. **Tenant Notifications**: Implement email/notification service

### Potential Improvements

- Add job execution history and statistics
- Implement job priority levels
- Add configurable retry strategies
- Create admin dashboard for job monitoring
- Add support for scheduled (cron) job execution
