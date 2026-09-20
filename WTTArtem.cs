using System.Reflection;
using JetBrains.Annotations;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Utils;
using Path = System.IO.Path;

namespace WTTArtem;

[Injectable(TypePriority = OnLoadOrder.Preload + 2), UsedImplicitly]
public class WTTArtem(
    ModHelper modHelper,
    ImageRouter imageRouter,
    TraderConfig traderConfig,
    RagfairConfig ragfairConfig,
    TimeUtil timeUtil,
    WTTArtemHelper wttArtemHelper,
    WTTServerCommonLib.WTTServerCommonLib wttCommon
) : IOnLoad
{

    public  async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var assembly = Assembly.GetExecutingAssembly();
        
        await wttCommon.CustomItemParentService.CreateCustomParents(assembly);
        await wttCommon.CustomItemServiceExtended.CreateCustomItems(assembly);
        await wttCommon.CustomQuestZoneService.CreateCustomQuestZones(assembly);
        
        var traderImagePath = Path.Combine(pathToMod, "res/66bf757f27d0b097db0acea5.jpg");
        var traderBase = modHelper.GetJsonDataFromFile<TraderBase>(pathToMod, "db/base.json");
        
        imageRouter.AddRoute(traderBase.Avatar!.Replace(".jpg", ""), traderImagePath);
        WTTArtemHelper.SetTraderUpdateTime(traderConfig, traderBase, timeUtil.GetHoursAsSeconds(1), timeUtil.GetHoursAsSeconds(2));
        
        ragfairConfig.Traders.TryAdd(traderBase.Id, true);
        
        wttArtemHelper.AddTraderWithEmptyAssortToDb(traderBase);
        
        wttArtemHelper.AddTraderToLocales(traderBase, "Artem", "[REDACTED]");
        
        await wttCommon.CustomQuestService.CreateCustomQuests(assembly);
        await wttCommon.CustomClothingService.CreateCustomClothing(assembly);
        
        var assort = modHelper.GetJsonDataFromFile<TraderAssort>(pathToMod, "db/assort.json");

        
        wttArtemHelper.OverwriteTraderAssort(traderBase.Id, assort);

        
        await Task.CompletedTask;
    }
}