using System.ComponentModel.DataAnnotations;

namespace DocFlow.Documents.Dtos;

/// <summary>
/// DTO for uploading a document.
/// Used with multipart/form-data requests where API receives the file.
/// </summary>
public sealed class UploadDocumentDto
{
    /// <summary>
    /// Optional custom filename. If not provided, original filename from form file will be used.
    /// </summary>
    [StringLength(255)]
    public string? FileName { get; set; }

    public string? Description { get; set; }
}
