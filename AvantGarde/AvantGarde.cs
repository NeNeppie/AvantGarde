using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

using AvantGarde.Data;
using AvantGarde.UI;

namespace AvantGarde;

public sealed class Plugin : IDalamudPlugin
{
    private MainWindow _mainWindow;
    private DataCollectionWindow _infoWindow;
    private bool _drawUi = false;

    public unsafe Plugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        _mainWindow = new();
        _infoWindow = new();

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

            TryExportFashionAttempt();

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
        Service.PluginConfig.Save();
    }

    private static unsafe void TryExportFashionAttempt()
    {
        var agentFashion = AgentFashion.Instance();
        if (agentFashion->OpenType != AgentFashionOpenType.Result)
            return;

        var exportObj = new Export();
        exportObj.WeekNum = agentFashion->FashionCheckData.WeeklyTheme - 9u;
        exportObj.Score = agentFashion->FashionCheckData.Score;

        var hints = agentFashion->FashionCheckData.ItemThemes;
        var stamps = agentFashion->FashionCheckData.ItemEvaluations;
        var items = agentFashion->Items;
    
        if (hints.Length != stamps.Length)
            return;

        for (int i = 0; i < hints.Length; i++)
        {
            exportObj.Categories.Add(new Category(hints[i], stamps[i]));
        }

        for (int i = 0; i < items.Length; i++)
        {
            var itemId = items[i].ItemId;
            exportObj.ItemIds.Add(itemId);
            
            var id = Service.DalamudDataManager.GetExcelSheet<Item>().GetRow(itemId).EquipSlotCategory.RowId;
            if ((id >= 3 && id <= 8) || id == 1 || id == 2 || id == 13)
            {
                exportObj.StainIds.AddRange(items[i].Stain0Id, items[i].Stain1Id);
            }
        }

        UploadManager.UploadRow upload = new(exportObj);
        UploadManager.Upload(upload);
    }

    private void DrawUI()
    {
        if (_drawUi)
        {
            _mainWindow.Draw();
            if (!Service.PluginConfig.SeenDataCollectionMessage)
            {
                _infoWindow.Draw();
            }
        }
    }
}

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool DataCollectionOptedIn = false;
    public bool SeenDataCollectionMessage = false;

    public void Save()
    {
        Service.PluginInterface.SavePluginConfig(this);
    }
}
