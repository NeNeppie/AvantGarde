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
                if (atkValues.Length != FashionCheckAtk.AtkValueCount)
                {
                    Service.PluginLog.Error("Failure to initialize window - AtkValues count mismatch");
                    return;
                }

                var atkData = new FashionCheckAtk(atkValues);
                var scoreGaugeAddon = Service.GameGui.GetAddonByName("FashionCheckScoreGauge");
                if (scoreGaugeAddon != IntPtr.Zero)
                {
                    var score = ((AtkUnitBase*)scoreGaugeAddon.Address)->AtkValues[0].UInt;
                    ExportFashionAttempt(atkData, score);
                }

                _mainWindow.Addon = (AtkUnitBase*)args.Addon.Address;
                _mainWindow.AtkData = atkData;
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

        private static void ExportFashionAttempt(FashionCheckAtk atk, uint score)
        {
            var exportObj = atk.Export();
            exportObj.WeekNum = DataManager.GetWeekNumFromTheme(atk.WeeklyTheme);
            exportObj.Score = score;

            UploadManager.UploadRow upload = new(exportObj);
            UploadManager.Upload(upload);
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
