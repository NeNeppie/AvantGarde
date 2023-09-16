using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using Dalamud.Interface;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

using FashionReporter.Data;
using FashionReporter.Utils;

namespace FashionReporter.UI;

public class Category
{
    public string Name { get; init; } = "";
    public List<int> IDs { get; init; } = new();
}

public class MainWindow
{
    private static readonly SlotWindow SlotWindow = new();
    private static ImGuiWindowFlags WindowFlags => ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMouseInputs;

    private readonly List<Category>? Data;

    public MainWindow()
    {
        var filePath = Path.Combine(Service.PluginInterface.AssemblyLocation.Directory?.FullName!, "Data\\data.json");
        if (!File.Exists(filePath))
            throw new Exception("Unable to load data file.");

        var jsonString = File.ReadAllText(filePath);
        this.Data = JsonSerializer.Deserialize<List<Category>>(jsonString);
    }

    public unsafe void Draw(AtkUnitBase* addon)
    {
        if (addon->RootNode is null) { return; }

        ImGui.SetNextWindowSize(new Vector2(addon->RootNode->Width, addon->RootNode->Height) * addon->Scale);
        ImGui.SetNextWindowPos(new Vector2(addon->X, addon->Y));
        ImGuiHelpers.ForceNextWindowMainViewport();
        if (!ImGui.Begin("Fashion Reporter", WindowFlags))
        {
            ImGui.End();
            return;
        }

        foreach (var slot in Enum.GetValues<ItemSlot>())
        {
            var slotCategory = "";
            var slotNodeID = 9 + (uint)slot;
            var atkValueIndex = 13 + ((int)slot * 11);

            var slotNode = addon->GetNodeById(slotNodeID);

            var buttonSize = slotNode->Height * addon->Scale * 0.8f;
            var buttonPos = this.GetButtonPosition(addon, slotNode, slot);

            slotCategory = MemoryHelper.ReadSeStringNullTerminated(new nint(addon->AtkValues[atkValueIndex].String)).TextValue;
            if (slotCategory == "") { continue; }

            ImGui.SetCursorPos(buttonPos);
            ImGui.BeginChild($"##child-{slot}", new Vector2(buttonSize * 1.15f));
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.4f, 0.4f, 0.4f, 0.6f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.3f, 0.3f, 0.7f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.2f, 0.2f, 0.2f, 0.8f));
                ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.125f, 0.094f, 0.067f, 1f));

                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2f);
                ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, buttonSize * 0.5f);

                ImGui.SetCursorPos(ImGui.GetStyle().FramePadding);
                if (GuiUtilities.IconButton(FontAwesomeIcon.List, new Vector2(buttonSize), "Show Gear"))
                {
                    var category = this.Data?.Find(x => x.Name == slotCategory!);
                    SlotWindow.Update(slot, category, ImGui.GetWindowPos() + ImGui.GetStyle().FramePadding, buttonSize);
                }

                ImGui.PopStyleVar(2);
                ImGui.PopStyleColor(4);
            }
            ImGui.EndChild();
        }
        SlotWindow.Draw();

        ImGui.End();
    }

    private unsafe Vector2 GetButtonPosition(AtkUnitBase* addon, AtkResNode* node, ItemSlot slot)
    {
        // Child nodes are all relative to their parent/addon, hence the seemingly random numbers ((246, 30) + (10, 48))
        var position = (new Vector2(256f + node->X, 78f + node->Y)
                        + new Vector2((node->Height * 0.1f) + 0.5f)) * addon->Scale;

        if (slot >= ItemSlot.Ears)
            // Width of the underlying NineGrid node
            position.X += 198f * addon->Scale;

        return position;
    }
}
