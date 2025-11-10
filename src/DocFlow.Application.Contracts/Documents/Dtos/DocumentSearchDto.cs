using System;
using System.Collections.Generic;
using DocFlow.Enums;

namespace DocFlow.Documents.Dtos;

/// <summary>
/// DTO for searching documents with filtering parameters.
/// </summary>
public sealed class DocumentSearchDto
{
    public DocumentStatus? Status { get; set; }
    public List<string>? Tags { get; set; }
    public string? FileNameContains { get; set; }
    public DateTime? UploadedAfter { get; set; }
    public DateTime? UploadedBefore { get; set; }
    public int MaxResults { get; set; } = 100;
    public int SkipCount { get; set; } = 0;
}
