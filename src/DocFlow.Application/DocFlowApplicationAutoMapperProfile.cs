using System.Linq;
using AutoMapper;
using DocFlow.ClassificationRules;
using DocFlow.ClassificationRules.Dtos;
using DocFlow.Documents;
using DocFlow.Documents.Dtos;
using DocFlow.RoutingQueues;
using DocFlow.RoutingQueues.Dtos;

namespace DocFlow;

public class DocFlowApplicationAutoMapperProfile : Profile
{
    public DocFlowApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        // Document mappings
        CreateMap<Document, DocumentDto>()
            .ForMember(d => d.FileName, opt => opt.MapFrom(s => s.FileName.Value))
            .ForMember(d => d.FileSizeBytes, opt => opt.MapFrom(s => s.FileSize.Bytes))
            .ForMember(d => d.MimeType, opt => opt.MapFrom(s => s.MimeType.Value))
            .ForMember(d => d.Tags, opt => opt.MapFrom(s => s.Tags.Select(t => t.Name.Value).ToList()))
            .ForMember(d => d.LastError, opt => opt.MapFrom(s => s.LastError != null ? s.LastError.Value : null));

        CreateMap<Document, DocumentListDto>()
            .ForMember(d => d.FileName, opt => opt.MapFrom(s => s.FileName.Value))
            .ForMember(d => d.FileSizeBytes, opt => opt.MapFrom(s => s.FileSize.Bytes))
            .ForMember(d => d.TagCount, opt => opt.MapFrom(s => s.Tags.Count));

        CreateMap<ClassificationHistoryEntry, ClassificationHistoryDto>()
            .ForMember(d => d.TagName, opt => opt.MapFrom(s => s.TagName.Value))
            .ForMember(d => d.ConfidenceScore, opt => opt.MapFrom(s => s.ConfidenceScore.Value));

        // ClassificationRule mappings
        CreateMap<ClassificationRule, ClassificationRuleDto>()
            .ForMember(d => d.Priority, opt => opt.MapFrom(s => s.Priority.Value))
            .ForMember(d => d.ApplyTags, opt => opt.MapFrom(s => s.ApplyTags.Select(t => t.Value).ToList()))
            .ForMember(d => d.Conditions, opt => opt.MapFrom(s => s.Conditions));

        CreateMap<RuleCondition, RuleConditionDto>()
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()));

        // RoutingQueue mappings
        CreateMap<RoutingQueue, RoutingQueueDto>()
            .ForMember(d => d.FolderPath, opt => opt.MapFrom(s => s.FolderPath != null ? s.FolderPath.Value : null))
            .ForMember(d => d.WebhookConfiguration, opt => opt.MapFrom(s => s.WebhookConfiguration));

        CreateMap<WebhookConfiguration, WebhookConfigurationDto>();

        CreateMap<WebhookDelivery, WebhookDeliveryDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.LastError, opt => opt.MapFrom(s => s.LastError != null ? s.LastError.Value : null));
    }
}
