using DocFlow.Samples;
using Xunit;

namespace DocFlow.EntityFrameworkCore.Applications;

[Collection(DocFlowTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<DocFlowEntityFrameworkCoreTestModule>
{

}
