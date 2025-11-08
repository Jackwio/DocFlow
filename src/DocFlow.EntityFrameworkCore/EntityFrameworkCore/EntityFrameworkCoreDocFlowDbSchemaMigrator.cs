using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DocFlow.Data;
using Volo.Abp.DependencyInjection;

namespace DocFlow.EntityFrameworkCore;

public class EntityFrameworkCoreDocFlowDbSchemaMigrator
    : IDocFlowDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreDocFlowDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolve the DocFlowDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<DocFlowDbContext>()
            .Database
            .MigrateAsync();
    }
}
