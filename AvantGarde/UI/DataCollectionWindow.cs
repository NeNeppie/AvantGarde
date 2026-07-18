using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

using AvantGarde.Utils;

namespace AvantGarde.UI;

public class DataCollectionWindow
{
    private static readonly ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;
    private bool _shouldDraw = true;
    private bool _doNotShowAgain = false;

    public void Draw()
    {
        if (!_shouldDraw) return;

        var center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        if (!ImGui.Begin("IMPORTANT !##avantgarde-datacollection", WindowFlags))
        {
            ImGui.End();
            return;
        }

        using (var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(ImGui.GetStyle().ItemSpacing.X, 10f)))
        {

            ImGui.TextColored(new Vector4(0.2f, 0.52f, 0.83f, 1f), "Thank you for using Avant-Garde!");
            ImGui.Separator();

            ImGui.TextWrapped("""
            You're now testing the new automatic crowdsourcing feature.
            As the database is being migrated, you might find that some categories are empty or lacking items.
            If you wish to see the full list of items, switch back to the stable version.
            If you encounter any issues, please report them on Dalamud's Discord, in either the plugin testing channel or Avant-Garde's forum post.
            """);
            ImGui.Separator();
        }

        ImGui.TextWrapped("""
        The plugin aims to provide you with the most comprehensive list of available items for you to use each week.
        To do that, it collects anonymized data. If you choose to opt-in, the plugin will collect the following info:
        """);

        string[] collectedInfo = ["The current week number and its theme", "How well you've scored", "Items & Dyes used"];
        foreach (var point in collectedInfo) // I'm being a little silly here
            ImGui.BulletText(point);
        ImGui.Spacing();

        ImGui.Checkbox("Opt-in to data collection", ref Service.PluginConfig.DataCollectionOptedIn);
        ImGui.Spacing();

        ImGui.Text("You can always change your mind by ticking the box inside the Fashion Report window.\nFunctionality is not affected by your choice.");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Checkbox("Don't show me this again", ref _doNotShowAgain);
        ImGui.Spacing();

        if (ImGui.Button("Okok!"))
        {
            Service.PluginConfig.SeenDataCollectionMessage = _doNotShowAgain;
            Service.PluginConfig.Save();
            _shouldDraw = false;
        }
        ImGui.SameLine();

        using (var color = ImRaii.PushColor(ImGuiCol.Button, new Vector4(0.345f, 0.396f, 0.949f, 1.0f)))
        {
            GuiUtilities.HyperlinkButton("Discord Forum Post", "https://discord.com/channels/581875019861328007/1166794253553381456");
        }

        ImGui.End();
    }
}
