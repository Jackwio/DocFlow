using System;
using DocFlow.Documents;
using DocFlow.Enums;
using Shouldly;
using Xunit;

namespace DocFlow.Domain.Tests.Documents;

/// <summary>
/// Unit tests for Document aggregate with Inbox functionality.
/// </summary>
public sealed class DocumentInboxTests
{
    [Fact]
    public void RegisterUpload_WithInbox_ShouldCreateDocumentWithInbox()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var fileName = FileName.Create("test.pdf");
        var fileSize = FileSize.Create(1024);
        var mimeType = MimeType.Create("application/pdf");
        var blobReference = BlobReference.Create("documents", "test-blob");
        var inbox = InboxName.Create("Accounting");

        // Act
        var document = Document.RegisterUpload(id, tenantId, fileName, fileSize, mimeType, blobReference, inbox);

        // Assert
        document.ShouldNotBeNull();
        document.Id.ShouldBe(id);
        document.Inbox.ShouldNotBeNull();
        document.Inbox!.Value.ShouldBe("Accounting");
    }

    [Fact]
    public void RegisterUpload_WithoutInbox_ShouldCreateDocumentWithNullInbox()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var fileName = FileName.Create("test.pdf");
        var fileSize = FileSize.Create(1024);
        var mimeType = MimeType.Create("application/pdf");
        var blobReference = BlobReference.Create("documents", "test-blob");

        // Act
        var document = Document.RegisterUpload(id, tenantId, fileName, fileSize, mimeType, blobReference);

        // Assert
        document.ShouldNotBeNull();
        document.Inbox.ShouldBeNull();
    }

    [Fact]
    public void UpdateInbox_WithValidInbox_ShouldUpdateInbox()
    {
        // Arrange
        var document = CreateTestDocument();
        var newInbox = InboxName.Create("Legal");

        // Act
        document.UpdateInbox(newInbox);

        // Assert
        document.Inbox.ShouldNotBeNull();
        document.Inbox!.Value.ShouldBe("Legal");
    }

    [Fact]
    public void UpdateInbox_WithNull_ShouldSetInboxToNull()
    {
        // Arrange
        var inbox = InboxName.Create("HR");
        var document = CreateTestDocument(inbox);

        // Act
        document.UpdateInbox(null);

        // Assert
        document.Inbox.ShouldBeNull();
    }

    [Fact]
    public void UpdateInbox_MultipleTimes_ShouldUpdateEachTime()
    {
        // Arrange
        var document = CreateTestDocument();
        var inbox1 = InboxName.Create("Finance");
        var inbox2 = InboxName.Create("Operations");

        // Act
        document.UpdateInbox(inbox1);
        document.Inbox!.Value.ShouldBe("Finance");

        document.UpdateInbox(inbox2);
        document.Inbox!.Value.ShouldBe("Operations");

        document.UpdateInbox(null);
        document.Inbox.ShouldBeNull();

        // Assert - final state checked above
    }

    [Fact]
    public void Document_WithInbox_ShouldMaintainInboxThroughClassification()
    {
        // Arrange
        var inbox = InboxName.Create("Accounting");
        var document = CreateTestDocument(inbox);
        var tag = Tag.CreateAutomatic(TagName.Create("Invoice"));
        var historyEntry = ClassificationHistoryEntry.Create(
            Guid.NewGuid(),
            TagName.Create("Invoice"),
            "filename contains invoice",
            ConfidenceScore.Create(0.95)
        );

        // Act
        document.ApplyClassificationResult(new[] { tag }, new[] { historyEntry });

        // Assert
        document.Status.ShouldBe(DocumentStatus.Classified);
        document.Inbox.ShouldNotBeNull();
        document.Inbox!.Value.ShouldBe("Accounting");
    }

    private static Document CreateTestDocument(InboxName? inbox = null)
    {
        return Document.RegisterUpload(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FileName.Create("test.pdf"),
            FileSize.Create(1024),
            MimeType.Create("application/pdf"),
            BlobReference.Create("documents", "test-blob"),
            inbox
        );
    }
}
