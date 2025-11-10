using System;

namespace DocFlow.Documents;

/// <summary>
/// Value object representing a reference to a document stored in blob storage.
/// </summary>
public sealed record BlobReference
{
    public string ContainerName { get; }
    public string BlobName { get; }

    private BlobReference(string containerName, string blobName)
    {
        ContainerName = containerName;
        BlobName = blobName;
    }

    /// <summary>
    /// Creates a new BlobReference value object with validation.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobName">The name of the blob within the container.</param>
    /// <returns>A validated BlobReference instance.</returns>
    /// <exception cref="ArgumentException">Thrown when container or blob name is invalid.</exception>
    public static BlobReference Create(string containerName, string blobName)
    {
        if (string.IsNullOrWhiteSpace(containerName))
            throw new ArgumentException("Container name cannot be empty or whitespace", nameof(containerName));

        if (string.IsNullOrWhiteSpace(blobName))
            throw new ArgumentException("Blob name cannot be empty or whitespace", nameof(blobName));

        return new BlobReference(containerName, blobName);
    }

    public override string ToString() => $"{ContainerName}/{BlobName}";
}
