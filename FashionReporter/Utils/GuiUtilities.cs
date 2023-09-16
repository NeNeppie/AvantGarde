using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;

namespace FashionReporter.Utils;

public static class GuiUtilities
{
    public static Vector2 IconSize => new(ImGui.GetTextLineHeight() * 2f);
    public static Vector2 SlotWindowSize => new((IconSize.X + ImGui.GetStyle().ItemSpacing.X) * 5f, (IconSize.Y + ImGui.GetStyle().ItemSpacing.Y) * 6f);

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
