using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Component.GUI;

using AvantGarde.Utilities;

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

        ImGui.TextColored(ImGuiColors.DalamudYellow, "General");

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
                    {
                        Service.PluginConfig.SortingMode = (int)choice;
                        Service.PluginConfig.Save();
                    }
                }
            }
        }
        ImGuiHelpers.ScaledDummy(5f);

        ImGui.TextColored(ImGuiColors.DalamudYellow, "Allagan Tools");
        ImGui.TextDisabled("These require Allagan Tools to be enabled");

        var highlightOwned = Service.PluginConfig.HighlightOwned;
        ImGui.Checkbox("Highlight items you own", ref highlightOwned);
        if (highlightOwned != Service.PluginConfig.HighlightOwned)
        {
            Service.PluginConfig.HighlightOwned = highlightOwned;
            Service.PluginConfig.Save();
        }

        var sortByOwned = Service.PluginConfig.SortByOwned;
        ImGui.Checkbox("Sort additionally by items owned", ref sortByOwned);
        if (sortByOwned != Service.PluginConfig.SortByOwned)
        {
            Service.PluginConfig.SortByOwned = sortByOwned;
            Service.PluginConfig.Save();
        }

        ImGuiHelpers.ScaledDummy(25f);

        ImGui.TextColored(ImGuiColors.DalamudYellow, "About");
        ImGui.Text($"Version: {Service.PluginInterface.Manifest.AssemblyVersion}");

        using (var color = ImRaii.PushColor(ImGuiCol.Button, ImGuiUtils.ColorDiscordBlurple))
            ImGuiUtils.HyperlinkButton("Discord Forum Post", "https://discord.com/channels/581875019861328007/1166794253553381456");

        ImGui.SameLine();

        using (var color = ImRaii.PushColor(ImGuiCol.Button, ImGuiUtils.ColorGithubOrange))
            ImGuiUtils.HyperlinkButton("Github Issues", "https://github.com/NeNeppie/AvantGarde/issues");

        ImGuiHelpers.ScaledDummy(5f);
        ImGui.End();
    }

    private Vector2 GetWindowPosition()
    {
        return new Vector2(Addon->X + Addon->GetScaledWidth(true), Addon->Y) + (new Vector2(-30f, 40f) * Addon->Scale);
    }
}
