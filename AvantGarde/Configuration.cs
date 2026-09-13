using System;
using Dalamud.Configuration;

namespace AvantGarde;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool DataCollectionOptedIn = false;
    public bool SeenDataCollectionMessage = false;

    public int SortingMode = 0;

    public bool HighlightOwned = false;
    public bool SortByOwned = false;

    public void Save()
    {
        Service.PluginInterface.SavePluginConfig(this);
    }
}
