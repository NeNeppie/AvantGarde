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
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;

    public static DataManagerNew DataManager { get; set; } = null!;
    public static Configuration PluginConfig { get; set; } = null!;

    public Service()
    {
        PluginConfig = (Configuration)PluginInterface.GetPluginConfig()! ?? new Configuration();
        DataManager = new DataManagerNew();
    }
}
