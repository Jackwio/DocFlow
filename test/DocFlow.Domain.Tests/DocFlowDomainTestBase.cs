using Volo.Abp.Modularity;

namespace DocFlow;

/* Inherit from this class for your domain layer tests. */
public abstract class DocFlowDomainTestBase<TStartupModule> : DocFlowTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
