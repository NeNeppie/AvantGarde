using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using ImGuiScene;
using Lumina.Excel.GeneratedSheets;

namespace FashionReporter.Windows;

// TODO: Move this out to a different file
public enum ItemSlot
{
    Head,
    Hands,
    Body,
    Legs,
    Feet,
    Ears,
    Neck,
    Wrists,
    RightRing,
    LeftRing
}

public class Category
{
    public string Name { get; init; } = "";
    public List<int> IDs { get; init; } = new();
}

public class MainWindow : Window
{
    private readonly ItemSlot Slot;
    private readonly List<Category>? Data;

    private readonly uint SlotNodeID;
    private readonly int AtkValueIndex;

    private List<Item> Items;
    private Dictionary<ushort, TextureWrap> Icons = new();

    public MainWindow(ItemSlot dedicatedSlot) : base("Fashion Reporter", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove)
    {
        this.IsOpen = true;
        this.Size = new Vector2(200, 50);
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 50),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        this.SizeCondition = ImGuiCond.FirstUseEver;

        this.Slot = dedicatedSlot;
        this.WindowName = $"##{dedicatedSlot}";
        // Addon shenanigans
        this.SlotNodeID = 9 + (uint)dedicatedSlot;
        this.AtkValueIndex = 13 + ((int)dedicatedSlot * 11);

        // TODO: Reformat later
        var filePath = Path.Combine(Service.PluginInterface.AssemblyLocation.Directory?.FullName!, "data.json");
        if (File.Exists(filePath))
        {
            var jsonString = File.ReadAllText(filePath);
            this.Data = JsonSerializer.Deserialize<List<Category>>(jsonString);
            if (this.Data is null)
            {
                PluginLog.Error("Data is null!");
            }
            else
            {
                foreach (var cat in this.Data)
                {
                    PluginLog.Debug($"{cat.Name} - {string.Join(',', cat.IDs)}");
                }
            }
        }
        else
        {
            PluginLog.Error($"Couldn't find file {filePath}");
        }

        this.Items = Service.DataManager.GetExcelSheet<Item>()!
            .Where(item => item.EquipSlotCategory.Row != 0 && IsMatchingSlot(item, this.Slot)).ToList();
        PluginLog.Debug($"Number of items loaded: {this.Items.Count}");
    }

    public override unsafe void Draw()
    {
        var addon = (AtkUnitBase*)Service.GameGui.GetAddonByName("FashionCheck");
        if (addon is null || !addon->IsVisible) { return; }

        var innerNode = addon->GetNodeById(6);
        var slotCategory = "";
        if (innerNode is not null)
        {
            var childNode = innerNode->ChildNode->PrevSiblingNode;
            var slotNode = addon->GetNodeById(this.SlotNodeID);
            this.Position = this.GetNodeTruePos(addon, childNode) +
                new Vector2(slotNode->X * 1.8f * addon->Scale, slotNode->Y * addon->Scale);

            var slotNodeText = slotNode->GetAsAtkTextNode();
            if (slotCategory == "")
            {
                slotCategory = MemoryHelper.ReadSeStringNullTerminated(new nint(addon->AtkValues[this.AtkValueIndex].String)).TextValue;
                if (slotCategory == "") { return; }
                var cat = this.Data?.Find(x => x.Name == slotCategory!);
                if (cat is not null)
                {
                    this.Items = this.Items.Where(item => cat?.IDs.Contains((int)item.RowId) == true).ToList();
                }
            }
        }

        if (ImGui.CollapsingHeader($"{this.Slot} - {slotCategory}"))
        {
            ImGui.Text($"{this.Position}");

            var clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
            clipper.Begin(this.Items.Count);
            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var item = this.Items[i];
                    this.DrawItem(item);
                }
            }
        }
    }

    private unsafe Vector2 GetNodeTruePos(AtkUnitBase* addon, AtkResNode* node)
    {
        var position = new Vector2();
        while (node != (AtkResNode*)addon)
        {
            position += new Vector2(node->X * addon->Scale, node->Y * addon->Scale);
            node = node->ParentNode;
        }
        return position += new Vector2(addon->X, addon->Y);
    }

    // TODO: Move this out to a different file
    private static bool IsMatchingSlot(Item item, ItemSlot slot)
    {
        return slot switch
        {
            ItemSlot.Head => item.EquipSlotCategory.Value!.Head > 0,
            ItemSlot.Hands => item.EquipSlotCategory.Value!.Gloves > 0,
            ItemSlot.Body => item.EquipSlotCategory.Value!.Body > 0,
            ItemSlot.Legs => item.EquipSlotCategory.Value!.Legs > 0,
            ItemSlot.Feet => item.EquipSlotCategory.Value!.Feet > 0,
            ItemSlot.Ears => item.EquipSlotCategory.Value!.Ears > 0,
            ItemSlot.Neck => item.EquipSlotCategory.Value!.Neck > 0,
            ItemSlot.Wrists => item.EquipSlotCategory.Value!.Wrists > 0,
            ItemSlot.RightRing => item.EquipSlotCategory.Value!.FingerR > 0,
            ItemSlot.LeftRing => item.EquipSlotCategory.Value!.FingerL > 0,
            _ => false
        };
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
            ImGui.Image(icon.ImGuiHandle, new Vector2(48));  // Icon size
            ImGui.SameLine();
        }

#if DEBUG
        ImGui.Text($"[{item.RowId}]");
        ImGui.SameLine();
#endif
        ImGui.TextWrapped($"{item.Name}");
    }
}
