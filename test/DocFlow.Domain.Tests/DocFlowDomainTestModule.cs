using Volo.Abp.Modularity;

namespace DocFlow;

[DependsOn(
    typeof(DocFlowDomainModule),
    typeof(DocFlowTestBaseModule)
)]
public class DocFlowDomainTestModule : AbpModule
{

}
