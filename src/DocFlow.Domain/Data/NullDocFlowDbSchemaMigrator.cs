using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace DocFlow.Data;

/* This is used if database provider does't define
 * IDocFlowDbSchemaMigrator implementation.
 */
public class NullDocFlowDbSchemaMigrator : IDocFlowDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
