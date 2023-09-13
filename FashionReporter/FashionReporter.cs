using System;
using System.Collections.Generic;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;

using FashionReporter.Windows;

namespace FashionReporter
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Fashion Reporter";

        // TODO: Implement own windowSystem to make menu-only windows (no outside background)
        public WindowSystem WindowSystem = new("FashionReporter");
        private Dictionary<ItemSlot, MainWindow> Windows = new();

        public Plugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();

            // TODO: Current idea is to iterate over ItemSlot and init a list of windows, to then add to the system
            //       Another idea is to have a primary window with buttons. pressing a button opens a secondary window. Total window count goes from 10 to 2.
            foreach (var slot in Enum.GetValues<ItemSlot>())
            {
                var window = new MainWindow(slot);
                this.Windows[slot] = window;
                this.WindowSystem.AddWindow(window);
            }

            Service.PluginInterface.UiBuilder.Draw += this.DrawUI;
        }

        public void Dispose()
        {
            Service.PluginInterface.UiBuilder.Draw -= this.DrawUI;
            this.WindowSystem.RemoveAllWindows();
        }

        private unsafe void DrawUI()
        {
            var addon = (AtkUnitBase*)Service.GameGui.GetAddonByName("FashionCheck");
            if (addon is null || !addon->IsVisible) { return; }
            this.WindowSystem.Draw();
        }
    }
}
