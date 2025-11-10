using System;

namespace DocFlow.Inboxes.Dtos;

public class InboxDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreationTime { get; set; }
}
