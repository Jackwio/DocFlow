using System;
using System.Linq;
using System.Threading.Tasks;
using DocFlow.Inboxes;
using DocFlow.Inboxes.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DocFlow.InboxManagement;

/// <summary>
/// Application service for managing inboxes.
/// Allows tenant administrators to create and organize document sources.
/// </summary>
public sealed class InboxApplicationService : ApplicationService, IInboxApplicationService
{
    private readonly IInboxRepository _inboxRepository;
    private readonly IRepository<Inbox, Guid> _repository;

    public InboxApplicationService(
        IInboxRepository inboxRepository,
        IRepository<Inbox, Guid> repository)
    {
        _inboxRepository = inboxRepository;
        _repository = repository;
    }

    public async Task<PagedResultDto<InboxDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();
        var query = queryable
            .Where(i => i.TenantId == CurrentTenant.Id)
            .OrderByDescending(i => i.CreationTime);

        var totalCount = await AsyncExecuter.CountAsync(query);
        
        var items = await AsyncExecuter.ToListAsync(
            query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<InboxDto>(
            totalCount,
            items.Select(MapToDto).ToList());
    }

    public async Task<InboxDto> GetAsync(Guid id)
    {
        var inbox = await _repository.GetAsync(id);
        return MapToDto(inbox);
    }

    public async Task<InboxDto> CreateAsync(CreateInboxDto input)
    {
        var inbox = Inbox.Create(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            InboxName.Create(input.Name),
            input.Description);

        await _repository.InsertAsync(inbox, autoSave: true);
        
        return MapToDto(inbox);
    }

    public async Task<InboxDto> UpdateAsync(Guid id, UpdateInboxDto input)
    {
        var inbox = await _repository.GetAsync(id);
        
        inbox.UpdateDetails(
            InboxName.Create(input.Name),
            input.Description);

        await _repository.UpdateAsync(inbox, autoSave: true);
        
        return MapToDto(inbox);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task ActivateAsync(Guid id)
    {
        var inbox = await _repository.GetAsync(id);
        inbox.Activate();
        await _repository.UpdateAsync(inbox, autoSave: true);
    }

    public async Task DeactivateAsync(Guid id)
    {
        var inbox = await _repository.GetAsync(id);
        inbox.Deactivate();
        await _repository.UpdateAsync(inbox, autoSave: true);
    }

    private static InboxDto MapToDto(Inbox inbox)
    {
        return new InboxDto
        {
            Id = inbox.Id,
            Name = inbox.Name.Value,
            Description = inbox.Description,
            IsActive = inbox.IsActive,
            CreationTime = inbox.CreationTime
        };
    }
}
