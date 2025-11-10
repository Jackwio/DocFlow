using System;
using System.Threading.Tasks;
using DocFlow.DocumentManagement;
using DocFlow.Documents.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace DocFlow.Controllers;

/// <summary>
/// REST API controller for AI-powered document operations.
/// Provides endpoints for AI classification, tagging suggestions, and summarization.
/// </summary>
[ApiController]
[Route("api/documents/ai")]
public class DocumentAiController : AbpControllerBase
{
    private readonly DocumentAiApplicationService _documentAiService;

    public DocumentAiController(DocumentAiApplicationService documentAiService)
    {
        _documentAiService = documentAiService;
    }

    /// <summary>
    /// Generate AI-powered classification suggestions for a document.
    /// Returns suggested tags, queue recommendations, and confidence scores.
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <returns>AI-generated suggestions</returns>
    [HttpPost]
    [Route("{documentId}/suggestions")]
    public async Task<AiSuggestionDto> GenerateSuggestionsAsync(Guid documentId)
    {
        return await _documentAiService.GenerateAiSuggestionsAsync(documentId);
    }

    /// <summary>
    /// Apply AI suggestions to a document (one-click apply).
    /// Converts AI-suggested tags to actual applied tags.
    /// </summary>
    /// <param name="input">Apply suggestions request</param>
    /// <returns>Updated document with applied tags</returns>
    [HttpPost]
    [Route("apply-suggestions")]
    public async Task<DocumentDto> ApplySuggestionsAsync([FromBody] ApplyAiSuggestionsDto input)
    {
        return await _documentAiService.ApplyAiSuggestionsAsync(input);
    }

    /// <summary>
    /// Generate a brief summary of a document using AI.
    /// </summary>
    /// <param name="input">Summary generation request</param>
    /// <returns>Document summary</returns>
    [HttpPost]
    [Route("summary")]
    public async Task<DocumentSummaryDto> GenerateSummaryAsync([FromBody] GenerateSummaryDto input)
    {
        return await _documentAiService.GenerateSummaryAsync(input);
    }

    /// <summary>
    /// Check if AI service is enabled and available for the current tenant.
    /// </summary>
    /// <returns>True if AI is enabled; otherwise, false</returns>
    [HttpGet]
    [Route("status")]
    public async Task<bool> IsAiEnabledAsync()
    {
        return await _documentAiService.IsAiEnabledAsync();
    }
}
