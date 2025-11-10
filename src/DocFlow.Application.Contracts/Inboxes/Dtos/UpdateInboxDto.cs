using System.ComponentModel.DataAnnotations;

namespace DocFlow.Inboxes.Dtos;

public class UpdateInboxDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}
