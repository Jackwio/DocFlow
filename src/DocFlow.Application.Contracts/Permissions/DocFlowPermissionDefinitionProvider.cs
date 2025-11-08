using DocFlow.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace DocFlow.Permissions;

public class DocFlowPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(DocFlowPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(DocFlowPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<DocFlowResource>(name);
    }
}
