using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Lumina.Excel.Sheets;

using AvantGarde.Managers;
using AvantGarde.Utilities;

namespace AvantGarde.UI;

public class ItemSlotWindow
{
    private static ImGuiWindowFlags WindowFlags => ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;

    private List<Item> _itemsFiltered;
    private Dictionary<uint, uint> _itemCounts = [];
    private Dictionary<uint, bool> _itemOwnership = [];
    private ItemSlot _slot;
    private Vector2 _position = new();
    private bool _isOpen = false;

    public ItemSlotWindow()
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
            _position.X += slot >= ItemSlot.Ears ? -ImGuiUtils.SlotWindowSize.X : buttonSize;

            if (items is not null)
            {
                var itemIds = items.Select(item => item.Id).ToList();
                _itemCounts = items.ToDictionary();
                _itemsFiltered = Service.DataManager.Items
                    .Where(item => slot.IsMatchingSlot(item) && itemIds.Contains(item.RowId)).ToList();
            }

            // TODO: Not rely on IPC for inventory searching
            _itemsFiltered.ForEach(item => _itemOwnership[item.RowId] = Service.AllaganToolsIpc.FindOwnedItem(item.RowId));
        }
    }

    public void Draw()
    {
        if (!_isOpen) { return; }

        ImGui.SetNextWindowSize(ImGuiUtils.SlotWindowSize);
        ImGui.SetNextWindowPos(_position);

        if (!ImGui.Begin($"##avantgarde-item-display-{_slot}", WindowFlags))
        {
            ImGui.End();
            return;
        }

        ImGui.Text($"Avant-Garde: {_slot.GetDescription()}");
        ImGui.Separator();

        if (_itemsFiltered.Count == 0)
        {
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiUtils.TextColor50))
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

        ImGuiClip.ClippedDraw(
            _itemsFiltered,
            item => DrawItem(
                item,
                useCount: _itemCounts[item.RowId],
                dimmed: Service.PluginConfig.HighlightOwned && !_itemOwnership[item.RowId]),
            ImGuiUtils.ClipperLineHeight);

        ImGui.End();
    }

    public static void DrawItem(Item item, uint useCount = 0, bool selectable = true, bool showIds = false, bool dimmed = false)
    {
        var itemName = item.Name.ExtractText();
        var selectableSize = new Vector2(ImGuiUtils.SlotWindowSize.X, ImGuiUtils.IconSize.Y);
        var tint = dimmed ? Vector4.One with {W = 0.5f} : Vector4.One;

        var textColor = ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.Text));
        if (dimmed)
            textColor.W = 0.5f;

        if (showIds)
            itemName = $"[{item.RowId}] " + itemName;

        if (selectable)
        {
            if (ImGui.Selectable($"##avantgarde-popup-select-{item.RowId}", false, ImGuiSelectableFlags.None, selectableSize))
                ImGui.OpenPopup($"##avantgarde-item-popup-{item.RowId}");

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGuiUtils.IconSize.Y - ImGui.GetStyle().FramePadding.Y);
        }

        if (ImGuiUtils.GameIcon(item.Icon, tint: tint))
            ImGui.SameLine();

        ImGui.TextColoredWrapped(textColor, itemName);

        ItemPopupWindow.Draw(item, useCount);
    }
}

public class DyeSlotWindow
{
    private static ImGuiWindowFlags WindowFlags => ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;

    private List<DataManager.StainEx> _dyes = [];
    private ItemSlot _slot;
    private Vector2 _position = new();
    private bool _isOpen = false;
    private long _totalRecords = 0;

    public void Update(ItemSlot slot, List<(uint Id, ulong Count, float Pct)>? dyes, Vector2 windowPos, float buttonSize)
    {
        if (slot == _slot && _isOpen)
            _isOpen = false;
        else
            _isOpen = true;

        _dyes = [];
        if (_isOpen)
        {
            _slot = slot;
            _position = windowPos;
            _position.X -= ImGuiUtils.SlotWindowSize.X;

            if (dyes is not null)
                _dyes = dyes.Select(dye => Service.DataManager.StainExMap[dye.Id] with { Count = dye.Count, Confidence = dye.Pct }).ToList();

            _totalRecords = _dyes.Sum(dye => (long)dye.Count);
        }
    }

    public void Draw()
    {
        if (!_isOpen) { return; }

        ImGui.SetNextWindowSize(ImGuiUtils.SlotWindowSize);
        ImGui.SetNextWindowPos(_position);

        if (!ImGui.Begin($"##avantgarde-dye-display-{_slot}", WindowFlags))
        {
            ImGui.End();
            return;
        }

        ImGui.Text($"Avant-Garde: {_slot.GetDescription()}");
        ImGui.Separator();

        if (_dyes.Count == 0)
        {
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiUtils.TextColor50))
            {
                ImGui.TextWrapped("No dye data currently exists for this slot.");
                ImGui.Spacing();
                ImGui.TextWrapped("New data becomes available on a daily basis. Please check back later!");
            }

            ImGui.End();
            return;
        }

        ImGuiClip.ClippedDraw(_dyes, dye => DrawDye(dye), ImGuiUtils.ClipperLineHeight);

        ImGui.End();
    }

    private void DrawDye(DataManager.StainEx dye)
    {
        var color = ArgbToAbgr(dye.Stain.Color);

        var colorVec4 = ImGui.ColorConvertU32ToFloat4(color);
        ImGui.ColorEdit4("", ref colorVec4, ImGuiColorEditFlags.NoPicker | ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoAlpha);
        ImGui.SameLine();

        var itemName = dye.Stain.Name.ExtractText();
        ImGui.TextWrapped($"{itemName} ({dye.ScoringShade})\nConfidence: {dye.Confidence * 100:F1}% ({dye.Count} of {_totalRecords})");
    }

    private static uint ArgbToAbgr(uint value)
    {
        value <<= 8;

        // Endian flip
        return (BitOperations.RotateRight(value & 0x00FF00FFu, 8)
            + BitOperations.RotateLeft(value & 0xFF00FF00u, 8))
            | 0xFF000000u;
    }
}
