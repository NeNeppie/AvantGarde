using System.Diagnostics;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

namespace AvantGarde.Utilities;

public static class ImGuiUtils
{
    public static Vector2 IconSize => new(ImGui.GetTextLineHeight() * 2f);
    public static Vector2 SlotWindowSize => new(ImGui.CalcTextSize("A").X * 30f, (IconSize.Y + ImGui.GetStyle().ItemSpacing.Y) * 6f);
    public static float ClipperLineHeight => IconSize.Y + ImGui.GetStyle().ItemSpacing.Y;

    public static void CenterNextElement(Vector2 windowSize, Vector2 elementSize) =>
        ImGui.SetCursorPos((windowSize - elementSize) * 0.5f);

    public static void CenterNextElement(float windowSize, float elementSize) =>
        ImGui.SetCursorPos(new Vector2(windowSize - elementSize) * 0.5f);

    public static bool IconButton(FontAwesomeIcon icon, Vector2 size = default, string tooltip = "", bool small = false)
    {
        var label = icon.ToIconString();

        ImGui.PushFont(UiBuilder.IconFont);
        bool res = small ? ImGui.SmallButton(label) : ImGui.Button(label, size);
        ImGui.PopFont();

        if (tooltip != "" && ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(tooltip);
        }

        return res;
    }

    public static bool HyperlinkButton(string label, string url, Vector2 size = default, bool small = false)
    {
        var res = small ? ImGui.SmallButton(label) : ImGui.Button(label, size);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(url);
        }

        if (res)
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }

        return res;
    }

    public static ImRaii.ChildDisposable BeginButtonWindow(float size, Vector2 position, string label)
    {
        var childSize = size * 1.15f;

        ImGui.SetCursorPos(position);
        var child = ImRaii.Child(label, new Vector2(childSize));
        if (child)
        {
            CenterNextElement(childSize, size);
        }

        return child;
    }
}
