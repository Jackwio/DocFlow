using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DocFlow.Documents.Dtos;

/// <summary>
/// DTO used for multipart/form-data upload that contains the file and other fields.
/// This wrapper allows Swashbuckle to properly generate the OpenAPI for file uploads.
/// Placed in the HttpApi.Host project because it depends on ASP.NET types.
/// </summary>
public sealed class UploadDocumentWithFileDto
{
    /// <summary>
    /// The uploaded file.
    /// </summary>
    [Required]
    public IFormFile File { get; set; } = default!;

    /// <summary>
    /// Optional custom filename. If not provided, original filename from form file will be used.
    /// </summary>
    [StringLength(255)]
    public string? FileName { get; set; }

    /// <summary>
    /// Optional description for the document.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional inbox/category name for the document.
    /// </summary>
    [StringLength(100)]
    public string? Inbox { get; set; }
}
