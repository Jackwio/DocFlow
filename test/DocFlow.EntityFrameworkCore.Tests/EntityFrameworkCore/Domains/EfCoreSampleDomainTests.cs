using DocFlow.Samples;
using Xunit;

namespace DocFlow.EntityFrameworkCore.Domains;

[Collection(DocFlowTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<DocFlowEntityFrameworkCoreTestModule>
{

}
