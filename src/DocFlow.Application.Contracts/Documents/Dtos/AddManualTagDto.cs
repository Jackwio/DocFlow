using System.ComponentModel.DataAnnotations;

namespace DocFlow.Documents.Dtos;

/// <summary>
/// DTO for adding a manual tag to a document.
/// </summary>
public sealed class AddManualTagDto
{
    [Required]
    [StringLength(50)]
    public string TagName { get; set; } = string.Empty;
}
