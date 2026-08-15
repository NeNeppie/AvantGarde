using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;

using AvantGarde.Utils;

namespace AvantGarde.Managers;

public class DataManager
{
    public readonly List<Item> Items;
    public Dictionary<uint, List<(uint Id, uint Count)>> CategoryData = [];
    public Dictionary<uint, List<(uint Id, ulong Count, float Pct)>> DyeData = [];

    private static readonly HttpClient Client = new();
    private static readonly string[] DataUrls = [
        "https://raw.githubusercontent.com/Infiziert90/FFXIVGachaSpreadsheet/refs/heads/master/website/static/data/FashionReport.json",
        "https://xivstats.com/data/FashionReport.json"
    ];

    public DataManager()
    {
        // Get all equipable items relevant for Fashion Report
        Items = Service.DalamudDataManager.GetExcelSheet<Item>()!
            .Where(item => item.EquipSlotCategory.RowId != 0 && item.EquipSlotCategory.Value!.SoulCrystal == 0
                                                             && item.EquipSlotCategory.Value!.MainHand == 0
                                                             && item.EquipSlotCategory.Value!.OffHand == 0).ToList();
        Service.PluginLog.Debug($"Number of items loaded: {Items.Count}");

        Client.DefaultRequestHeaders.Add("Accept", "applcation/json");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        PopulateData();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    public async Task PopulateData()
    {
        CategoryData.Clear();

        foreach (var url in DataUrls)
        {   
            var res = await Client.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                Service.PluginLog.Error($"Failed to fetch data from: {url} , {res.ReasonPhrase}");
                continue;
            }
            
            try
            {
                var json = await res.Content.ReadAsStringAsync();
                var importData = JsonConvert.DeserializeObject<ImportData>(json) ?? throw new JsonException();

                foreach (var category in importData.Categories)
                {
                    CategoryData.Add(category.Key, category.Value.Select((pair) => (pair.Key, pair.Value)).ToList());
                }
                CategoryData = CategoryData.OrderBy(cat => cat.Key).ToDictionary();

                var weekNum = GetFashionReportWeek();
                if (importData.WeeklyDyes.TryGetValue(weekNum, out var weeklyDyeData))
                {
                    foreach (var slotData in weeklyDyeData) 
                    {
                        if (NormalizeSlotID(slotData.Id) is not uint slotId)
                            continue;

                        DyeData[slotId] = [];
                        var slot = DyeData[slotId!];

                        foreach(var dye in slotData.Dyes)
                        {
                            slot.Add((dye.Key, dye.Value.Count, dye.Value.Pct));
                        }
                        // Should already be sorted, but just in case
                        slot = slot.OrderByDescending(x => x.Pct).ToList();
                    }
                }

                Service.PluginLog.Debug($"Data fetched with status code {(int)res.StatusCode} from: {url}");
                break;
            }
            catch (Exception ex)
            {
                Service.PluginLog.Error(ex, $"Failed to fetch data from: {url}");
            }
        }
    }

    /// <summary>
    /// Returns the corresponding row ID of a category name.
    /// </summary>
    /// <param name="category">Category name in client language</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static uint GetCategoryID(string category)
    {
        var themeCategory = Service.DalamudDataManager.GetExcelSheet<FashionCheckThemeCategory>(Service.ClientState.ClientLanguage);
        var matchingCategory = themeCategory?.FirstOrDefault(cat => cat.Name.ExtractText() == category)
            ?? throw new NullReferenceException();
        return matchingCategory.RowId;
    }

    /// <summary>
    /// Returns the corresponding row ID of a weekly theme name.
    /// Subtract by 9 to get the week number this theme ran on.
    /// </summary>
    /// <param name="weeklyTheme">Weekly theme name in client language</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static uint GetWeeklyThemeID(string weeklyTheme)
    {
        var sheet = Service.DalamudDataManager.GetExcelSheet<FashionCheckWeeklyTheme>(Service.ClientState.ClientLanguage);
        var matchingRow = sheet?.FirstOrDefault(theme => theme.Name.ExtractText() == weeklyTheme)
            ?? throw new NullReferenceException();
        return matchingRow.RowId;
    }

    public static uint GetWeekNumFromTheme(string weeklyTheme) => GetWeeklyThemeID(weeklyTheme) - 9;

    private static uint? NormalizeSlotID(uint slotId)
    {
        return slotId switch
        {
            // TODO:
            // 1 => (uint)ItemSlot.Weapon,
            34 => (uint)ItemSlot.Head,
            35 => (uint)ItemSlot.Body,
            37 => (uint)ItemSlot.Hands,
            36 => (uint)ItemSlot.Legs,
            38 => (uint)ItemSlot.Feet,
            _ => null
        };
    }

    private static uint GetFashionReportWeek()
    {
        var weekOne = new DateTime(2018, 1, 30);
        weekOne = DateTime.SpecifyKind(weekOne, DateTimeKind.Utc).AddHours(8);

        var today = DateTime.UtcNow;
        var diff = today - weekOne;
        var weeks = (diff.TotalDays / 7) + 1;
        return (uint)weeks;
    }

    public class ImportData
    {
        public Dictionary<uint, Dictionary<uint, uint>> Categories = [];
        public Dictionary<uint, List<DyeSlotInfo>> WeeklyDyes = [];

        public class DyeSlotInfo
        {
            public uint Id;
            public string Name = string.Empty;
            public Dictionary<uint, DyeInfo> Dyes = [];
        }

        public class DyeInfo
        {
            public ulong Count;
            public float Pct;
        }
    }
}
