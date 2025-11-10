using System;
using System.Threading.Tasks;
using DocFlow.Documents;
using Microsoft.Extensions.Logging;
using Volo.Abp.AuditLogging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace DocFlow.BackgroundJobs;

public class DocumentCleanupJob : AsyncBackgroundJob<DocumentCleanupJobArgs>, ITransientDependency
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<DocumentCleanupJob> _logger;

    public DocumentCleanupJob(
        IDocumentRepository documentRepository,
        IAuditLogRepository auditLogRepository,
        ILogger<DocumentCleanupJob> logger)
    {
        _documentRepository = documentRepository;
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    [UnitOfWork]
    public override async Task ExecuteAsync(DocumentCleanupJobArgs args)
    {
        try
        {
            var expiredDocuments = await _documentRepository.GetExpiredDocumentsAsync();

            _logger.LogInformation("Found {Count} expired documents to clean up", expiredDocuments.Count);

            foreach (var document in expiredDocuments)
            {
                try
                {
                    document.MarkAsExpired();
                    await _documentRepository.UpdateAsync(document);

                    // TODO: Delete actual file from storage
                    await DeleteFileFromStorage(document.FilePath);

                    // Log audit event
                    _logger.LogInformation(
                        "Document {DocumentId} (File: {FileName}) deleted due to retention policy expiry",
                        document.Id, document.FileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to clean up document {DocumentId}", document.Id);
                }
            }

            _logger.LogInformation("Document cleanup job completed. Processed {Count} documents", expiredDocuments.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing document cleanup job");
            throw;
        }
    }

    private Task DeleteFileFromStorage(string filePath)
    {
        // Placeholder for actual file deletion logic
        // In production, this would delete the file from blob storage, file system, etc.
        _logger.LogDebug("Deleting file from storage: {FilePath}", filePath);
        return Task.CompletedTask;
    }
}

public class DocumentCleanupJobArgs
{
    // No specific arguments needed for now
}
