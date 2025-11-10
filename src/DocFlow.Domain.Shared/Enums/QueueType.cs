namespace DocFlow.Enums;

/// <summary>
/// Defines the type of destination for document routing.
/// </summary>
public enum QueueType
{
    /// <summary>
    /// Route documents to a file system folder.
    /// </summary>
    Folder = 0,

    /// <summary>
    /// Route documents by sending to a webhook endpoint.
    /// </summary>
    Webhook = 1
}
