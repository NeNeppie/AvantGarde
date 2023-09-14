using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using Dalamud.Interface;
using Dalamud.Logging;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

using FashionReporter.Utils;

namespace FashionReporter.UI;

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

public class MainWindow
{
    private readonly List<Category>? Data;
    private ImGuiWindowFlags WindowFlags => ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMouseInputs;

    private static readonly SlotWindow SlotWindow = new();

    public MainWindow()
    {
        // TODO: Reformat later?
        var filePath = Path.Combine(Service.PluginInterface.AssemblyLocation.Directory?.FullName!, "data.json");
        if (File.Exists(filePath))
        {
            var jsonString = File.ReadAllText(filePath);
            this.Data = JsonSerializer.Deserialize<List<Category>>(jsonString);
            if (this.Data is not null)
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
    }

    public unsafe void Draw(AtkUnitBase* addon)
    {
        var innerNode = addon->GetNodeById(6);
        if (innerNode is null || addon->RootNode is null) { return; }

        ImGuiHelpers.ForceNextWindowMainViewport();
        if (!ImGui.Begin("Fashion Reporter", this.WindowFlags))
        {
            ImGui.End();
            return;
        }

        ImGui.SetWindowSize(new Vector2(addon->RootNode->Width * addon->Scale, addon->RootNode->Height * addon->Scale));
        ImGui.SetWindowPos(new Vector2(addon->X, addon->Y));

        foreach (var slot in Enum.GetValues<ItemSlot>())
        {
            var slotCategory = "";
            var slotNodeID = 9 + (uint)slot;
            var atkValueIndex = 13 + ((int)slot * 11);

            var childNode = innerNode->ChildNode->PrevSiblingNode;
            var slotNode = addon->GetNodeById(slotNodeID);
            var position = Utilities.GetNodePosAbsolute(addon, childNode)
                            + Utilities.GetNodePosAbsolute(addon, innerNode)
                            + new Vector2(slotNode->X * addon->Scale, slotNode->Y * addon->Scale);
            if ((int)slot >= 5)
                position += new Vector2((slotNode->Width * addon->Scale) - 60, 0);
            ImGui.SetCursorPos(position);

            var slotNodeText = slotNode->GetAsAtkTextNode();
            if (slotCategory == "")
            {
                slotCategory = MemoryHelper.ReadSeStringNullTerminated(new nint(addon->AtkValues[atkValueIndex].String)).TextValue;
                if (slotCategory == "") { continue; }
            }

            // TODO: Proper sizing, global scale yada yada you know the deal already (applies to all elements so far)
            ImGui.BeginChild($"##child-{slot}", new Vector2(60));
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.6f, 0.6f, 0.6f, 0.4f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.4f, 0.4f, 0.6f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.2f, 0.2f, 0.2f, 0.8f));

                ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.125f, 0.094f, 0.067f, 1f));

                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 3f);
                ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 15f);

                if (Utilities.IconButton(FontAwesomeIcon.List, new Vector2(60), "Show Gear"))
                {
                    SlotWindow.Update(slot, slotCategory, this.Data, ImGui.GetWindowPos());
                }

                ImGui.PopStyleVar(2);
                ImGui.PopStyleColor(4);
            }
            ImGui.EndChild();
        }
        SlotWindow.Draw();

        ImGui.End();
    }

    // TODO: Move this out to a different file
    public static bool IsMatchingSlot(Item item, ItemSlot slot)
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
}
