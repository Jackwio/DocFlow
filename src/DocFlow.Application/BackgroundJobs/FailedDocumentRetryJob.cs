using System;
using System.Threading.Tasks;
using DocFlow.Documents;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace DocFlow.BackgroundJobs;

public class FailedDocumentRetryJob : AsyncBackgroundJob<FailedDocumentRetryJobArgs>, ITransientDependency
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly ILogger<FailedDocumentRetryJob> _logger;

    public FailedDocumentRetryJob(
        IDocumentRepository documentRepository,
        IBackgroundJobManager backgroundJobManager,
        ILogger<FailedDocumentRetryJob> logger)
    {
        _documentRepository = documentRepository;
        _backgroundJobManager = backgroundJobManager;
        _logger = logger;
    }

    [UnitOfWork]
    public override async Task ExecuteAsync(FailedDocumentRetryJobArgs args)
    {
        try
        {
            var documents = await _documentRepository.GetFailedDocumentsForRetryAsync(args.MaxRetries, args.BatchSize);

            foreach (var document in documents)
            {
                if (document.CanRetry(args.MaxRetries))
                {
                    _logger.LogInformation(
                        "Retrying classification for document {DocumentId} (Attempt {RetryCount}/{MaxRetries})",
                        document.Id, document.RetryCount + 1, args.MaxRetries);

                    // Queue document for classification retry
                    await _backgroundJobManager.EnqueueAsync(new DocumentClassificationJobArgs { BatchSize = 1 });
                }
                else
                {
                    // Max retries exceeded, send to DLQ
                    document.SendToDeadLetterQueue();
                    await _documentRepository.UpdateAsync(document);
                    
                    _logger.LogWarning(
                        "Document {DocumentId} sent to Dead Letter Queue after {RetryCount} failed attempts",
                        document.Id, document.RetryCount);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing failed document retry job");
            throw;
        }
    }
}

public class FailedDocumentRetryJobArgs
{
    public int MaxRetries { get; set; } = 3;
    public int BatchSize { get; set; } = 50;
}
