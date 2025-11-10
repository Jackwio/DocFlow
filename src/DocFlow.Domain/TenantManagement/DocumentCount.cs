using System;
using Volo.Abp.Domain.Values;

namespace DocFlow.TenantManagement;

/// <summary>
/// Value object representing document count.
/// </summary>
public sealed class DocumentCount : ValueObject
{
    public int Value { get; }

    private DocumentCount(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Factory method to create DocumentCount.
    /// </summary>
    public static DocumentCount Create(int count)
    {
        if (count < 0)
            throw new ArgumentException("Document count cannot be negative", nameof(count));

        return new DocumentCount(count);
    }

    public DocumentCount Increment() => new DocumentCount(Value + 1);

    public DocumentCount Decrement()
    {
        if (Value <= 0)
            throw new InvalidOperationException("Cannot decrement document count below zero");

        return new DocumentCount(Value - 1);
    }

    protected override System.Collections.Generic.IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
