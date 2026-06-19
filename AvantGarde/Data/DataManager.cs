using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Lumina.Excel.Sheets;

namespace AvantGarde.Data;

public class DataManager
{
    public readonly List<Item> Items;
    public Dictionary<uint, List<int>> CategoryData = new();

    private HttpClient _client = new();
    private static string SpreadsheetUrl =>
        "https://docs.google.com/spreadsheets/d/e/2PACX-1vR26PTxSzanzniszYo3TROm7cxcRDlQclpS6PEHfFy498iiemAfmBK4uLNliQxaCV_huv7W_PAiIB4S/pub?output=csv";

    public DataManager()
    {
        // Get all equipable items relevant for Fashion Report
        Items = Service.DalamudDataManager.GetExcelSheet<Item>()!
            .Where(item => item.EquipSlotCategory.RowId != 0 && item.EquipSlotCategory.Value!.SoulCrystal == 0
                                                             && item.EquipSlotCategory.Value!.MainHand == 0
                                                             && item.EquipSlotCategory.Value!.OffHand == 0).ToList();
        Service.PluginLog.Debug($"Number of items loaded: {Items.Count}");

#pragma warning disable CS4014
        PopulateData();
#pragma warning restore CS4041
    }

    public async Task PopulateData()
    {
        var stream = await this.HttpGetStream(SpreadsheetUrl);
        if (stream != Stream.Null)
            this.ParseCSV(stream);
    }

    private async Task<Stream> HttpGetStream(string url)
    {
        var response = await _client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStreamAsync();
            Service.PluginLog.Debug($"Sheet downloaded with status code {(int)response.StatusCode}");
            return content;
        }
        Service.PluginLog.Error($"Error getting spreadsheet data! {response.ReasonPhrase}");
        return Stream.Null;
    }

    private void ParseCSV(Stream stream)
    {
        using var reader = new StreamReader(stream);

        string? line;
        for (uint lineCount = 0; (line = reader.ReadLine()) != null; lineCount++)
        {
            if (lineCount == 0) { continue; }

            var row = line.Split(',', 2);

            CategoryData[lineCount] = new();
            if (row[1] != "#N/A")
            {
                var ids = row[1].Trim('"').Split(',');
                foreach (var id in ids)
                {
                    CategoryData[lineCount].Add(int.Parse(id));
                }
            }
        }
    }

    public static uint GetCategoryID(string category)
    {
        var themeCategory = Service.DalamudDataManager.GetExcelSheet<FashionCheckThemeCategory>(Service.ClientState.ClientLanguage);
        var matchingCategory = themeCategory?.FirstOrDefault(cat => cat.Name.ExtractText() == category)
            ?? throw new NullReferenceException();
        return matchingCategory.RowId;
    }
}

