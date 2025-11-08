using Volo.Abp.Modularity;

namespace DocFlow;

[DependsOn(
    typeof(DocFlowApplicationModule),
    typeof(DocFlowDomainTestModule)
)]
public class DocFlowApplicationTestModule : AbpModule
{

}
