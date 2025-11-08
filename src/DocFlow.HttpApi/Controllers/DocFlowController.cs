using DocFlow.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace DocFlow.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class DocFlowController : AbpControllerBase
{
    protected DocFlowController()
    {
        LocalizationResource = typeof(DocFlowResource);
    }
}
