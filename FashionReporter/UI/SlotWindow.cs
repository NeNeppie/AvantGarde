using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Logging;
using ImGuiNET;
using ImGuiScene;
using Lumina.Excel.GeneratedSheets;

using FashionReporter.Data;
using FashionReporter.Utils;

namespace FashionReporter.UI;

public class SlotWindow
{
    private static ImGuiWindowFlags WindowFlags => ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;

    private readonly List<Item> Items;
    private List<Item> ItemsFiltered;
    private ItemSlot Slot;

    private Dictionary<ushort, TextureWrap> Icons = new();
    private Vector2 Position = new();
    private bool IsOpen = false;

    public SlotWindow()
    {
        // Get all equipable items relevant for Fashion Report
        this.Items = Service.DataManager.GetExcelSheet<Item>()!
            .Where(item => item.EquipSlotCategory.Row != 0 && item.EquipSlotCategory.Value!.SoulCrystal == 0
                                                           && item.EquipSlotCategory.Value!.MainHand == 0
                                                           && item.EquipSlotCategory.Value!.OffHand == 0).ToList();
        PluginLog.Debug($"Number of items loaded: {this.Items.Count}");
        this.ItemsFiltered = this.Items;
    }

    public void Update(ItemSlot slot, Category? category, Vector2 windowPos, float buttonSize)
    {
        this.IsOpen = !this.IsOpen;

        this.ItemsFiltered = new();
        if (this.IsOpen)
        {
            this.Slot = slot;
            this.Position = windowPos;
            this.Position.X += slot >= ItemSlot.Ears ? buttonSize : -GuiUtilities.SlotWindowSize.X;

            if (category is not null)
            {
                this.ItemsFiltered = this.Items
                    .Where(item => slot.IsMatchingSlot(item) && category?.IDs.Contains((int)item.RowId) == true).ToList();
            }
        }
    }

    public unsafe void Draw()
    {
        if (!this.IsOpen) { return; }

        ImGui.SetNextWindowSize(GuiUtilities.SlotWindowSize);
        ImGui.SetNextWindowPos(this.Position);
        if (!ImGui.Begin($"##fashionreporter-item-display-{this.Slot}", WindowFlags))
        {
            ImGui.End();
            return;
        }

        ImGui.Text($"Fashion Reporter - {this.Slot.GetDescription()}");
        ImGui.Separator();

        if (!this.ItemsFiltered.Any())
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 1f));
            ImGui.TextWrapped("Text to be replaced with an explanation or something of the sort"); // TODO:
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
        TextureWrap? icon;
        if (this.Icons.TryGetValue(item.Icon, out var texture))
        {
            icon = texture;
        }
        else
        {
            icon = Service.TextureProvider.GetIcon(item.Icon);
            this.Icons[item.Icon] = icon!;
        }

        if (icon is not null)
        {
            ImGui.Image(icon.ImGuiHandle, GuiUtilities.IconSize);
            ImGui.SameLine();
        }

        ImGui.TextWrapped($"{item.Name}");
    }
}
