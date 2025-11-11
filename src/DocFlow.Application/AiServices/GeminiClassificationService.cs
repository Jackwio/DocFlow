using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DocFlow.Documents;
using Mscc.GenerativeAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DocFlow.AiServices;

/// <summary>
/// Google Gemini-based implementation of AI classification service.
/// </summary>
public class GeminiClassificationService : IAiClassificationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiClassificationService> _logger;
    private readonly string? _apiKey;
    private readonly string _modelName;

    public GeminiClassificationService(
        IConfiguration configuration,
        ILogger<GeminiClassificationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _apiKey = _configuration["Gemini:ApiKey"] ?? "test";
        _modelName = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";
    }

    public async Task<AiSuggestion> GenerateSuggestionsAsync(string documentContent, string fileName, string mimeType)
    {
        if (string.IsNullOrWhiteSpace(documentContent))
            throw new ArgumentException("Document content cannot be empty", nameof(documentContent));

        if (!await IsAvailableAsync())
            throw new InvalidOperationException("AI service is not available or not configured");

        try
        {
            var model = new GoogleAI(apiKey: _apiKey);

            var prompt = $@"You are a document classification assistant. Analyze the provided document and suggest:
1. Relevant tags for classification (e.g., Invoice, Contract, Receipt, Legal, Accounting)
2. A recommended queue/department (e.g., Accounting, Legal, HR, Sales)
3. A confidence score (0.0 to 1.0) for your suggestions
4. Brief reasoning for each tag

File Name: {fileName}
MIME Type: {mimeType}

Document Content:
{documentContent.Substring(0, Math.Min(documentContent.Length, 4000))}

Respond ONLY with valid JSON in this exact format:
{{
  ""suggestedTags"": [
    {{
      ""tagName"": ""Invoice"",
      ""confidence"": 0.95,
      ""reasoning"": ""Document contains invoice number and payment terms""
    }}
  ],
  ""suggestedQueue"": ""Accounting"",
  ""overallConfidence"": 0.90,
  ""summary"": ""Brief document summary""
}}

Analyze this document and provide classification suggestions in JSON format.";

            var response = await model.GenerateContentAsync(_modelName, prompt);
            var responseContent = response?.Text ?? string.Empty;

            _logger.LogInformation("Gemini response: {Response}", responseContent);

            // Clean up response if it contains markdown code blocks
            responseContent = responseContent.Trim();
            if (responseContent.StartsWith("```json"))
            {
                responseContent = responseContent.Substring(7);
            }
            if (responseContent.StartsWith("```"))
            {
                responseContent = responseContent.Substring(3);
            }
            if (responseContent.EndsWith("```"))
            {
                responseContent = responseContent.Substring(0, responseContent.Length - 3);
            }
            responseContent = responseContent.Trim();

            // Parse JSON response
            var jsonResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (jsonResponse == null || jsonResponse.SuggestedTags == null || !jsonResponse.SuggestedTags.Any())
            {
                throw new InvalidOperationException("Invalid response from AI service");
            }

            // Convert to domain model
            var suggestedTags = jsonResponse.SuggestedTags
                .Select(t => SuggestedTag.Create(
                    TagName.Create(t.TagName),
                    ConfidenceScore.Create(t.Confidence),
                    t.Reasoning))
                .ToList();

            var overallConfidence = ConfidenceScore.Create(jsonResponse.OverallConfidence);

            // For now, we don't map queue names to GUIDs - that would require queue repository lookup
            // This can be enhanced later
            var suggestion = AiSuggestion.Create(
                suggestedTags,
                null, // suggestedQueueId - would need to resolve from queue name
                overallConfidence,
                jsonResponse.Summary);

            return suggestion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI suggestions for document {FileName}", fileName);
            throw new InvalidOperationException($"Failed to generate AI suggestions: {ex.Message}", ex);
        }
    }

    public async Task<string> GenerateSummaryAsync(string documentContent, int maxLength = 500)
    {
        if (string.IsNullOrWhiteSpace(documentContent))
            throw new ArgumentException("Document content cannot be empty", nameof(documentContent));

        if (!await IsAvailableAsync())
            throw new InvalidOperationException("AI service is not available or not configured");

        try
        {
            var model = new GoogleAI(apiKey: _apiKey);

            var prompt = $@"You are a document summarization assistant. Provide a brief, clear summary of the document in no more than {maxLength} characters. Focus on the key points and purpose of the document.

Document Content:
{documentContent.Substring(0, Math.Min(documentContent.Length, 4000))}

Provide a brief summary:";

            var response = await model.GenerateContentAsync(_modelName, prompt);
            var summary = response?.Text?.Trim() ?? string.Empty;

            // Ensure summary doesn't exceed max length
            if (summary.Length > maxLength)
            {
                summary = summary.Substring(0, maxLength - 3) + "...";
            }

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating summary");
            throw new InvalidOperationException($"Failed to generate summary: {ex.Message}", ex);
        }
    }

    public Task<bool> IsAvailableAsync()
    {
        var isAvailable = !string.IsNullOrWhiteSpace(_apiKey);
        return Task.FromResult(isAvailable);
    }

    // Helper class for JSON deserialization
    private class GeminiResponse
    {
        public List<GeminiTagSuggestion> SuggestedTags { get; set; } = new();
        public string? SuggestedQueue { get; set; }
        public double OverallConfidence { get; set; }
        public string? Summary { get; set; }
    }

    private class GeminiTagSuggestion
    {
        public string TagName { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string? Reasoning { get; set; }
    }
}
