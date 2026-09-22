using Volo.Abp.Settings;

namespace MyMechanicShop.Settings;

public class MyMechanicShopSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(MyMechanicShopSettings.MySetting1));
    }
}
