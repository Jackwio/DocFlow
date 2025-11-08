using System;
using System.Collections.Generic;
using System.Text;
using DocFlow.Localization;
using Volo.Abp.Application.Services;

namespace DocFlow;

/* Inherit your application services from this class.
 */
public abstract class DocFlowAppService : ApplicationService
{
    protected DocFlowAppService()
    {
        LocalizationResource = typeof(DocFlowResource);
    }
}
