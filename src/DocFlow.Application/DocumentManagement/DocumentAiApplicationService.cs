using System;
using System.Linq;
using System.Threading.Tasks;
using DocFlow.AiServices;
using DocFlow.Documents;
using DocFlow.Documents.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;

namespace DocFlow.DocumentManagement;

/// <summary>
/// Application service for AI-powered document operations.
/// Implements AI classification, tagging suggestions, and summarization.
/// </summary>
[Authorize]
public class DocumentAiApplicationService : ApplicationService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IAiClassificationService _aiClassificationService;
    private readonly PdfTextExtractionManager _textExtractionManager;
    private readonly IBlobContainer<DocFlowBlobContainer> _blobContainer;
    private readonly ILogger<DocumentAiApplicationService> _logger;

    public DocumentAiApplicationService(
        IDocumentRepository documentRepository,
        IAiClassificationService aiClassificationService,
        PdfTextExtractionManager textExtractionManager,
        IBlobContainer<DocFlowBlobContainer> blobContainer,
        ILogger<DocumentAiApplicationService> logger)
    {
        _documentRepository = documentRepository;
        _aiClassificationService = aiClassificationService;
        _textExtractionManager = textExtractionManager;
        _blobContainer = blobContainer;
        _logger = logger;
    }

    /// <summary>
    /// Generates AI suggestions for a document.
    /// </summary>
    public async Task<AiSuggestionDto> GenerateAiSuggestionsAsync(Guid documentId)
    {
        var document = await _documentRepository.GetAsync(documentId);

        // Check if AI is available
        if (!await _aiClassificationService.IsAvailableAsync())
        {
            throw new InvalidOperationException("AI service is not available. Please check configuration.");
        }

        // Extract text from document
        var blobStream = await _blobContainer.GetAsync(document.BlobReference.BlobName);
        var extractedText = await _textExtractionManager.ExtractTextAsync(blobStream);

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            throw new InvalidOperationException("Unable to extract text from document for AI analysis");
        }

        // Generate AI suggestions
        var aiSuggestion = await _aiClassificationService.GenerateSuggestionsAsync(
            extractedText,
            document.FileName.Value,
            document.MimeType.Value);

        // Store suggestions in document
        document.StoreAiSuggestion(aiSuggestion);
        await _documentRepository.UpdateAsync(document, autoSave: true);

        // Map to DTO
        return MapAiSuggestionToDto(aiSuggestion);
    }

    /// <summary>
    /// Applies AI suggestions to a document.
    /// </summary>
    public async Task<DocumentDto> ApplyAiSuggestionsAsync(ApplyAiSuggestionsDto input)
    {
        var document = await _documentRepository.GetAsync(input.DocumentId);

        if (document.AiSuggestion == null)
        {
            throw new InvalidOperationException("No AI suggestions available for this document. Generate suggestions first.");
        }

        // Apply AI suggestions
        document.ApplyAiSuggestions();
        await _documentRepository.UpdateAsync(document, autoSave: true);

        return ObjectMapper.Map<Document, DocumentDto>(document);
    }

    /// <summary>
    /// Generates a summary for a document.
    /// </summary>
    public async Task<DocumentSummaryDto> GenerateSummaryAsync(GenerateSummaryDto input)
    {
        var document = await _documentRepository.GetAsync(input.DocumentId);

        // Check if AI is available
        if (!await _aiClassificationService.IsAvailableAsync())
        {
            throw new InvalidOperationException("AI service is not available. Please check configuration.");
        }

        // Extract text from document
        var blobStream = await _blobContainer.GetAsync(document.BlobReference.BlobName);
        var extractedText = await _textExtractionManager.ExtractTextAsync(blobStream);

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            throw new InvalidOperationException("Unable to extract text from document for AI analysis");
        }

        // Generate summary
        var maxLength = input.MaxLength ?? 500;
        var summary = await _aiClassificationService.GenerateSummaryAsync(extractedText, maxLength);

        return new DocumentSummaryDto
        {
            DocumentId = document.Id,
            Summary = summary,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Checks if AI service is enabled and available for the current tenant.
    /// </summary>
    public async Task<bool> IsAiEnabledAsync()
    {
        // For now, just check if AI service is configured
        // In future, this could check tenant-specific settings
        return await _aiClassificationService.IsAvailableAsync();
    }

    private AiSuggestionDto MapAiSuggestionToDto(AiSuggestion aiSuggestion)
    {
        return new AiSuggestionDto
        {
            SuggestedTags = aiSuggestion.SuggestedTags
                .Select(t => new SuggestedTagDto
                {
                    TagName = t.TagName.Value,
                    Confidence = t.Confidence.Value,
                    Reasoning = t.Reasoning
                })
                .ToList(),
            SuggestedQueueId = aiSuggestion.SuggestedQueueId,
            Confidence = aiSuggestion.Confidence.Value,
            Summary = aiSuggestion.Summary,
            GeneratedAt = aiSuggestion.GeneratedAt
        };
    }
}
