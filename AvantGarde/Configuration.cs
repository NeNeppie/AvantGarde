using System;
using System.Text.Json.Serialization;
using Dalamud.Configuration;

namespace AvantGarde;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool DataCollectionOptedIn = false;
    public bool SeenDataCollectionMessage = false;

    public int SortingMode = 0;

    [property: JsonIgnore]
    public bool HighlightOwned
    {
        get
        {
            return field ^ false; // TEMP: Placeholder for Allagan Tools IPC
        }
        set;
    } = false;

    public void Save()
    {
        Service.PluginInterface.SavePluginConfig(this);
    }
}
