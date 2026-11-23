using System.Diagnostics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.Sheets;

using AvantGarde.Managers;
using AvantGarde.Utilities;

namespace AvantGarde.UI;

public static class ItemPopupWindow
{
    public static unsafe void Draw(Item item, uint timesUsed)
    {
        using var popup = ImRaii.Popup($"##avantgarde-item-popup-{item.RowId}");
        if (!popup) return;

        var drawSeparatorIpc = true;

        ImGui.TextUnformatted(item.Name.ExtractText());
        ImGui.Separator();

        ImGui.Text($"Equippable by: {item.ClassJobCategory.Value.Name}");
        ImGui.Spacing();

        ImGui.Text($"Times used: {timesUsed}");
        ImGui.Spacing();

        DrawItemSource(item.RowId);
        ImGui.Spacing();

        if (ImGui.Selectable("Try On"))
            AgentTryon.TryOn(0, item.RowId);

        if (ImGui.Selectable("Search Item"))
            ItemFinderModule.Instance()->SearchForItem(item.RowId, true);

        if (ImGui.Selectable("Link"))
            LinkItem(item);

        if (ImGui.Selectable("Copy Name"))
            ImGui.SetClipboardText(item.Name.ExtractText());

        if (ImGui.Selectable("Open in Garland Tools"))
            Process.Start(new ProcessStartInfo { FileName = $"https://garlandtools.org/db/#item/{item.RowId}", UseShellExecute = true });
        if (ImGui.IsItemHovered())
            ImGuiUtils.HyperlinkTooltip($"https://garlandtools.org/db/#item/{item.RowId}");

        if (Service.AllaganToolsIpc.IsAvailable)
        {
            DrawIPCSeparator(ref drawSeparatorIpc);
            
            if (ImGui.Selectable("Open in Allagan Tools"))   
                Service.AllaganToolsIpc.OpenMoreInformation(item.RowId);
        }

        if (Service.ItemVendorLocIpc.IsAvailable)
        {
            DrawIPCSeparator(ref drawSeparatorIpc);

            if (ImGui.Selectable("Open in Item Vendor Location"))
                Service.ItemVendorLocIpc.OpenVendorResults(item.RowId);
        }
    }

    private static void DrawItemSource(uint itemId)
    {
        var sources = Service.SourceManager.GetItemSources(itemId);
        if (sources.Count == 0)
        {
            ImGuiUtils.GameIcon(SourceIcons.SourceTypeIconQuestionMark);
            ImGui.SameLine();
            ImGui.Text("Unknown Source!\nWork In Progress...");
            return;   
        }

        foreach (var source in sources)
        {
            ImGuiUtils.GameIcon(source.DisplayIcon);
            ImGui.SameLine();
            ImGui.Text(source.AsText());
        }
    }

    private static void DrawIPCSeparator(ref bool shouldDraw)
    {
        if (shouldDraw)
        {
            ImGui.Separator();
            shouldDraw = false;
        }
    }

    private static unsafe void LinkItem(Item item)
    {
        var agentChatLog = AgentChatLog.Instance();

        agentChatLog->LinkedItem.ItemId = item.RowId;
        agentChatLog->LinkedItem.Quantity = 1;
        agentChatLog->LinkedItemName.SetString(item.Name.ExtractText());
        agentChatLog->LinkedItem.LinkedItemQuality = item.Rarity;

        // 1096 is the ID for <item>
        agentChatLog->InsertTextCommandParam(1096, true);
    }
}
