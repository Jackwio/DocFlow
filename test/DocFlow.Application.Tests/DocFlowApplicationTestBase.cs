using Volo.Abp.Modularity;

namespace DocFlow;

public abstract class DocFlowApplicationTestBase<TStartupModule> : DocFlowTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
