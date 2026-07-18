using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Lumina.Excel.Sheets;

using AvantGarde.Data;
using AvantGarde.Utils;

namespace AvantGarde.UI;

public class SlotWindow
{
    private static ImGuiWindowFlags WindowFlags => ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;

    private List<Item> _itemsFiltered;
    private Dictionary<uint, uint> _itemCounts = [];
    private ItemSlot _slot;
    private Vector2 _position = new();
    private bool _isOpen = false;

    public SlotWindow()
    {
        _itemsFiltered = Service.DataManager.Items;
    }

    public void Update(ItemSlot slot, List<(uint Id, uint Count)>? items, Vector2 windowPos, float buttonSize)
    {
        if (slot == _slot && _isOpen)
            _isOpen = false;
        else
            _isOpen = true;

        _itemsFiltered = [];
        if (_isOpen)
        {
            _slot = slot;
            _position = windowPos;
            _position.X += slot >= ItemSlot.Ears ? buttonSize : -GuiUtilities.SlotWindowSize.X;

            if (items is not null)
            {
                var itemIds = items.Select(item => item.Id).ToList();
                _itemCounts = items.ToDictionary();
                _itemsFiltered = Service.DataManager.Items
                    .Where(item => slot.IsMatchingSlot(item) && itemIds.Contains(item.RowId) == true).ToList();
            }
        }
    }

    public void Draw()
    {
        if (!_isOpen) { return; }

        ImGui.SetNextWindowSize(GuiUtilities.SlotWindowSize);
        ImGui.SetNextWindowPos(_position);

        if (!ImGui.Begin($"##avantgarde-item-display-{_slot}", WindowFlags))
        {
            ImGui.End();
            return;
        }

        ImGui.Text($"Avant-Garde: {_slot.GetDescription()}");
        ImGui.Separator();

        if (!_itemsFiltered.Any())
        {
            using (ImRaii.PushColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 1f)))
            {
                ImGui.TextWrapped("""
                This category is currently empty in the database.
                New data becomes available on a daily basis. Please check back later!
                """);
                ImGui.Spacing();
                if (Service.PluginConfig.DataCollectionOptedIn)
                {
                    ImGui.TextWrapped("Alternatively, in the meantime, go and discover new options! Each submission helps expand the database.");
                }
                else
                {
                    ImGui.TextWrapped("""
                    Alternatively, in the meantime, you may help crowdsourcing by opting-in to data collection.
                    No personal or sensitive information is ever collected.
                    """);
                }
            }

            ImGui.End();
            return;
        }

        ImGuiClip.ClippedDraw(_itemsFiltered, item => DrawItem(item, showIDs: false, canInteract: true, count: _itemCounts[item.RowId]), GuiUtilities.IconSize.Y + ImGui.GetStyle().ItemSpacing.Y);

        ImGui.End();
    }

    public static void DrawItem(Item item, bool showIDs, bool canInteract, uint count = 0)
    {
        if (canInteract)
        {
            if (ImGui.Selectable($"##avantgarde-popup-select-{item.RowId}", false, ImGuiSelectableFlags.None, new Vector2(GuiUtilities.SlotWindowSize.X, GuiUtilities.IconSize.Y))
                && (ImGui.IsMouseReleased(ImGuiMouseButton.Left) || ImGui.IsMouseReleased(ImGuiMouseButton.Right)))
            {
                ImGui.OpenPopup($"##avantgarde-item-popup-{item.RowId}");
            }
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - GuiUtilities.IconSize.Y - ImGui.GetStyle().FramePadding.Y);
        }

        if (Service.TextureProvider.GetFromGameIcon(new GameIconLookup { IconId = item.Icon }).TryGetWrap(out var icon, out _))
        {
            if (icon is not null)
            {
                ImGui.Image(icon.Handle, GuiUtilities.IconSize);
                ImGui.SameLine();
            }
        }

        var itemName = item.Name.ExtractText();
        if (showIDs)
        {
            itemName = $"[{item.RowId}] " + itemName;
        }
        ImGui.TextWrapped(itemName);

        ItemPopupWindow.Draw(item, count);
    }
}
