using System;
using System.Threading.Tasks;
using DocFlow.BackgroundJobs;
using DocFlow.Documents;
using Shouldly;
using Xunit;

namespace DocFlow.Application.Tests.BackgroundJobs;

public class DocumentCleanupJobTests : DocFlowApplicationTestBase<DocFlowApplicationTestModule>
{
    private readonly DocumentCleanupJob _documentCleanupJob;
    private readonly IDocumentRepository _documentRepository;

    public DocumentCleanupJobTests()
    {
        _documentCleanupJob = GetRequiredService<DocumentCleanupJob>();
        _documentRepository = GetRequiredService<IDocumentRepository>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMarkExpiredDocuments()
    {
        // Arrange
        var expiredDocument = Document.Create(
            Guid.NewGuid(),
            null,
            "expired.pdf",
            "/documents/expired.pdf",
            1024,
            "application/pdf",
            DateTime.UtcNow.AddDays(-1)); // Already expired

        await _documentRepository.InsertAsync(expiredDocument);

        var args = new DocumentCleanupJobArgs();

        // Act
        await _documentCleanupJob.ExecuteAsync(args);

        // Assert
        var updatedDocument = await _documentRepository.GetAsync(expiredDocument.Id);
        updatedDocument.Status.ShouldBe(DocumentStatus.Expired);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotMarkNonExpiredDocuments()
    {
        // Arrange
        var validDocument = Document.Create(
            Guid.NewGuid(),
            null,
            "valid.pdf",
            "/documents/valid.pdf",
            1024,
            "application/pdf",
            DateTime.UtcNow.AddDays(30)); // Not expired

        await _documentRepository.InsertAsync(validDocument);

        var args = new DocumentCleanupJobArgs();

        // Act
        await _documentCleanupJob.ExecuteAsync(args);

        // Assert
        var updatedDocument = await _documentRepository.GetAsync(validDocument.Id);
        updatedDocument.Status.ShouldBe(DocumentStatus.Pending);
    }
}
