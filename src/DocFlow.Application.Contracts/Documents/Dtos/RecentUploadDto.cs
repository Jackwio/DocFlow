using System;

namespace DocFlow.Documents.Dtos;

/// <summary>
/// DTO for recently uploaded files (within 24 hours).
/// </summary>
public sealed class RecentUploadDto
{
    /// <summary>
    /// Document ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// File name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// File size in megabytes for display.
    /// </summary>
    public double FileSizeMB => FileSizeBytes / (1024.0 * 1024.0);

    /// <summary>
    /// Upload timestamp.
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// Time elapsed since upload (in minutes).
    /// </summary>
    public int MinutesAgo
    {
        get
        {
            var elapsed = DateTime.UtcNow - UploadedAt;
            return (int)elapsed.TotalMinutes;
        }
    }

    /// <summary>
    /// Processing status indicator.
    /// </summary>
    public bool IsProcessed { get; set; }
}
