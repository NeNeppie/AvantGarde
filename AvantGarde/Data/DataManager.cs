using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;

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
}

public class Export
{
    public uint WeekNum;
    public uint Score;
    public List<Category> Categories = [];
    public List<uint> ItemIds = [];
    public List<uint> StainIds = [];
}

public record Category(uint HintId, uint StampId)
{
    public uint[] Coupled() => [HintId, StampId];
};

public static class UploadManager
{
    private const string UrlBase = "https://infi.ovh/api/";
    private const string AnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJyb2xlIjoiYW5vbiJ9.Ur6wgi_rD4dr3uLLvbLoaEvfLCu4QFWdrF-uHRtbl_s";

    private static readonly HttpClient _client = new();

    static UploadManager()
    {
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AnonKey}");
        _client.DefaultRequestHeaders.Add("Prefer", "return=minimal");
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public class UploadRow
    {
        [JsonProperty("version")]
        public string Version = Service.PluginInterface.Manifest.AssemblyVersion.ToString();

        [JsonProperty("plugin")]
        public uint Plugin = 1;

        [JsonProperty("week_num")]
        public uint WeekNum;

        [JsonProperty("score")]
        public uint Score;

        [JsonProperty("hints")]
        public uint[] Hints;

        [JsonProperty("items")]
        public uint[] Items;

        [JsonProperty("dyes")]
        public uint[] Dyes;

        public UploadRow(Export export)
        {
            WeekNum = export.WeekNum;
            Score = export.Score;

            Hints = export.Categories.SelectMany(cat => cat.Coupled()).ToArray();
            Items = export.ItemIds.ToArray();
            Dyes = export.StainIds.ToArray();
        }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public static async void Upload(UploadRow entry)
    {
        try
        {
            var content = new StringContent(JsonConvert.SerializeObject(entry), Encoding.UTF8, "application/json");
            Service.PluginLog.Debug(content.ReadAsStringAsync().Result);
            var response = await _client.PostAsync($"{UrlBase}FashionReport", content);

            if (response.StatusCode != HttpStatusCode.Created)
                Service.PluginLog.Debug($"Content: {response.Content.ReadAsStringAsync().Result}");
        }
        catch (Exception ex)
        {
            Service.PluginLog.Warning(ex, "Failed to upload entry.");
        }
    }
}
