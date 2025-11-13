using System.ComponentModel.DataAnnotations;

namespace DocFlow.TenantManagement.Dtos;

public class UpdatePrivacyModeDto
{
    [Required]
    public bool PrivacyStrictMode { get; set; }
}
