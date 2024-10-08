using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using AvantGarde.Data;

namespace AvantGarde;

internal sealed class Service
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; private set; } = null!;
    [PluginService] public static IDataManager DalamudDataManager { get; private set; } = null!;
    [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] public static IPluginLog PluginLog { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;

    public static DataManager DataManager { get; set; } = null!;

    public Service()
    {
        DataManager = new DataManager();
    }
}
