using DocFlow.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace DocFlow.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(DocFlowEntityFrameworkCoreModule),
    typeof(DocFlowApplicationContractsModule)
    )]
public class DocFlowDbMigratorModule : AbpModule
{
}
