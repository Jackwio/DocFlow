using Microsoft.Extensions.Localization;
using DocFlow.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace DocFlow.Blazor.Client;

[Dependency(ReplaceServices = true)]
public class DocFlowBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<DocFlowResource> _localizer;

    public DocFlowBrandingProvider(IStringLocalizer<DocFlowResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
