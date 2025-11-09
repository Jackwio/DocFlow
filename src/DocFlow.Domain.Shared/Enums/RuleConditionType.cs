namespace DocFlow.Enums;

/// <summary>
/// Defines the type of condition that can be evaluated in a classification rule.
/// </summary>
public enum RuleConditionType
{
    /// <summary>
    /// Match against filename using regular expression pattern.
    /// </summary>
    FileNameRegex = 0,

    /// <summary>
    /// Match against document MIME type.
    /// </summary>
    MimeType = 1,

    /// <summary>
    /// Match against file size range.
    /// </summary>
    FileSize = 2,

    /// <summary>
    /// Match against text content extracted from document.
    /// </summary>
    TextContent = 3
}
