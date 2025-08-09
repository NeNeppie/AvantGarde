using System;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;

using AvantGarde.UI;

namespace AvantGarde
{
    public sealed class Plugin : IDalamudPlugin
    {
        private MainWindow _mainWindow;

        public Plugin(IDalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();
            _mainWindow = new();

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
                var baseNode = (AtkUnitBase*)addon.Address;
                if (baseNode->RootNode != null && baseNode->RootNode->IsVisible())
                {
                    _mainWindow.Draw(baseNode);
                }
            }
        }
    }
}
