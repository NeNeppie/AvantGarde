using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

using AvantGarde.Utils;

namespace AvantGarde.Managers;

public static class UploadManager
{
    private const string UrlBase = "https://infi.ovh/api/";
    private const string AnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJyb2xlIjoiYW5vbiJ9.Ur6wgi_rD4dr3uLLvbLoaEvfLCu4QFWdrF-uHRtbl_s";

    private static readonly HttpClient Client = new();

    static UploadManager()
    {
        Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AnonKey}");
        Client.DefaultRequestHeaders.Add("Prefer", "return=minimal");
    }

    public class UploadRow(Export export)
    {
        [JsonProperty("version")]
        public string Version = Service.PluginInterface.Manifest.AssemblyVersion.ToString();

        [JsonProperty("plugin")]
        public uint Plugin = 1;

        [JsonProperty("week_num")]
        public uint WeekNum = export.WeekNum;

        [JsonProperty("score")]
        public uint Score = export.Score;

        [JsonProperty("hints")]
        public uint[] Hints = export.Categories.SelectMany(cat => cat.Coupled()).ToArray();

        [JsonProperty("items")]
        public uint[] Items = export.ItemIds.ToArray();

        [JsonProperty("dyes")]
        public uint[] Dyes = export.StainIds.ToArray();
    }

    public static async void Upload(UploadRow entry)
    {
        try
        {
            var content = new StringContent(JsonConvert.SerializeObject(entry), Encoding.UTF8, "application/json");
            Service.PluginLog.Debug(content.ReadAsStringAsync().Result);
            var response = await Client.PostAsync($"{UrlBase}FashionReport", content);

            if (response.StatusCode != HttpStatusCode.Created)
            {
                Service.ChatGui.Print(GuiUtilities.BuildUploadErrorMessage());
                Service.PluginLog.Debug($"Content: {response.Content.ReadAsStringAsync().Result}");
            }
        }
        catch (Exception ex)
        {
            Service.PluginLog.Warning(ex, "Failed to upload entry.");
        }
    }
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
