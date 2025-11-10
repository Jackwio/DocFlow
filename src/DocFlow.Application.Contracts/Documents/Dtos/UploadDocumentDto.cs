using System.ComponentModel.DataAnnotations;

namespace DocFlow.Documents.Dtos;

/// <summary>
/// DTO for uploading a document.
/// File content should be handled separately via blob storage.
/// </summary>
public sealed class UploadDocumentDto
{
    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public long FileSizeBytes { get; set; }

    [Required]
    [StringLength(100)]
    public string MimeType { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string BlobContainerName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string BlobName { get; set; } = string.Empty;

    public string? Description { get; set; }
}
