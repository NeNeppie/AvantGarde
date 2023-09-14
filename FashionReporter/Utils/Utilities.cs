using System.Numerics;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace FashionReporter.Utils;

public static class Utilities
{
    public static unsafe Vector2 GetNodePosAbsolute(AtkUnitBase* addon, AtkResNode* node)
    {
        return new Vector2(node->X * addon->Scale, node->Y * addon->Scale);
    }

    public static bool IconButton(FontAwesomeIcon icon, Vector2 size = default, string? tooltip = null, bool small = false)
    {
        var label = icon.ToIconString();

        ImGui.PushFont(UiBuilder.IconFont);
        bool res = small ? ImGui.SmallButton(label) : ImGui.Button(label, size);
        ImGui.PopFont();

        if (tooltip != null && ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(tooltip);
        }

        return res;
    }
}
