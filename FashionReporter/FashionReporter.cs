using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;

using FashionReporter.UI;

namespace FashionReporter
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Fashion Reporter";

        private MainWindow MainWindow;

        public Plugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();

            this.MainWindow = new MainWindow();

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
