using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using LuminaSupplemental.Excel.Model;
using LuminaSupplemental.Excel.Services;

namespace AvantGarde.Managers;

public class ItemSourceManager : IDisposable
{
    private readonly ExcelSheet<ContentFinderCondition> _sContentFinder;
    private readonly ExcelSheet<ENpcBase> _sENpcBase;
    private readonly ExcelSheet<ENpcResident> _sENpcResident;
    private readonly ExcelSheet<Map> _sMap;
    private readonly ExcelSheet<RecipeLookup> _sRecipeLookup;
    private readonly ExcelSheet<TopicSelect> _sTopicSelect;

    private readonly SubrowExcelSheet<GilShopItem> _sGilShopItem;

    private readonly ImmutableList<uint> _validGearIds = [];
    private readonly FrozenDictionary<uint, List<ItemSource>> _itemSources;

    private Dictionary<uint, Map> _eNpcLocations = [];
    private Dictionary<uint, ItemRelation> _itemEnpcRelation = [];

    public bool IsLoaded { get; private set; } = false;
    
    public ItemSourceManager()
    {
        _sContentFinder = Service.DalamudDataManager.GetExcelSheet<ContentFinderCondition>();
        _sENpcBase = Service.DalamudDataManager.GetExcelSheet<ENpcBase>();
        _sENpcResident = Service.DalamudDataManager.GetExcelSheet<ENpcResident>();
        _sMap = Service.DalamudDataManager.GetExcelSheet<Map>();
        _sRecipeLookup = Service.DalamudDataManager.GetExcelSheet<RecipeLookup>();
        _sTopicSelect = Service.DalamudDataManager.GetExcelSheet<TopicSelect>();

        _sGilShopItem = Service.DalamudDataManager.GetSubrowExcelSheet<GilShopItem>();

        if (Service.DataManager.Items.Count > 0)
        {
            _validGearIds = Service.DataManager.Items.Select(i => i.RowId).ToImmutableList();
            ProcessENpcs();
            _itemSources = CompileSources(Service.DataManager.Items);
            
            IsLoaded = true;
        }
        else
        {
            Service.PluginLog.Warning("DataManager uninitialized, skipping item source lookups.");
            _itemSources = FrozenDictionary<uint, List<ItemSource>>.Empty;
        }
    }

    public void Dispose() { }

    public List<ItemSource> GetItemSources(uint itemId)
    {
        if (_itemSources.TryGetValue(itemId, out var sources))
            return sources;
        return [];
    }

    public FrozenDictionary<uint, List<ItemSource>> CompileSources(IEnumerable<Item> items)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            return BuildSourceLookup(items);
        }
        catch (Exception e)
        {
            Service.PluginLog.Error($"Error compiling item sources: {e.Message}\n{e.StackTrace}");
            return FrozenDictionary<uint, List<ItemSource>>.Empty;
        }
        finally
        {
            stopwatch.Stop();
            Service.PluginLog.Debug($"SourceManager.CompileSources done in {stopwatch.Elapsed.TotalMilliseconds}ms");
        }
    }

    private FrozenDictionary<uint, List<ItemSource>> BuildSourceLookup(IEnumerable<Item> items)
    {
        var storeItems = CsvLoader.LoadResource<StoreItem>(CsvLoader.StoreItemResourceName, false, out _, out _, Service.DalamudDataManager.GameData)
            .DistinctBy(i => i.ItemId)
            .ToDictionary(i => i.ItemId, i => i.StoreId);

        var dungeonBossChestItems = CsvLoader.LoadResource<DungeonBossChest>(CsvLoader.DungeonBossChestResourceName, false, out _, out _, Service.DalamudDataManager.GameData)
            .DistinctBy(i => i.ItemId)
            .ToDictionary(i => i.ItemId, i => i.ContentFinderConditionId);

        var pairs = new List<KeyValuePair<uint, List<ItemSource>>>();
        foreach (var item in items)
        {
            var itemId = item.RowId;
            var pair = new KeyValuePair<uint, List<ItemSource>>(itemId, []);

            // Crafting source
            if (_sRecipeLookup.TryGetRow(itemId, out var row))
            {
                RowRef<Recipe>[] recipes = [
                    row.CRP, row.BSM, row.ARM, row.GSM,
                    row.LTW, row.WVR, row.ALC, row.CUL
                ];
                if (recipes.FirstOrDefault(row => row.RowId != 0) is { } recipe)
                {
                    pair.Value.Add(new ItemSourceCrafting(itemId, recipe.Value));
                }
            }

            // Special Shop source (Exchange)


            // Regular Shop source (Gil)
            if (_itemEnpcRelation.TryGetValue(itemId, out var relation))
            {
                pair.Value.Add(new ItemSourceShop(relation.GilShopId, item.PriceMid, relation.ResidentName, relation.Location));
            }

            // Marketboard source
            if (item.ItemSearchCategory.RowId != 0)
            {
                pair.Value.Add(new ItemSourceMarketboard());
            }

            // Online Store source (Mogstore)
            if (storeItems.TryGetValue(itemId, out var storeId))
            {
                pair.Value.Add(new ItemSourceMogstore(storeId));
            }

            // Desynthesis source


            // Loot Coffer source (Coffer)


            // Duty Drop source (Drop)
            if (dungeonBossChestItems.TryGetValue(itemId, out var contentFinderConditionId))
            {
                var contentFinderCondition = _sContentFinder.GetRow(contentFinderConditionId);
                pair.Value.Add(new ItemSourceDrop(contentFinderCondition));           
            } // incomplete

            // Achievement Reward source


            // Venture Loot source


            // Field Op Lockbox source


            // Overworld Treasure Chest source

            pairs.Add(pair);
        }

        return pairs.ToFrozenDictionary();
    }

    private void ProcessENpcs()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var levels = Service.DalamudDataManager.GetExcelSheet<Level>().Where(level => level.Type == 8);
        foreach (var level in levels)
        {
            var enpc = level.Object.RowId;
            if (!_eNpcLocations.ContainsKey(enpc))
                _eNpcLocations[enpc] = level.Map.Value;
        }

        var levelsSupplemental = CsvLoader.LoadResource<ENpcPlace>(CsvLoader.ENpcPlaceResourceName, false, out _,  out _, Service.DalamudDataManager.GameData);
        foreach (var level in levelsSupplemental)
        {
            var enpc = level.ENpcResidentId;
            if (!_eNpcLocations.ContainsKey(enpc))
                _eNpcLocations[enpc] = _sMap.GetRow(level.MapId);
        }

        foreach (var npcBase in _sENpcBase)
        {
            var residentData = _sENpcResident.GetRow(npcBase.RowId);
            if (!_eNpcLocations.TryGetValue(npcBase.RowId, out var map))
                continue;

            foreach (var data in npcBase.ENpcData.Select(i => i.RowId))
            {
                if (data >> 16 == 0x0004) // GilShop
                    AddGilShopItems(data, residentData, map);

                else if (data >> 16 == 0x0032) // TopicSelect (Goes into more GilShops)
                {
                    var topic = _sTopicSelect.GetRow(data);
                    foreach (var topicShop in topic.Shop.Select(i => i.RowId))
                    {
                        if (topicShop >> 16 == 0x0004)
                            AddGilShopItems(topicShop, residentData, map);
                    }
                }
            }
        }

        stopwatch.Stop();
        Service.PluginLog.Debug($"Enpcs processed in {stopwatch.Elapsed.TotalMilliseconds}ms");
    }

    private void AddGilShopItems(uint shopId, ENpcResident resident, Map map)
    {
        var shopItems = _sGilShopItem.GetRow(shopId);
        var items = shopItems.Select(subrow => subrow.Item.RowId)
                             .Where(_validGearIds.Contains);

        foreach (var item in items)
            AddItemRelation(item, shopId, resident, map);
    }

    private void AddItemRelation(uint itemId, uint gilShopId, ENpcResident resident, Map map)
    {
        var residentName = resident.Singular.ExtractText();
        var mapName = map.PlaceName.Value.Name.ExtractText();

        if (!_itemEnpcRelation.ContainsKey(itemId))
            _itemEnpcRelation[itemId] = new ItemRelation(gilShopId, residentName, mapName);
    }

    private record ItemRelation(uint GilShopId, string ResidentName, string Location);
}
