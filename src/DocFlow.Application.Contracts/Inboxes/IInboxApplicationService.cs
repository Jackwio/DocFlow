using System;
using System.Threading.Tasks;
using DocFlow.Inboxes.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DocFlow.Inboxes;

/// <summary>
/// Application service for managing inboxes.
/// </summary>
public interface IInboxApplicationService : IApplicationService
{
    /// <summary>
    /// Gets a list of all inboxes for the current tenant.
    /// </summary>
    Task<PagedResultDto<InboxDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Gets an inbox by ID.
    /// </summary>
    Task<InboxDto> GetAsync(Guid id);

    /// <summary>
    /// Creates a new inbox.
    /// </summary>
    Task<InboxDto> CreateAsync(CreateInboxDto input);

    /// <summary>
    /// Updates an existing inbox.
    /// </summary>
    Task<InboxDto> UpdateAsync(Guid id, UpdateInboxDto input);

    /// <summary>
    /// Deletes an inbox.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Activates an inbox.
    /// </summary>
    Task ActivateAsync(Guid id);

    /// <summary>
    /// Deactivates an inbox.
    /// </summary>
    Task DeactivateAsync(Guid id);
}
