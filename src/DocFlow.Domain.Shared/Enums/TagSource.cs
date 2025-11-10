namespace DocFlow.Enums;

/// <summary>
/// Indicates how a tag was applied to a document.
/// </summary>
public enum TagSource
{
    /// <summary>
    /// Tag was applied automatically by classification rules.
    /// </summary>
    Automatic = 0,

    /// <summary>
    /// Tag was manually added by an operator.
    /// </summary>
    Manual = 1,

    /// <summary>
    /// Tag was suggested by AI model.
    /// </summary>
    AiSuggested = 2,

    /// <summary>
    /// Tag was applied from AI suggestion.
    /// </summary>
    AiApplied = 3
}
