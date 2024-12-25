using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Textures;
using ImGuiNET;
using Lumina.Excel.Sheets;

using AvantGarde.Data;
using AvantGarde.Utils;

namespace AvantGarde.UI;

public class SlotWindow
{
    private static ImGuiWindowFlags WindowFlags => ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;

    private List<Item> _itemsFiltered;
    private ItemSlot _slot;
    private Vector2 _position = new();
    private bool _isOpen = false;

    public SlotWindow()
    {
        _itemsFiltered = Service.DataManager.Items;
    }

    public void Update(ItemSlot slot, List<int>? itemIDs, Vector2 windowPos, float buttonSize)
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

            if (itemIDs is not null)
            {
                _itemsFiltered = Service.DataManager.Items
                    .Where(item => slot.IsMatchingSlot(item) && itemIDs.Contains((int)item.RowId) == true).ToList();
            }
        }
    }

    public unsafe void Draw()
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
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 1f));
            ImGui.TextWrapped("This category could be new, and/or is currently empty in the database.");
            ImGui.Spacing();
            ImGui.TextWrapped("If you wish to help, see the github page for more information.");
            ImGui.PopStyleColor();

            ImGui.End();
            return;
        }

        var clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
        clipper.Begin(_itemsFiltered.Count);
        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var item = _itemsFiltered[i];
                DrawItem(item, showIDs: false, canInteract: true);
            }
        }
        clipper.End();

        ImGui.End();
    }

    public static void DrawItem(Item item, bool showIDs, bool canInteract)
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
                ImGui.Image(icon.ImGuiHandle, GuiUtilities.IconSize);
                ImGui.SameLine();
            }
        }

        var itemName = item.Name.ExtractText();
        if (showIDs)
        {
            itemName = $"[{item.RowId}] " + itemName;
        }
        ImGui.TextWrapped(itemName);

        ItemPopupWindow.Draw(item);
    }
}
