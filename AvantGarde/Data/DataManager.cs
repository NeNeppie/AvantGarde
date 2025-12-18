using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Lumina.Excel.Sheets;

namespace AvantGarde.Data;

/*
TODO: Structure of FashionCheck AtkValues (as of Endwalker), 
    for future automatic submission implementation:
    [0] Weekly Theme
    [1] Sub-struct for every gear piece
        [0] Bool - Hint active
        [1] String - Hint
------> [2] UInt - Score stamp (0 is Gold, 4 is Low, 5 is None)
        [3] UInt - unk
        [4] String - Slot
        [5] UInt - unk
        [6] UInt - unk
------> [7] UInt - Dye ID
        [8] UInt - unk (Connected to dye)
        [9] Bool - unk
        [10] Bool - unk
    [122] Remaining Attempts
    [123] High Score
    [124-132] Irrelevant
*/

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
}

