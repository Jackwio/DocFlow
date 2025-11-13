using System.ComponentModel.DataAnnotations;

namespace DocFlow.TenantManagement.Dtos;

public class UpdateRetentionDaysDto
{
    [Required]
    [Range(30, 3650)]
    public int RetentionDays { get; set; }
}
