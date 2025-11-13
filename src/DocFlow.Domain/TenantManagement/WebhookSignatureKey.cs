using System;
using Volo.Abp.Domain.Values;

namespace DocFlow.TenantManagement;

/// <summary>
/// Value object representing webhook signature key for external system verification.
/// </summary>
public sealed class WebhookSignatureKey : ValueObject
{
    public string Value { get; }

    private WebhookSignatureKey(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Factory method to create a webhook signature key with validation.
    /// </summary>
    public static WebhookSignatureKey Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Webhook signature key cannot be empty", nameof(value));

        if (value.Length < 32)
            throw new ArgumentException("Webhook signature key must be at least 32 characters", nameof(value));

        if (value.Length > 256)
            throw new ArgumentException("Webhook signature key cannot exceed 256 characters", nameof(value));

        return new WebhookSignatureKey(value);
    }

    protected override System.Collections.Generic.IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => "***" + Value.Substring(Value.Length - 4); // Mask for security
}
