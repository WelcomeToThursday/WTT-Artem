using System.Reflection;
using JetBrains.Annotations;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using Path = System.IO.Path;

namespace WTTArtem;

[Injectable(TypePriority = OnLoadOrder.Preload + 1), UsedImplicitly]
public class WTTArtemTrader(
    ModHelper modHelper,
    ImageRouter imageRouter,
    TraderConfig traderConfig,
    RagfairConfig ragfairConfig,
    TimeUtil timeUtil,
    ISptLogger<WTTArtemTrader> logger,
    ICloner cloner,
    TradersTable tradersTable,
    LocaleTable localesTable)
{
    public string Register(Assembly assembly)
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(assembly);
        var traderBase = modHelper.GetJsonDataFromFile<TraderBase>(pathToMod, "db/base.json");
        var traderImagePath = Path.Combine(pathToMod, "res", "66bf757f27d0b097db0acea5.jpg");

        imageRouter.AddRoute(traderBase.Avatar!.Replace(".jpg", ""), traderImagePath);
        traderConfig.UpdateTime.Add(new UpdateTime
        {
            TraderId = traderBase.Id,
            Seconds = new MinMax<int>(timeUtil.GetHoursAsSeconds(1), timeUtil.GetHoursAsSeconds(2))
        });

        ragfairConfig.Traders.TryAdd(traderBase.Id, true);
        AddTraderWithEmptyAssort(traderBase);
        AddTraderToLocales(traderBase);

        return traderBase.Id;
    }

    public void LoadAssort(Assembly assembly, string traderId)
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(assembly);
        var assort = modHelper.GetJsonDataFromFile<TraderAssort>(pathToMod, "db/assort.json");

        if (!tradersTable.TryGetValue(traderId, out var trader))
        {
            logger.Warning($"Unable to update assorts for trader: {traderId}, they couldn't be found on the server");
            return;
        }

        trader.Assort = assort;
    }

    private void AddTraderWithEmptyAssort(TraderBase traderBase)
    {
        var trader = new Trader
        {
            Assort = new TraderAssort
            {
                Items = [],
                BarterScheme = [],
                LoyalLevelItems = []
            },
            Base = cloner.Clone(traderBase)!,
            QuestAssort = new Dictionary<string, Dictionary<MongoId, MongoId>>
            {
                ["Started"] = [],
                ["Success"] = [],
                ["Fail"] = []
            },
            Dialogue = []
        };

        if (!tradersTable.TryAdd(traderBase.Id, trader))
        {
            logger.Warning($"Unable to register trader: {traderBase.Id}, an entry already exists");
        }
    }

    private void AddTraderToLocales(TraderBase traderBase)
    {
        var traderId = traderBase.Id;
        var fullName = traderBase.Name;
        var nickname = traderBase.Nickname;
        var location = traderBase.Location;

        foreach (var (_, locale) in localesTable.Global)
        {
            locale.AddTransformer(localeData =>
            {
                localeData!.Add($"{traderId} FullName", fullName);
                localeData.Add($"{traderId} FirstName", "Artem");

                if (nickname is not null) { localeData.Add($"{traderId} Nickname", nickname); }
                if (location is not null) { localeData.Add($"{traderId} Location", location); }

                localeData.Add($"{traderId} Description", "[REDACTED]");
                return localeData;
            });
        }
    }
}
