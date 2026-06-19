using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;

using AvantGarde.Data;
using AvantGarde.UI;

namespace AvantGarde
{
    public sealed class Plugin : IDalamudPlugin
    {
        private MainWindow _mainWindow;
        private bool _drawUi = false;

        public unsafe Plugin(IDalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();
            _mainWindow = new();

            Service.PluginInterface.UiBuilder.Draw += this.DrawUI;

            Service.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "FashionCheck", (type, args) =>
            {
                var atkValues = new Span<AtkValue>((AtkValue*)((AddonSetupArgs)args).AtkValues, args.Addon.AtkValuesCount);
                var atkData = new FashionCheckAtk(atkValues);

                _mainWindow.Addon = (AtkUnitBase*)args.Addon.Address;
                _mainWindow.AtkData = atkData;

                // DEBUG: Move somewhere else when uploading logic is implemeneted
                var scoreGaugeAddon = Service.GameGui.GetAddonByName("FashionCheckScoreGauge");
                if (scoreGaugeAddon != IntPtr.Zero) {
                    foreach (var item in atkData.GetGoldStamps()) {
                        // Should compare to existing data?
                        Service.PluginLog.Info($"Gold Stamp Found: {item}");
                    }
                }
                _drawUi = true;
            });

            Service.AddonLifecycle.RegisterListener(AddonEvent.PreClose, "FashionCheck", (type, args) => 
            { 
                _mainWindow.Addon = null;
                _mainWindow.AtkData = null;
                _drawUi = false; 
            });
        }

        public void Dispose()
        {
            Service.PluginInterface.UiBuilder.Draw -= this.DrawUI;
            Service.AddonLifecycle.UnregisterListener(AddonEvent.PreClose, "FashionCheck");
            Service.AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "FashionCheck");
        }

        private void DrawUI()
        {
            if (_drawUi)
            {
                _mainWindow.Draw();
            }
        }
    }
}
