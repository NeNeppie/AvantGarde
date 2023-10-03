using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;

using AvantGarde.UI;

namespace AvantGarde
{
    public sealed class Plugin : IDalamudPlugin
    {
        private MainWindow MainWindow;

        public Plugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();
            this.MainWindow = new();

            Service.PluginInterface.UiBuilder.Draw += this.DrawUI;
        }

        public void Dispose()
        {
            Service.PluginInterface.UiBuilder.Draw -= this.DrawUI;
        }

        private unsafe void DrawUI()
        {
            var addon = (AtkUnitBase*)Service.GameGui.GetAddonByName("FashionCheck");
            if (addon is null || !addon->IsVisible) { return; }
            this.MainWindow.Draw(addon);
        }
    }
}
