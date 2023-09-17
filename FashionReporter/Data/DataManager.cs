using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Dalamud.Logging;

namespace FashionReporter.Data;

public class DataManager
{
    public Dictionary<string, List<int>> Data = new();

    private HttpClient Client = new();
    private static string SpreadsheetUrl =>
        "https://docs.google.com/spreadsheets/d/e/2PACX-1vR26PTxSzanzniszYo3TROm7cxcRDlQclpS6PEHfFy498iiemAfmBK4uLNliQxaCV_huv7W_PAiIB4S/pub?output=csv";

    public DataManager()
    {
#pragma warning disable CS4014
        this.PopulateData();
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
        var response = await Client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStreamAsync();
            PluginLog.Debug($"Sheet downloaded with status code {(int)response.StatusCode}");
            return content;
        }
        PluginLog.Error("Error getting spreadsheet data!");
        return Stream.Null;
    }

    private void ParseCSV(Stream stream)
    {
        using var reader = new StreamReader(stream);

        int lineCount = 0;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (lineCount++ == 0) { continue; }

            var row = line.Split(',', 2);

            this.Data[row[0]] = new();
            if (row[1] != "#N/A")
            {
                var ids = row[1].Trim('"').Split(',');
                foreach (var id in ids)
                {
                    this.Data[row[0]].Add(int.Parse(id));
                }
            }
        }
    }
}

