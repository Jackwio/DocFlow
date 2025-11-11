using System;
using System.Collections.Generic;
using DocFlow.Enums;
using Volo.Abp.Application.Dtos;

namespace DocFlow.Documents.Dtos;

/// <summary>
/// DTO for document with full details.
/// </summary>
public sealed class DocumentDto : EntityDto<Guid>
{
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? LastError { get; set; }
    public Guid? RoutedToQueueId { get; set; }
    public string? Inbox { get; set; }
    public DateTime CreationTime { get; set; }
}
