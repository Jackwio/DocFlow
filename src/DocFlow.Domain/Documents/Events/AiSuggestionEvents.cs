using System;
using System.Collections.Generic;

namespace DocFlow.Documents.Events;

/// <summary>
/// Domain event raised when AI suggestions are generated for a document.
/// </summary>
public sealed class AiSuggestionGeneratedEvent
{
    public Guid DocumentId { get; }
    public AiSuggestion AiSuggestion { get; }

    public AiSuggestionGeneratedEvent(Guid documentId, AiSuggestion aiSuggestion)
    {
        DocumentId = documentId;
        AiSuggestion = aiSuggestion ?? throw new ArgumentNullException(nameof(aiSuggestion));
    }
}

/// <summary>
/// Domain event raised when AI suggestions are applied to a document.
/// </summary>
public sealed class AiSuggestionsAppliedEvent
{
    public Guid DocumentId { get; }
    public IReadOnlyList<TagName> AppliedTags { get; }

    public AiSuggestionsAppliedEvent(Guid documentId, IReadOnlyList<TagName> appliedTags)
    {
        DocumentId = documentId;
        AppliedTags = appliedTags ?? throw new ArgumentNullException(nameof(appliedTags));
    }
}
