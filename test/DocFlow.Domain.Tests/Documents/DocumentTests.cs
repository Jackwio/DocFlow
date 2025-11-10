using System;
using DocFlow.Documents;
using Shouldly;
using Xunit;

namespace DocFlow.Domain.Tests.Documents;

public class DocumentTests
{
    [Fact]
    public void Create_ShouldCreateDocument_WithPendingStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var fileName = "test.pdf";
        var filePath = "/documents/test.pdf";
        long fileSize = 1024;
        var contentType = "application/pdf";

        // Act
        var document = Document.Create(id, tenantId, fileName, filePath, fileSize, contentType);

        // Assert
        document.Id.ShouldBe(id);
        document.TenantId.ShouldBe(tenantId);
        document.FileName.ShouldBe(fileName);
        document.FilePath.ShouldBe(filePath);
        document.FileSize.ShouldBe(fileSize);
        document.ContentType.ShouldBe(contentType);
        document.Status.ShouldBe(DocumentStatus.Pending);
        document.RetryCount.ShouldBe(0);
    }

    [Fact]
    public void MarkAsClassifying_ShouldChangeStatus_WhenPending()
    {
        // Arrange
        var document = Document.Create(Guid.NewGuid(), Guid.NewGuid(), "test.pdf", "/path", 1024, "application/pdf");

        // Act
        document.MarkAsClassifying();

        // Assert
        document.Status.ShouldBe(DocumentStatus.Classifying);
    }

    [Fact]
    public void MarkAsClassifying_ShouldThrowException_WhenNotPendingOrFailed()
    {
        // Arrange
        var document = Document.Create(Guid.NewGuid(), Guid.NewGuid(), "test.pdf", "/path", 1024, "application/pdf");
        document.MarkAsClassifying();
        document.MarkAsClassified("Document", "/archive");

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => document.MarkAsClassifying());
    }

    [Fact]
    public void MarkAsClassified_ShouldSetClassificationAndRouting()
    {
        // Arrange
        var document = Document.Create(Guid.NewGuid(), Guid.NewGuid(), "test.pdf", "/path", 1024, "application/pdf");
        document.MarkAsClassifying();
        var classification = "Document";
        var routing = "/archive";

        // Act
        document.MarkAsClassified(classification, routing);

        // Assert
        document.Status.ShouldBe(DocumentStatus.Classified);
        document.Classification.ShouldBe(classification);
        document.RoutingDestination.ShouldBe(routing);
        document.RetryCount.ShouldBe(0);
    }

    [Fact]
    public void MarkAsFailed_ShouldIncrementRetryCount()
    {
        // Arrange
        var document = Document.Create(Guid.NewGuid(), Guid.NewGuid(), "test.pdf", "/path", 1024, "application/pdf");

        // Act
        document.MarkAsFailed();

        // Assert
        document.Status.ShouldBe(DocumentStatus.Failed);
        document.RetryCount.ShouldBe(1);
        document.LastRetryTime.ShouldNotBeNull();
    }

    [Fact]
    public void CanRetry_ShouldReturnTrue_WhenRetryCountBelowMax()
    {
        // Arrange
        var document = Document.Create(Guid.NewGuid(), Guid.NewGuid(), "test.pdf", "/path", 1024, "application/pdf");
        document.MarkAsFailed();

        // Act
        var canRetry = document.CanRetry(3);

        // Assert
        canRetry.ShouldBeTrue();
    }

    [Fact]
    public void CanRetry_ShouldReturnFalse_WhenRetryCountReachedMax()
    {
        // Arrange
        var document = Document.Create(Guid.NewGuid(), Guid.NewGuid(), "test.pdf", "/path", 1024, "application/pdf");
        document.MarkAsFailed();
        document.MarkAsFailed();
        document.MarkAsFailed();

        // Act
        var canRetry = document.CanRetry(3);

        // Assert
        canRetry.ShouldBeFalse();
    }

    [Fact]
    public void SendToDeadLetterQueue_ShouldSetStatusToDeadLetter()
    {
        // Arrange
        var document = Document.Create(Guid.NewGuid(), Guid.NewGuid(), "test.pdf", "/path", 1024, "application/pdf");

        // Act
        document.SendToDeadLetterQueue();

        // Assert
        document.Status.ShouldBe(DocumentStatus.DeadLetter);
    }

    [Fact]
    public void MarkAsExpired_ShouldSetStatusToExpired()
    {
        // Arrange
        var document = Document.Create(Guid.NewGuid(), Guid.NewGuid(), "test.pdf", "/path", 1024, "application/pdf");

        // Act
        document.MarkAsExpired();

        // Assert
        document.Status.ShouldBe(DocumentStatus.Expired);
    }
}
