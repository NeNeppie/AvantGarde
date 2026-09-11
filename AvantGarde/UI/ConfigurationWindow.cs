using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AvantGarde.UI;

public unsafe class ConfigurationWindow
{
    public AtkUnitBase* Addon = null;

    private static readonly ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove;
    private bool _isVisible = false;

    public void ToggleVisible() => _isVisible = !_isVisible;

    public void Draw()
    {
        if (!_isVisible)
            return;

        var pos = GetWindowPosition();
        ImGuiHelpers.ForceNextWindowMainViewport();
        ImGuiHelpers.SetNextWindowPosRelativeMainViewport(pos);

        if (!ImGui.Begin("Avant-Garde Settings##avantgarde-settings", WindowFlags))
        {
            ImGui.End();
            return;
        }

        ImGui.Checkbox("Opt-in to data collection", ref Service.PluginConfig.DataCollectionOptedIn);
        ImGuiHelpers.ScaledDummy(5f);

        ImGui.Text("Sort items by:");
        var currentValue = ((SortingMode)Service.PluginConfig.SortingMode).GetDescription();
        var choices = Enum.GetValues<SortingMode>();
        using (var combo = ImRaii.Combo("##sortingmode-combo", currentValue))
        {
            if (combo.Success)
            {
                foreach (var choice in choices)
                {
                    var label = choice.GetDescription();

                    if (ImGui.Selectable(label, label == currentValue))
                        Service.PluginConfig.SortingMode = (int)choice;
                }
            }
        }
        ImGuiHelpers.ScaledDummy(5f);

        ImGui.End();
    }

    private Vector2 GetWindowPosition()
    {
        return new Vector2(Addon->X + Addon->GetScaledWidth(true), Addon->Y) + (new Vector2(-30f, 40f) * Addon->Scale);
    }
}
