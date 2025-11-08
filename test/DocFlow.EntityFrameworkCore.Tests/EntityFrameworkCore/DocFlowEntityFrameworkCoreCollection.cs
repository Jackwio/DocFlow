using Xunit;

namespace DocFlow.EntityFrameworkCore;

[CollectionDefinition(DocFlowTestConsts.CollectionDefinitionName)]
public class DocFlowEntityFrameworkCoreCollection : ICollectionFixture<DocFlowEntityFrameworkCoreFixture>
{

}
