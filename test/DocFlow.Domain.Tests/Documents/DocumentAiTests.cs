using System;
using System.Collections.Generic;
using DocFlow.Documents;
using DocFlow.Enums;
using Xunit;

namespace DocFlow.Domain.Tests.Documents;

public class DocumentAiTests
{
    [Fact]
    public void Document_CanStoreAiSuggestion()
    {
        // Arrange
        var document = CreateTestDocument();
        var aiSuggestion = CreateTestAiSuggestion();

        // Act
        document.StoreAiSuggestion(aiSuggestion);

        // Assert
        Assert.NotNull(document.AiSuggestion);
        Assert.Equal(aiSuggestion, document.AiSuggestion);
    }

    [Fact]
    public void Document_CanApplyAiSuggestions()
    {
        // Arrange
        var document = CreateTestDocument();
        var aiSuggestion = CreateTestAiSuggestion();
        document.StoreAiSuggestion(aiSuggestion);

        // Act
        document.ApplyAiSuggestions();

        // Assert
        Assert.NotEmpty(document.Tags);
        Assert.Contains(document.Tags, t => t.Source == TagSource.AiApplied);
        Assert.Equal(DocumentStatus.Classified, document.Status);
    }

    [Fact]
    public void Document_ThrowsException_WhenApplyingWithoutSuggestions()
    {
        // Arrange
        var document = CreateTestDocument();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => document.ApplyAiSuggestions());
    }

    [Fact]
    public void Tag_CanCreateAiAppliedTag()
    {
        // Arrange
        var tagName = TagName.Create("Invoice");
        var confidence = ConfidenceScore.Create(0.95);

        // Act
        var tag = Tag.CreateAiApplied(tagName, confidence);

        // Assert
        Assert.Equal(TagSource.AiApplied, tag.Source);
        Assert.Equal(tagName, tag.Name);
        Assert.NotNull(tag.Confidence);
        Assert.Equal(0.95, tag.Confidence.Value);
    }

    [Fact]
    public void SuggestedTag_CanBeCreated()
    {
        // Arrange
        var tagName = TagName.Create("Contract");
        var confidence = ConfidenceScore.Create(0.88);
        var reasoning = "Document contains legal terminology";

        // Act
        var suggestedTag = SuggestedTag.Create(tagName, confidence, reasoning);

        // Assert
        Assert.Equal(tagName, suggestedTag.TagName);
        Assert.Equal(confidence, suggestedTag.Confidence);
        Assert.Equal(reasoning, suggestedTag.Reasoning);
    }

    [Fact]
    public void AiSuggestion_RequiresAtLeastOneTag()
    {
        // Arrange
        var emptyTags = new List<SuggestedTag>();
        var confidence = ConfidenceScore.Create(0.9);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            AiSuggestion.Create(emptyTags, null, confidence));
    }

    private Document CreateTestDocument()
    {
        return Document.RegisterUpload(
            Guid.NewGuid(),
            null,
            FileName.Create("test.pdf"),
            FileSize.Create(1024),
            MimeType.Create("application/pdf"),
            BlobReference.Create("documents", "test/test.pdf"));
    }

    private AiSuggestion CreateTestAiSuggestion()
    {
        var suggestedTags = new List<SuggestedTag>
        {
            SuggestedTag.Create(
                TagName.Create("Invoice"),
                ConfidenceScore.Create(0.95),
                "Contains invoice number and payment terms"),
            SuggestedTag.Create(
                TagName.Create("Accounting"),
                ConfidenceScore.Create(0.90),
                "Financial document")
        };

        return AiSuggestion.Create(
            suggestedTags,
            null,
            ConfidenceScore.Create(0.92),
            "This is a test invoice document");
    }
}
