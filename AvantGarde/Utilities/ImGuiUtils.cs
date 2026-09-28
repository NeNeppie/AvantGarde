using System.Diagnostics;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace AvantGarde.Utilities;

public static class ImGuiUtils
{
    public static Vector2 IconSize => new(ImGui.GetTextLineHeight() * 2f);
    public static Vector2 SlotWindowSize => new(ImGui.CalcTextSize("A").X * 30f, (IconSize.Y + ImGui.GetStyle().ItemSpacing.Y) * 6f);
    public static float ClipperLineHeight => IconSize.Y + ImGui.GetStyle().ItemSpacing.Y;

    #region colors
    public static readonly Vector4 ColorDiscordBlurple = new(0.34f, 0.40f, 0.95f, 1.0f); // #5865f2
    public static readonly Vector4 ColorGithubOrange = new(0.50f, 0.12f, 0.06f, 1.0f); // #801E0F

    public static Vector4 GetDefaultColor(ImGuiCol col) => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(col));

    public static Vector4 TextColor => GetDefaultColor(ImGuiCol.Text);
    public static Vector4 TextColor50 => TextColor with { W = 0.5f };
    #endregion

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

    public static bool IconButtonThemed(FontAwesomeIcon icon, float size = default, string tooltip = "", bool circular = false, bool useColor = true)
    {
        using var color = useColor ?
            ImRaii.PushColor(ImGuiCol.Button, new Vector4(0.4f, 0.4f, 0.4f, 0.6f))
                  .Push(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.3f, 0.3f, 0.7f))
                  .Push(ImGuiCol.ButtonActive, new Vector4(0.2f, 0.2f, 0.2f, 0.8f))
                  .Push(ImGuiCol.Border, new Vector4(0.125f, 0.094f, 0.067f, 1f))
                  .Push(ImGuiCol.Text, Vector4.One)
            : null;

        var roundingCoefficient = circular ? 0.5f : 0.2f;
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 1.66f * ImGuiHelpers.GlobalScale)
                                .Push(ImGuiStyleVar.FrameRounding, roundingCoefficient * size);

        return IconButton(icon, new Vector2(size), tooltip); ;
    }

    // https://github.com/Haselnussbomber/HaselCommon/blob/main/HaselCommon/Gui/ImGuiUtils.cs#L36
    public static void HyperlinkTooltip(string url)
    {
        using var tooltip = ImRaii.Tooltip();

        ImGui.GetWindowDrawList().AddText(
            UiBuilder.IconFont, 12 * ImGuiHelpers.GlobalScale,
            ImGui.GetCursorScreenPos() + new Vector2(2 * ImGuiHelpers.GlobalScale),
            ImGui.ColorConvertFloat4ToU32(TextColor50),
            FontAwesomeIcon.ExternalLinkAlt.ToIconString()
        );

        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (20 * ImGuiHelpers.GlobalScale));

        ImGui.TextColored(TextColor50, url);
    }

    public static bool HyperlinkButton(string label, string url, Vector2 size = default, bool small = false)
    {
        var res = small ? ImGui.SmallButton(label) : ImGui.Button(label, size);

        if (ImGui.IsItemHovered())
        {
            HyperlinkTooltip(url);
        }

        if (res)
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }

        return res;
    }

    public static bool GameIcon(ushort icon, Vector2? size = null, Vector4? tint = null)
    {
        var texture = Service.TextureProvider.GetFromGameIcon(new(icon));
        if (texture.TryGetWrap(out var wrap, out _))
        {
            ImGui.Image(wrap.Handle, size ?? IconSize, Vector2.Zero, Vector2.One, tint ?? Vector4.One);
            return true;
        }
        return false;
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
