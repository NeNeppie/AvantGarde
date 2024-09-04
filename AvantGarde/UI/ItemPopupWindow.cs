using System.Diagnostics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace AvantGarde.UI;

public static class ItemPopupWindow
{
    public static unsafe void Draw(Item item)
    {
        if (item == null)
            return;

        if (!ImGui.BeginPopup($"##avantgarde-item-popup-{item.RowId}"))
            return;

        ImGui.TextUnformatted(item.Name);
        ImGui.Separator();

        ImGui.Text($"Equippable by: {item.ClassJobCategory.Value?.Name}");
        ImGui.TextUnformatted("");

        if (ImGui.Selectable("Try On"))
            AgentTryon.TryOn(0, item.RowId);

        if (ImGui.Selectable("Search Item"))
            ItemFinderModule.Instance()->SearchForItem(item.RowId, true);

        if (ImGui.Selectable("Link"))
            LinkItem(item);

        if (ImGui.Selectable("Copy Name"))
            ImGui.SetClipboardText(item.Name.RawString);

        if (ImGui.Selectable("Open in Garland Tools"))
            Process.Start(new ProcessStartInfo { FileName = $"https://garlandtools.org/db/#item/{item.RowId}", UseShellExecute = true });
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip($"https://garlandtools.org/db/#item/{item.RowId}");

        // TODO: Possible item {EQUIPABLE GEAR} sources:
        //      * Crafting
        //      * Exchange (Special Shop - non-gil-currencies, raids tokens, tomestones, etc.)
        //      * Gil Shops (Gridania/Uldah/Limsa etc.)
        //      * Marketboard (e.g. old Starlight Celebration set)
        //      * Desynth (e.g. rare fishing drops)
        //      * Instance (Dungeon, Raid Coffers, AR, etc.) Drops
        //      * Achievement Claim
        //      * Retainer Ventures (Rare ARR gear exclusive to Ventures)
        //      * Eureka & Bozja Lockboxes

        ImGui.EndPopup();
    }

    private static unsafe void LinkItem(Item item)
    {
        var agentChatLog = AgentChatLog.Instance();

        agentChatLog->LinkedItem.ItemId = item.RowId;
        agentChatLog->LinkedItem.Quantity = 1;
        agentChatLog->LinkedItemName.SetString(item.Name.RawString);
        agentChatLog->LinkedItemQuality = item.Rarity;

        // 1096 is the ID for <item>
        agentChatLog->InsertTextCommandParam(1096, true);
    }
}
