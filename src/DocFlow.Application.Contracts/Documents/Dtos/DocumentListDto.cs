using System;
using System.Collections.Generic;
using DocFlow.Enums;
using Volo.Abp.Application.Dtos;

namespace DocFlow.Documents.Dtos;

/// <summary>
/// DTO for document list view with minimal information.
/// </summary>
public sealed class DocumentListDto : EntityDto<Guid>
{
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DocumentStatus Status { get; set; }
    public int TagCount { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? Inbox { get; set; }
    public DateTime CreationTime { get; set; }
}
