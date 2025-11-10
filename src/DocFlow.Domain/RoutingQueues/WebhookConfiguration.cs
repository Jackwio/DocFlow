using System;
using System.Collections.Generic;

namespace DocFlow.RoutingQueues;

/// <summary>
/// Value object representing webhook configuration for document routing.
/// Includes URL, headers, and retry policy.
/// </summary>
public sealed record WebhookConfiguration
{
    public string Url { get; }
    public IReadOnlyDictionary<string, string> Headers { get; }
    public int MaxRetryAttempts { get; }
    public int RetryDelaySeconds { get; }

    private WebhookConfiguration(
        string url,
        IReadOnlyDictionary<string, string> headers,
        int maxRetryAttempts,
        int retryDelaySeconds)
    {
        Url = url;
        Headers = headers;
        MaxRetryAttempts = maxRetryAttempts;
        RetryDelaySeconds = retryDelaySeconds;
    }

    /// <summary>
    /// Creates a new WebhookConfiguration with validation.
    /// </summary>
    public static WebhookConfiguration Create(
        string url,
        Dictionary<string, string>? headers = null,
        int maxRetryAttempts = 3,
        int retryDelaySeconds = 60)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Webhook URL cannot be empty", nameof(url));

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || 
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("Webhook URL must be a valid HTTP or HTTPS URL", nameof(url));

        if (maxRetryAttempts < 0 || maxRetryAttempts > 10)
            throw new ArgumentException("Max retry attempts must be between 0 and 10", nameof(maxRetryAttempts));

        if (retryDelaySeconds < 1 || retryDelaySeconds > 3600)
            throw new ArgumentException("Retry delay must be between 1 and 3600 seconds", nameof(retryDelaySeconds));

        var safeHeaders = headers != null 
            ? new Dictionary<string, string>(headers) 
            : new Dictionary<string, string>();

        return new WebhookConfiguration(url, safeHeaders, maxRetryAttempts, retryDelaySeconds);
    }

    public override string ToString() => $"{Url} (max {MaxRetryAttempts} retries, {RetryDelaySeconds}s delay)";
}
