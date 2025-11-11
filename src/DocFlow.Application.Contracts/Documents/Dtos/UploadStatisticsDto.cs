namespace DocFlow.Documents.Dtos;

/// <summary>
/// DTO for upload statistics per user.
/// Tracks file counts and storage usage.
/// </summary>
public sealed class UploadStatisticsDto
{
    /// <summary>
    /// Number of files uploaded today.
    /// </summary>
    public int FilesToday { get; set; }

    /// <summary>
    /// Number of files uploaded this week.
    /// </summary>
    public int FilesThisWeek { get; set; }

    /// <summary>
    /// Total number of files uploaded.
    /// </summary>
    public int FilesTotal { get; set; }

    /// <summary>
    /// Storage used in bytes.
    /// </summary>
    public long StorageUsedBytes { get; set; }

    /// <summary>
    /// Maximum storage quota in bytes (default: 100 GB).
    /// </summary>
    public long StorageQuotaBytes { get; set; }

    /// <summary>
    /// Storage used in gigabytes for display.
    /// </summary>
    public double StorageUsedGB => StorageUsedBytes / (1024.0 * 1024.0 * 1024.0);

    /// <summary>
    /// Storage quota in gigabytes for display.
    /// </summary>
    public double StorageQuotaGB => StorageQuotaBytes / (1024.0 * 1024.0 * 1024.0);

    /// <summary>
    /// Storage usage percentage (0-100).
    /// </summary>
    public double StorageUsagePercent => StorageQuotaBytes > 0 
        ? (StorageUsedBytes * 100.0) / StorageQuotaBytes 
        : 0;
}
