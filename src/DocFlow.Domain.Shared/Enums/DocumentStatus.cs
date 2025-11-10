namespace DocFlow.Enums;

/// <summary>
/// Represents the processing status of a document in the system.
/// </summary>
public enum DocumentStatus
{
    /// <summary>
    /// Document has been uploaded and is awaiting classification.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Document has been successfully classified with tags applied.
    /// </summary>
    Classified = 1,

    /// <summary>
    /// Document has been routed to its destination queue.
    /// </summary>
    Routed = 2,

    /// <summary>
    /// Document classification or routing failed and requires intervention.
    /// </summary>
    Failed = 3
}
