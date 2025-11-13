using System;

namespace DocFlow.TenantManagement.Dtos;

public class TenantUsageStatsDto
{
    public Guid Id { get; set; }
    public int DocumentCount { get; set; }
    public long StorageUsageBytes { get; set; }
    public double StorageUsageMB { get; set; }
    public double StorageUsageGB { get; set; }
    public int RuleCount { get; set; }
    public DateTime LastUpdated { get; set; }
}
