using Volo.Abp.Settings;

namespace DocFlow.Settings;

public class DocFlowSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(DocFlowSettings.MySetting1));
    }
}
