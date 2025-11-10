using Volo.Abp.BlobStoring;

namespace DocFlow.Documents;

/// <summary>
/// Blob container for storing document files.
/// </summary>
[BlobContainerName("docflow-documents")]
public sealed class DocFlowBlobContainer
{
}
