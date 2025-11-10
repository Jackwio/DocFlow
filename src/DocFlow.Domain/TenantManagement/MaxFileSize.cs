using System;
using Volo.Abp.Domain.Values;

namespace DocFlow.TenantManagement;

/// <summary>
/// Value object representing maximum file size in bytes.
/// </summary>
public sealed class MaxFileSize : ValueObject
{
    public long Bytes { get; }

    private MaxFileSize(long bytes)
    {
        Bytes = bytes;
    }

    /// <summary>
    /// Factory method to create MaxFileSize with validation.
    /// </summary>
    public static MaxFileSize Create(long bytes)
    {
        if (bytes < 1024) // Minimum 1KB
            throw new ArgumentException("Max file size must be at least 1KB", nameof(bytes));

        if (bytes > 104857600) // 100MB max
            throw new ArgumentException("Max file size cannot exceed 100MB", nameof(bytes));

        return new MaxFileSize(bytes);
    }

    /// <summary>
    /// Creates MaxFileSize from megabytes.
    /// </summary>
    public static MaxFileSize FromMegabytes(int megabytes)
    {
        if (megabytes < 1)
            throw new ArgumentException("Max file size must be at least 1MB", nameof(megabytes));

        return Create(megabytes * 1024L * 1024L);
    }

    public double ToMegabytes() => Bytes / (1024.0 * 1024.0);

    protected override System.Collections.Generic.IEnumerable<object> GetAtomicValues()
    {
        yield return Bytes;
    }

    public override string ToString() => $"{ToMegabytes():F2} MB";
}
