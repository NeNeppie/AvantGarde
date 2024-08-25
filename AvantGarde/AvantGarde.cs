using System;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;

using AvantGarde.UI;

namespace AvantGarde
{
    public sealed class Plugin : IDalamudPlugin
    {
        private MainWindow MainWindow;

        public Plugin(IDalamudPluginInterface pluginInterface)
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
            var addon = Service.GameGui.GetAddonByName("FashionCheck");
            if (addon != IntPtr.Zero)
            {
                var baseNode = (AtkUnitBase*)addon; 
                if (baseNode->RootNode != null && baseNode->RootNode->IsVisible())
                {
                    this.MainWindow.Draw(baseNode);
                }
            }
        }
    }
}
