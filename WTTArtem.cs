using System.Reflection;
using JetBrains.Annotations;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using WTTServerCommonLib.Models;

namespace WTTArtem;

[Injectable(TypePriority = OnLoadOrder.Preload + 2), UsedImplicitly]
public class WTTArtem(WTTArtemTrader trader, WTTServerCommonLib.WTTServerCommonLib wttCommon, ModHelper modHelper) : IOnLoad
{
    private readonly byte[] _devBytes = [0x80, 0x8F, 0x92, 0xA1, 0xB7, 0xC3, 0xD5, 0xE2, 0xF0, 0x9A, 0xAD, 0xBE, 0xC8, 0xD1, 0xE7, 0xFF];
    
    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var modPath = modHelper.GetAbsolutePathToModFolder(assembly);
        var traderId = trader.Register(assembly);
        TraderIds.Add(traderId, "ARTEM");
        
        var flag = File.Exists($"{modPath}\\flag") && await File.ReadAllBytesAsync($"{modPath}\\flag", cancellationToken) == _devBytes;
        
        await wttCommon.CustomItemParentService.CreateCustomParents(assembly);
        await wttCommon.CustomItemServiceExtended.CreateCustomItems(assembly);
        await wttCommon.CustomQuestZoneService.CreateCustomQuestZones(assembly);
        await wttCommon.CustomQuestService.CreateCustomQuests(assembly);
        await wttCommon.CustomClothingService.CreateCustomClothing(assembly);
        
        if (flag)
        {
            await wttCommon.CustomItemServiceExtended.CreateCustomItems(assembly, "DevAssets\\CustomItems");
            await wttCommon.CustomClothingService.CreateCustomClothing(assembly, "DevAssets\\CustomClothing");
        }
        
        trader.LoadAssort(assembly, traderId);
    }
}
