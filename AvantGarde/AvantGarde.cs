using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

using AvantGarde.Managers;
using AvantGarde.UI;

namespace AvantGarde;

public sealed class Plugin : IDalamudPlugin
{
    private MainWindow _mainWindow;
    private DataCollectionWindow _infoWindow;
    private bool _drawUi = false;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        _mainWindow = new();
        _infoWindow = new();

        Service.PluginInterface.UiBuilder.Draw += this.DrawUI;

        Service.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "FashionCheck", OnFashionCheckPostSetup);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "FashionCheck", TryExportFashionAttempt);

        Service.AddonLifecycle.RegisterListener(AddonEvent.PreClose, "FashionCheck", OnFashionCheckDispose);
    }

    public void Dispose()
    {
        Service.PluginInterface.UiBuilder.Draw -= this.DrawUI;
        Service.AddonLifecycle.UnregisterListener(AddonEvent.PreClose, "FashionCheck");
        Service.AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "FashionCheck");
        Service.PluginConfig.Save();
    }

    private unsafe void OnFashionCheckPostSetup(AddonEvent type, AddonArgs args)
    {
        _mainWindow.Addon = (AtkUnitBase*)args.Addon.Address;
        _drawUi = true;
    }

    private unsafe void OnFashionCheckDispose(AddonEvent type, AddonArgs args)
    {
        _mainWindow.Addon = null;
        _drawUi = false;
    }

    private unsafe void TryExportFashionAttempt(AddonEvent type, AddonArgs args)
    {
        var agentFashion = AgentFashion.Instance();
        if (agentFashion->OpenType != AgentFashionOpenType.Result)
            return;

        var exportObj = new Export
        {
            WeekNum = agentFashion->FashionCheckData.WeeklyTheme - 9u,
            Score = agentFashion->FashionCheckData.Score
        };

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
            var itemId = ItemUtil.GetBaseId(items[i].ItemId).ItemId;
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
