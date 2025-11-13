using System.ComponentModel.DataAnnotations;

namespace DocFlow.TenantManagement.Dtos;

public class UpdateMaxFileSizeDto
{
    [Required]
    [Range(1, 100)]
    public int MaxFileSizeMB { get; set; }
}
