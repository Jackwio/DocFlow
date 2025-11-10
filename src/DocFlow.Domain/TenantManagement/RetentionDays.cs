using System;
using Volo.Abp.Domain.Values;

namespace DocFlow.TenantManagement;

/// <summary>
/// Value object representing document retention period in days.
/// </summary>
public sealed class RetentionDays : ValueObject
{
    public int Value { get; }

    private RetentionDays(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Factory method to create RetentionDays with validation.
    /// </summary>
    public static RetentionDays Create(int days)
    {
        if (days < 30)
            throw new ArgumentException("Retention days must be at least 30 days", nameof(days));

        if (days > 3650) // 10 years max
            throw new ArgumentException("Retention days cannot exceed 3650 days (10 years)", nameof(days));

        return new RetentionDays(days);
    }

    protected override System.Collections.Generic.IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value} days";
}
