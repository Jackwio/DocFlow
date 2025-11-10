using System;
using System.Threading.Tasks;
using DocFlow.Documents;

namespace DocFlow.AiServices;

/// <summary>
/// Service interface for AI-powered document classification and analysis.
/// </summary>
public interface IAiClassificationService
{
    /// <summary>
    /// Generates classification suggestions for a document using AI.
    /// </summary>
    /// <param name="documentContent">The text content extracted from the document.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="mimeType">The MIME type of the document.</param>
    /// <returns>AI-generated suggestions including tags, queue recommendations, and confidence scores.</returns>
    Task<AiSuggestion> GenerateSuggestionsAsync(string documentContent, string fileName, string mimeType);

    /// <summary>
    /// Generates a brief summary of a document using AI.
    /// </summary>
    /// <param name="documentContent">The text content extracted from the document.</param>
    /// <param name="maxLength">Maximum length of the summary in characters.</param>
    /// <returns>A brief summary of the document.</returns>
    Task<string> GenerateSummaryAsync(string documentContent, int maxLength = 500);

    /// <summary>
    /// Checks if AI service is available and configured.
    /// </summary>
    /// <returns>True if AI service is available; otherwise, false.</returns>
    Task<bool> IsAvailableAsync();
}
