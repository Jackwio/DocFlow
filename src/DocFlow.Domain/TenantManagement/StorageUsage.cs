using System;
using Volo.Abp.Domain.Values;

namespace DocFlow.TenantManagement;

/// <summary>
/// Value object representing storage usage in bytes.
/// </summary>
public sealed class StorageUsage : ValueObject
{
    public long Bytes { get; }

    private StorageUsage(long bytes)
    {
        Bytes = bytes;
    }

    /// <summary>
    /// Factory method to create StorageUsage.
    /// </summary>
    public static StorageUsage Create(long bytes)
    {
        if (bytes < 0)
            throw new ArgumentException("Storage usage cannot be negative", nameof(bytes));

        return new StorageUsage(bytes);
    }

    public StorageUsage Add(long bytes)
    {
        if (bytes < 0)
            throw new ArgumentException("Cannot add negative bytes", nameof(bytes));

        return new StorageUsage(Bytes + bytes);
    }

    public StorageUsage Subtract(long bytes)
    {
        if (bytes < 0)
            throw new ArgumentException("Cannot subtract negative bytes", nameof(bytes));

        var newValue = Bytes - bytes;
        if (newValue < 0)
            newValue = 0; // Prevent negative storage usage

        return new StorageUsage(newValue);
    }

    public double ToMegabytes() => Bytes / (1024.0 * 1024.0);
    public double ToGigabytes() => Bytes / (1024.0 * 1024.0 * 1024.0);

    protected override System.Collections.Generic.IEnumerable<object> GetAtomicValues()
    {
        yield return Bytes;
    }

    public override string ToString() => $"{ToMegabytes():F2} MB";
}
