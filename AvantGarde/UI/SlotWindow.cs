using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Textures;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

using AvantGarde.Data;
using AvantGarde.Utils;

namespace AvantGarde.UI;

public class SlotWindow
{
    private static ImGuiWindowFlags WindowFlags => ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;

    private readonly List<Item> Items;
    private List<Item> ItemsFiltered;
    private ItemSlot Slot;

    private Vector2 Position = new();
    private bool IsOpen = false;

    public SlotWindow()
    {
        // Get all equipable items relevant for Fashion Report
        this.Items = Service.DalamudDataManager.GetExcelSheet<Item>()!
            .Where(item => item.EquipSlotCategory.Row != 0 && item.EquipSlotCategory.Value!.SoulCrystal == 0
                                                           && item.EquipSlotCategory.Value!.MainHand == 0
                                                           && item.EquipSlotCategory.Value!.OffHand == 0).ToList();
        Service.PluginLog.Debug($"Number of items loaded: {this.Items.Count}");
        this.ItemsFiltered = this.Items;
    }

    public void Update(ItemSlot slot, List<int>? itemIDs, Vector2 windowPos, float buttonSize)
    {
        this.IsOpen = !this.IsOpen;

        this.ItemsFiltered = new();
        if (this.IsOpen)
        {
            this.Slot = slot;
            this.Position = windowPos;
            this.Position.X += slot >= ItemSlot.Ears ? buttonSize : -GuiUtilities.SlotWindowSize.X;

            if (itemIDs is not null)
            {
                this.ItemsFiltered = this.Items
                    .Where(item => slot.IsMatchingSlot(item) && itemIDs.Contains((int)item.RowId) == true).ToList();
            }
        }
    }

    public unsafe void Draw()
    {
        if (!this.IsOpen) { return; }

        ImGui.SetNextWindowSize(GuiUtilities.SlotWindowSize);
        ImGui.SetNextWindowPos(this.Position);
        if (!ImGui.Begin($"##avantgarde-item-display-{this.Slot}", WindowFlags))
        {
            ImGui.End();
            return;
        }

        ImGui.Text($"Avant-Garde: {this.Slot.GetDescription()}");
        ImGui.Separator();

        if (!this.ItemsFiltered.Any())
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 1f));
            ImGui.TextWrapped("This category could be new, and/or is currently empty in the database.");
            ImGui.Spacing();
            ImGui.TextWrapped("If you wish to help expand the database, see the github page for more information.");
            ImGui.PopStyleColor();
            return;
        }

        var clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
        clipper.Begin(this.ItemsFiltered.Count);
        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var item = this.ItemsFiltered[i];
                this.DrawItem(item);
            }
        }
        clipper.End();

        ImGui.End();
    }

    private void DrawItem(Item item)
    {
        if (Service.TextureProvider.GetFromGameIcon(new GameIconLookup { IconId = item.Icon }).TryGetWrap(out var icon, out _))
        {
            if (icon is not null)
            {
                ImGui.Image(icon.ImGuiHandle, GuiUtilities.IconSize);
                ImGui.SameLine();
            }
        }

        ImGui.TextWrapped($"{item.Name}");
    }
}
