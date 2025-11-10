# DocFlow AI Classification Feature

## Overview

The AI Classification feature enables automatic document classification and tagging using OpenAI's GPT models. This feature helps operators quickly classify documents with AI-suggested tags, generate summaries, and improve document processing efficiency.

## Features

### 1. AI-Powered Tag Suggestions
- Analyzes document content to suggest relevant tags
- Provides confidence scores (0.0 to 1.0) for each suggestion
- Includes reasoning for each suggested tag
- Supports manual review before applying

### 2. One-Click Apply
- Apply all AI suggestions with a single API call
- Converts suggested tags to actual applied tags
- Updates document status to "Classified"
- Maintains audit trail through domain events

### 3. Document Summarization
- Generate brief summaries of document content
- Configurable summary length (default: 500 characters)
- Helps operators quickly understand document contents

### 4. Tenant-Level Control
- Enable/disable AI features via configuration
- Check AI availability status
- Future: Per-tenant AI settings in database

## Configuration

Add the following configuration to `appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key-here",
    "Model": "gpt-4o-mini",
    "Enabled": true,
    "AutoApplySuggestions": false,
    "MinConfidenceThreshold": 0.7
  }
}
```

### Configuration Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ApiKey` | string | "test" | Your OpenAI API key |
| `Model` | string | "gpt-4o-mini" | OpenAI model to use |
| `Enabled` | bool | true | Enable/disable AI features |
| `AutoApplySuggestions` | bool | false | Automatically apply high-confidence suggestions |
| `MinConfidenceThreshold` | double | 0.7 | Minimum confidence score for auto-apply |

## API Endpoints

### 1. Generate AI Suggestions

**Endpoint:** `POST /api/documents/ai/{documentId}/suggestions`

**Description:** Analyzes a document and generates AI-powered classification suggestions.

**Request:**
```http
POST /api/documents/ai/3fa85f64-5717-4562-b3fc-2c963f66afa6/suggestions
Authorization: Bearer {token}
```

**Response:**
```json
{
  "suggestedTags": [
    {
      "tagName": "Invoice",
      "confidence": 0.95,
      "reasoning": "Document contains invoice number and payment terms"
    },
    {
      "tagName": "Accounting",
      "confidence": 0.90,
      "reasoning": "Financial document with monetary amounts"
    }
  ],
  "suggestedQueueId": null,
  "confidence": 0.92,
  "summary": "This is an invoice for services rendered...",
  "generatedAt": "2025-11-10T23:15:00Z"
}
```

### 2. Apply AI Suggestions (One-Click)

**Endpoint:** `POST /api/documents/ai/apply-suggestions`

**Description:** Applies AI-suggested tags to the document.

**Request:**
```http
POST /api/documents/ai/apply-suggestions
Content-Type: application/json
Authorization: Bearer {token}

{
  "documentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "invoice-2025.pdf",
  "status": "Classified",
  "tags": ["Invoice", "Accounting"],
  "aiSuggestion": {
    "suggestedTags": [...],
    "confidence": 0.92,
    "summary": "...",
    "generatedAt": "2025-11-10T23:15:00Z"
  }
}
```

### 3. Generate Document Summary

**Endpoint:** `POST /api/documents/ai/summary`

**Description:** Generates a brief summary of the document content.

**Request:**
```http
POST /api/documents/ai/summary
Content-Type: application/json
Authorization: Bearer {token}

{
  "documentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "maxLength": 500
}
```

**Response:**
```json
{
  "documentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "summary": "This invoice, dated January 15, 2025, requests payment for consulting services provided in Q4 2024. The total amount due is $5,000, with payment terms of Net 30 days.",
  "generatedAt": "2025-11-10T23:15:00Z"
}
```

### 4. Check AI Status

**Endpoint:** `GET /api/documents/ai/status`

**Description:** Checks if AI service is enabled and available.

**Request:**
```http
GET /api/documents/ai/status
Authorization: Bearer {token}
```

**Response:**
```json
true
```

## Usage Workflow

### For Operators

1. **Upload Document**
   - Upload a document via `/api/documents/upload`
   - Document is stored with "Pending" status

2. **Request AI Suggestions**
   - Call `/api/documents/ai/{documentId}/suggestions`
   - Review suggested tags and confidence scores
   - Read the AI-generated reasoning for each tag

3. **Apply Suggestions (One-Click)**
   - Call `/api/documents/ai/apply-suggestions`
   - All suggested tags are applied with "AiApplied" source
   - Document status changes to "Classified"

4. **Manual Adjustments** (Optional)
   - Add manual tags via `/api/documents/{id}/tags`
   - Remove incorrect tags if needed
   - Manual tags are marked with "Manual" source

### For Administrators

1. **Enable AI Features**
   - Set `OpenAI:Enabled = true` in configuration
   - Configure OpenAI API key
   - Choose appropriate model (gpt-4o-mini recommended for cost efficiency)

2. **Monitor Usage**
   - Check domain events for AI operations
   - Review confidence scores and accuracy
   - Adjust `MinConfidenceThreshold` as needed

3. **Disable AI** (Emergency)
   - Set `OpenAI:Enabled = false`
   - System continues to work without AI
   - Manual classification still available

## Database Schema

### DocumentAiSuggestedTags Table

Stores AI-suggested tags before they are applied.

| Column | Type | Description |
|--------|------|-------------|
| DocumentId | UUID | Foreign key to Documents |
| Id | int | Auto-increment ID |
| TagName | varchar(50) | Suggested tag name |
| Confidence | double | Confidence score (0.0-1.0) |
| Reasoning | varchar(500) | AI reasoning for suggestion |

### Documents Table (New Columns)

| Column | Type | Description |
|--------|------|-------------|
| AiConfidence | double | Overall AI confidence |
| AiGeneratedAt | timestamp | When suggestions were generated |
| AiSuggestedQueueId | UUID | Suggested routing queue |
| AiSummary | varchar(2000) | AI-generated summary |

### DocumentTags Table (Modified)

| Column | Type | Description |
|--------|------|-------------|
| ConfidenceScore | double | Confidence score for AI tags |

## Domain Model

### TagSource Enum

```csharp
public enum TagSource
{
    Automatic = 0,    // Applied by classification rules
    Manual = 1,       // Added by operator
    AiSuggested = 2,  // Suggested by AI (not yet applied)
    AiApplied = 3     // Applied from AI suggestion
}
```

### Key Domain Methods

```csharp
// Store AI suggestions without applying
document.StoreAiSuggestion(aiSuggestion);

// Apply AI suggestions (one-click)
document.ApplyAiSuggestions();

// Create AI-applied tag with confidence
var tag = Tag.CreateAiApplied(tagName, confidence);
```

## Security Considerations

1. **API Key Protection**
   - Store OpenAI API key in secure configuration (e.g., Azure Key Vault)
   - Never commit API keys to source control
   - Use different keys for development and production

2. **Rate Limiting**
   - Implement rate limiting to prevent abuse
   - Monitor OpenAI API usage and costs
   - Set appropriate timeout values

3. **Input Validation**
   - Validate document content before sending to AI
   - Sanitize AI responses before storing
   - Limit document size for AI processing

4. **Privacy**
   - Be aware that document content is sent to OpenAI
   - Consider data residency requirements
   - Review OpenAI's privacy policy

## Cost Optimization

1. **Model Selection**
   - Use `gpt-4o-mini` for cost-effective processing
   - Reserve `gpt-4o` for complex documents
   - Adjust model based on accuracy requirements

2. **Content Optimization**
   - Limit text sent to AI (first 4000 characters)
   - Extract only relevant content (skip headers/footers)
   - Cache results to avoid redundant API calls

3. **Batch Processing**
   - Process multiple documents in batches
   - Use background jobs for non-urgent classifications
   - Implement retry logic with exponential backoff

## Troubleshooting

### AI Service Not Available

**Problem:** `/api/documents/ai/status` returns `false`

**Solutions:**
- Check `OpenAI:ApiKey` configuration
- Verify `OpenAI:Enabled = true`
- Test API key with OpenAI directly
- Check network connectivity

### Low Confidence Scores

**Problem:** AI suggestions have consistently low confidence

**Solutions:**
- Improve document quality (OCR, scanning)
- Provide more context in prompts
- Use a more advanced model (gpt-4o)
- Fine-tune prompts for your domain

### Incorrect Suggestions

**Problem:** AI suggests irrelevant tags

**Solutions:**
- Review and refine system prompts
- Add domain-specific examples to prompts
- Increase `MinConfidenceThreshold`
- Use feedback to improve prompts

### Performance Issues

**Problem:** AI classification takes too long

**Solutions:**
- Reduce document size sent to AI
- Use faster model (gpt-4o-mini)
- Implement caching for repeated documents
- Process in background jobs

## Future Enhancements

- [ ] Persistent tenant-level AI settings
- [ ] Automatic application based on confidence threshold
- [ ] Feedback loop for learning from corrections
- [ ] Queue recommendation mapping
- [ ] Multi-language support
- [ ] Custom AI model fine-tuning
- [ ] Batch processing optimization
- [ ] Cost tracking and reporting

## Support

For issues or questions:
- Check this documentation first
- Review domain events and logs
- Check OpenAI API status
- Contact development team

## References

- [OpenAI API Documentation](https://platform.openai.com/docs)
- [Azure.AI.OpenAI Package](https://www.nuget.org/packages/Azure.AI.OpenAI)
- [DocFlow Architecture Documentation](../README.md)
