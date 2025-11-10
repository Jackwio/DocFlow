using System;
using System.Threading.Tasks;
using DocFlow.BackgroundJobs;
using DocFlow.Documents;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace DocFlow.Application.Tests.BackgroundJobs;

public class DocumentClassificationJobTests : DocFlowApplicationTestBase<DocFlowApplicationTestModule>
{
    private readonly DocumentClassificationJob _documentClassificationJob;
    private readonly IDocumentRepository _documentRepository;

    public DocumentClassificationJobTests()
    {
        _documentClassificationJob = GetRequiredService<DocumentClassificationJob>();
        _documentRepository = GetRequiredService<IDocumentRepository>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldClassifyPendingDocuments()
    {
        // Arrange
        var document = Document.Create(
            Guid.NewGuid(),
            null,
            "test.pdf",
            "/documents/test.pdf",
            1024,
            "application/pdf");

        await _documentRepository.InsertAsync(document);

        var args = new DocumentClassificationJobArgs { BatchSize = 10 };

        // Act
        await _documentClassificationJob.ExecuteAsync(args);

        // Assert
        var updatedDocument = await _documentRepository.GetAsync(document.Id);
        updatedDocument.Status.ShouldBe(DocumentStatus.Classified);
        updatedDocument.Classification.ShouldNotBeNullOrEmpty();
        updatedDocument.RoutingDestination.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleMultipleDocuments()
    {
        // Arrange
        var document1 = Document.Create(Guid.NewGuid(), null, "test1.pdf", "/path1", 1024, "application/pdf");
        var document2 = Document.Create(Guid.NewGuid(), null, "test2.jpg", "/path2", 2048, "image/jpeg");
        
        await _documentRepository.InsertAsync(document1);
        await _documentRepository.InsertAsync(document2);

        var args = new DocumentClassificationJobArgs { BatchSize = 10 };

        // Act
        await _documentClassificationJob.ExecuteAsync(args);

        // Assert
        var updatedDocument1 = await _documentRepository.GetAsync(document1.Id);
        var updatedDocument2 = await _documentRepository.GetAsync(document2.Id);
        
        updatedDocument1.Status.ShouldBe(DocumentStatus.Classified);
        updatedDocument2.Status.ShouldBe(DocumentStatus.Classified);
    }
}
