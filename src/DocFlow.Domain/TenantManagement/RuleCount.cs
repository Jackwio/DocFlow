using System;
using Volo.Abp.Domain.Values;

namespace DocFlow.TenantManagement;

/// <summary>
/// Value object representing rule count.
/// </summary>
public sealed class RuleCount : ValueObject
{
    public int Value { get; }

    private RuleCount(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Factory method to create RuleCount.
    /// </summary>
    public static RuleCount Create(int count)
    {
        if (count < 0)
            throw new ArgumentException("Rule count cannot be negative", nameof(count));

        return new RuleCount(count);
    }

    public RuleCount Increment() => new RuleCount(Value + 1);

    public RuleCount Decrement()
    {
        if (Value <= 0)
            throw new InvalidOperationException("Cannot decrement rule count below zero");

        return new RuleCount(Value - 1);
    }

    protected override System.Collections.Generic.IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
