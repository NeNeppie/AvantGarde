using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Lumina.Excel.Sheets;

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
                    .Where(item => slot.IsMatchingSlot(item) && itemIds.Contains(item.RowId)).ToList();
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

        ImGuiClip.ClippedDraw(_itemsFiltered, item => DrawItem(item, showIDs: false, canInteract: true, count: _itemCounts[item.RowId]), GuiUtilities.ClipperLineHeight);

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

public class DyeSlotWindow
{
    private static ImGuiWindowFlags WindowFlags => ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;

    private List<StainEx> _dyes = [];
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
            _position.X += slot >= ItemSlot.Ears ? buttonSize : -GuiUtilities.SlotWindowSize.X;

            // TODO: Attach icon image to each dye
            if (dyes is not null)
                _dyes = dyes.Select(dye => new StainEx((int)dye.Id, dye.Count, dye.Pct)).ToList();

            _totalRecords = _dyes.Sum(dye => (long)dye.Count);
        }
    }

    public void Draw()
    {
        if (!_isOpen) { return; }

        ImGui.SetNextWindowSize(GuiUtilities.SlotWindowSize);
        ImGui.SetNextWindowPos(_position);

        if (!ImGui.Begin($"##avantgarde-dye-display-{_slot}", WindowFlags))
        {
            ImGui.End();
            return;
        }

        ImGui.Text($"Avant-Garde: {_slot.GetDescription()}");
        ImGui.Separator();

        // TODO: Add lack-of-data message
        if (_dyes.Any())
        {
            ImGuiClip.ClippedDraw(_dyes, dye => DrawDye(dye), GuiUtilities.ClipperLineHeight);
        }

        ImGui.End();
    }

    private void DrawDye(StainEx dye)
    {
        if (Service.TextureProvider.GetFromGameIcon(new GameIconLookup { IconId = dye.Icon }).TryGetWrap(out var icon, out _))
        {
            if (icon is not null)
            {
                ImGui.Image(icon.Handle, GuiUtilities.IconSize);
                ImGui.SameLine();
            }
        }

        var itemName = dye.Stain.Name.ExtractText();
        itemName = $"[{dye.Stain.RowId}] " + itemName;
        ImGui.TextWrapped($"{itemName} ({dye.ScoringShade})\nConfidence: {dye.Confidence * 100:F1}% ({dye.Count} of {_totalRecords})");
    }
}

// TEMP: No reason to not do this on DataManager initialization.
internal class StainEx
{
    public Stain Stain;
    public ulong Count;
    public float Confidence;
    public ushort Icon;
    public string ScoringShade;

    private static readonly Dictionary<int, ushort> DyeIconMap = [];
    // Ids of old dye items: 5729-5813, 13114-13117, 13708-13723, 30116-30124, 48163-48172, 48227
    private static readonly List<int> DyeItemIds =
                Enumerable.Range(5729, 5813 - 5729 + 1)
        .Concat(Enumerable.Range(13114, 13117 - 13114 + 1))
        .Concat(Enumerable.Range(13708, 13723 - 13708 + 1))
        .Concat(Enumerable.Range(30116, 30124 - 30116 + 1))
        .Concat(Enumerable.Range(48163, 48172 - 48163 + 1))
        .Concat(Enumerable.Range(48227, 1)).ToList();

    static StainEx()
    {
        var sheet = Service.DalamudDataManager.GetExcelSheet<Item>();
        foreach (var id in DyeItemIds)
        {
            var item = sheet.GetRowAt(id);
            DyeIconMap.Add((int)item.AdditionalData.RowId, item.Icon);
        }
    }

    public StainEx(int id, ulong count, float pct)
    {
        Stain = Service.DalamudDataManager.GetExcelSheet<Stain>().GetRowAt(id);
        Count = count;
        Confidence = pct;
        Icon = DyeIconMap.GetValueOrDefault<int, ushort>(id, 27614); // Terebinth icon
        ScoringShade = MapIconToShade(Icon);
    }

    private static string MapIconToShade(ushort icon)
    {
        return icon switch
        {
            22811 or 22820 or 22817 => "White", // Metallic Silver is an exception and considered "White"
            22808 => "Grey",
            22807 or 22816 => "Black",
            22805 or 22814 => "Red",
            22809 or 22818 => "Orange",
            22806 or 22815 => "Yellow",
            22810 or 22819 => "Green",
            22804 or 22813 => "Blue",
            22812 or 22821 => "Purple",
            _ => "Unknown"
        };
    }
}
