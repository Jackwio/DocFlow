using System;
using System.IO;
using System.Linq;

namespace DocFlow.RoutingQueues;

/// <summary>
/// Value object representing a validated file system folder path.
/// </summary>
public sealed record FolderPath
{
    private const int MaxLength = 260; // Windows MAX_PATH
    private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

    public string Value { get; }

    private FolderPath(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new FolderPath with validation.
    /// </summary>
    public static FolderPath Create(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Folder path cannot be empty", nameof(path));

        if (path.Length > MaxLength)
            throw new ArgumentException($"Folder path cannot exceed {MaxLength} characters", nameof(path));

        if (ContainsInvalidPathCharacters(path))
            throw new ArgumentException("Folder path contains invalid characters", nameof(path));

        if (IsSuspiciousPath(path))
            throw new ArgumentException("Folder path appears suspicious or potentially malicious", nameof(path));

        // Normalize the path
        var normalizedPath = Path.GetFullPath(path);

        return new FolderPath(normalizedPath);
    }

    private static bool ContainsInvalidPathCharacters(string path)
    {
        return path.IndexOfAny(InvalidPathChars) >= 0;
    }

    private static bool IsSuspiciousPath(string path)
    {
        // Check for path traversal attempts
        var suspicious = new[] { "..", "~", "$", "|", "<", ">" };
        return suspicious.Any(pattern => path.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    public override string ToString() => Value;
}
