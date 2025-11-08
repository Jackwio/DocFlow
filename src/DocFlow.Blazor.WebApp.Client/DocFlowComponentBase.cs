using DocFlow.Localization;
using Volo.Abp.AspNetCore.Components;

namespace DocFlow.Blazor.WebApp.Client;

public abstract class DocFlowComponentBase : AbpComponentBase
{
    protected DocFlowComponentBase()
    {
        LocalizationResource = typeof(DocFlowResource);
    }
}
