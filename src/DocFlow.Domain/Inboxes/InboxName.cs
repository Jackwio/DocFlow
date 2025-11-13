using System;
using Volo.Abp.Domain.Values;

namespace DocFlow.Inboxes;

/// <summary>
/// Value object representing an inbox name.
/// </summary>
public sealed class InboxName : ValueObject
{
    public string Value { get; }

    private InboxName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Factory method to create an InboxName with validation.
    /// </summary>
    public static InboxName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Inbox name cannot be empty", nameof(value));

        if (value.Length > 100)
            throw new ArgumentException("Inbox name cannot exceed 100 characters", nameof(value));

        return new InboxName(value.Trim());
    }

    protected override System.Collections.Generic.IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
