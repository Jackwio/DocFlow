using System.ComponentModel.DataAnnotations;

namespace DocFlow.TenantManagement.Dtos;

public class ConfigureTagColorDto
{
    [Required]
    [StringLength(50)]
    public string TagName { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^#?[0-9A-Fa-f]{6}$")]
    public string ColorHex { get; set; } = string.Empty;
}
