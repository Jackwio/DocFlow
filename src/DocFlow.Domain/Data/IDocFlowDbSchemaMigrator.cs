using System.Threading.Tasks;

namespace DocFlow.Data;

public interface IDocFlowDbSchemaMigrator
{
    Task MigrateAsync();
}
