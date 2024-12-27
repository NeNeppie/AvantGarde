using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Lumina.Excel.Sheets;

using AvantGarde.Data;
using AvantGarde.Utils;

namespace AvantGarde.UI;

public unsafe class MainWindow
{
    private static ImGuiWindowFlags WindowFlags => ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMouseInputs;

    private readonly SlotWindow SlotWindow = new();

    public void Draw(AtkUnitBase* addon)
    {
        var windowPos = new Vector2(addon->X, addon->Y);
        var windowSize = new Vector2(addon->RootNode->Width, addon->RootNode->Height) * addon->Scale;
        ImGuiHelpers.ForceNextWindowMainViewport();
        ImGui.SetNextWindowSize(windowSize);
        ImGuiHelpers.SetNextWindowPosRelativeMainViewport(windowPos);

        if (!ImGui.Begin("Avant-Garde", WindowFlags))
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

            slotCategory = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[atkValueIndex].String).TextValue;
            if (slotCategory == "") { continue; }

            ImGui.SetCursorPos(buttonPos);
            using var child = ImRaii.Child($"##child-{slot}", new Vector2(buttonSize * 1.15f));
            if (child)
            {
                using var color = ImRaii.PushColor(ImGuiCol.Button, new Vector4(0.4f, 0.4f, 0.4f, 0.6f))
                                        .Push(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.3f, 0.3f, 0.7f))
                                        .Push(ImGuiCol.ButtonActive, new Vector4(0.2f, 0.2f, 0.2f, 0.8f))
                                        .Push(ImGuiCol.Border, new Vector4(0.125f, 0.094f, 0.067f, 1f));

                using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 2f)
                                        .Push(ImGuiStyleVar.FrameRounding, buttonSize * 0.5f);

                ImGui.SetCursorPos(ImGui.GetStyle().FramePadding);
                try
                {
                    if (GuiUtilities.IconButton(FontAwesomeIcon.List, new Vector2(buttonSize), "Show Gear"))
                    {
                        List<int>? itemIDs = [];
                        Service.DataManager.CategoryData.TryGetValue(GetCategoryID(slotCategory), out itemIDs);
                        SlotWindow.Update(slot, itemIDs, ImGui.GetWindowPos() + ImGui.GetStyle().FramePadding, buttonSize);
                    }
                }
                catch (Exception e) when (e is ArgumentNullException || e is NullReferenceException) {
                    Service.PluginLog.Error(e, $"Exception {e.GetType} while updating hint window with category: {slotCategory} for slot: {slot}");
                }
            }
        }
        SlotWindow.Draw();

        ImGui.End();
    }

    private Vector2 GetButtonPosition(AtkUnitBase* addon, AtkResNode* node, ItemSlot slot)
    {
        // Child nodes are all relative to their parent/addon, hence the seemingly random numbers ((246, 30) + (10, 48))
        var position = (new Vector2(256f + node->X, 78f + node->Y)
                        + new Vector2((node->Height * 0.1f) + 0.5f)) * addon->Scale;

        if (slot >= ItemSlot.Ears)
            // Width of the underlying NineGrid node
            position.X += 198f * addon->Scale;

        return position;
    }

    private static uint GetCategoryID(string category)
    {
        var themeCategory = Service.DalamudDataManager.GetExcelSheet<FashionCheckThemeCategory>(Service.ClientState.ClientLanguage);
        var matchingCategory = themeCategory?.FirstOrDefault(cat => cat.Name.ExtractText() == category)
            ?? throw new NullReferenceException();
        return matchingCategory.RowId;
    }
}
