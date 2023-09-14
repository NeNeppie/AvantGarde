using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Logging;
using ImGuiNET;
using ImGuiScene;
using Lumina.Excel.GeneratedSheets;

namespace FashionReporter.UI;

public class SlotWindow
{
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

    public void Update(ItemSlot slot, string slotCategory, List<Category>? data, Vector2 windowPos)
    {
        this.IsOpen = !this.IsOpen;

        this.ItemsFiltered = new();
        if (this.IsOpen)
        {
            this.Slot = slot;
            this.Position = windowPos;
            this.Position.X += (int)slot >= 5 ? 60 : -300;

            var category = data?.Find(x => x.Name == slotCategory!);
            if (category is not null)
            {
                this.ItemsFiltered = this.Items
                    .Where(item => MainWindow.IsMatchingSlot(item, slot) && category?.IDs.Contains((int)item.RowId) == true).ToList();
            }
        }
    }

    public unsafe void Draw()
    {
        if (!this.IsOpen) { return; }

        var windowSize = new Vector2(300, 200);
        ImGui.SetNextWindowSize(windowSize);
        ImGui.SetNextWindowPos(this.Position);
        if (!ImGui.Begin($"##fashionreporter-item-display-{this.Slot}", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove))
        {
            ImGui.End();
            return;
        }

        ImGui.Text($"Fashion Reporter - {this.Slot}");
        ImGui.Separator();

        if (!this.ItemsFiltered.Any())
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 1f));
            ImGui.TextWrapped("Text to be replaced with an explanation or something of the sort");
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
        TextureWrap? icon = null;
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
            ImGui.Image(icon.ImGuiHandle, new Vector2(48));  // Icon size. TODO: Global scaling
            ImGui.SameLine();
        }

#if DEBUG
        ImGui.Text($"[{item.RowId}]");
        ImGui.SameLine();
#endif
        ImGui.TextWrapped($"{item.Name}");
    }
}
