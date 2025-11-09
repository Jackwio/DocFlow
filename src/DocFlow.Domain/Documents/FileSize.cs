using System;

namespace DocFlow.Documents;

/// <summary>
/// Value object representing a file size in bytes with validation.
/// Enforces minimum and maximum size constraints.
/// </summary>
public sealed record FileSize
{
    private const long MinSizeBytes = 0;
    private const long MaxSizeBytes = 52428800; // 50MB

    public long Bytes { get; }

    private FileSize(long bytes)
    {
        Bytes = bytes;
    }

    /// <summary>
    /// Creates a new FileSize value object with validation.
    /// </summary>
    /// <param name="bytes">The size in bytes.</param>
    /// <returns>A validated FileSize instance.</returns>
    /// <exception cref="ArgumentException">Thrown when size is outside valid range.</exception>
    public static FileSize Create(long bytes)
    {
        if (bytes < MinSizeBytes)
            throw new ArgumentException($"File size cannot be negative", nameof(bytes));

        if (bytes > MaxSizeBytes)
            throw new ArgumentException($"File size cannot exceed {MaxSizeBytes} bytes ({ToMegabytes(MaxSizeBytes)}MB)", nameof(bytes));

        return new FileSize(bytes);
    }

    /// <summary>
    /// Converts the file size to megabytes.
    /// </summary>
    public double ToMegabytes() => ToMegabytes(Bytes);

    private static double ToMegabytes(long bytes) => bytes / (1024.0 * 1024.0);

    public override string ToString() => $"{ToMegabytes():F2}MB ({Bytes} bytes)";
}
