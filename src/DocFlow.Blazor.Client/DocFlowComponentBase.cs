using DocFlow.Localization;
using Volo.Abp.AspNetCore.Components;

namespace DocFlow.Blazor.Client;

public abstract class DocFlowComponentBase : AbpComponentBase
{
    protected DocFlowComponentBase()
    {
        LocalizationResource = typeof(DocFlowResource);
    }
}
