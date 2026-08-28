using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Component.GUI;

using AvantGarde.Managers;
using AvantGarde.Utils;

namespace AvantGarde.UI;

public unsafe class MainWindow
{
    public AtkUnitBase* Addon = null;

    private static ImGuiWindowFlags WindowFlags => ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMouseInputs;

    private readonly SlotWindow SlotWindow = new();
    private readonly DyeSlotWindow DyeSlotWindow = new();

    public void Draw()
    {
        if (Addon is null) { return; }

        var windowPos = new Vector2(Addon->X, Addon->Y);
        var windowSize = new Vector2(Addon->RootNode->Width, Addon->RootNode->Height) * Addon->Scale;
        ImGuiHelpers.ForceNextWindowMainViewport();
        ImGui.SetNextWindowSize(windowSize);
        ImGuiHelpers.SetNextWindowPosRelativeMainViewport(windowPos);

        if (!ImGui.Begin("Avant-Garde", WindowFlags))
        {
            ImGui.End();
            return;
        }

        DrawDataCollectionCheckbox(Addon);

        foreach (var slot in Enum.GetValues<ItemSlot>())
        {
            var slotNodeId = 8 + (uint)slot;
            var atkValueIndex = 2 + ((uint)slot * 11);
            var slotCategory = Addon->AtkValues[atkValueIndex].String.ToString();
            var slotNode = Addon->GetNodeById(slotNodeId);

            var itemButtonSize = slotNode->Height * 0.8f * Addon->Scale;
            var dyeButtonSize = slotNode->Height * 0.5f * Addon->Scale;
            var itemButtonPos = GetItemButtonPos(slotNode, Addon->Scale);
            var dyeButtonPos = GetDyeButtonPos(slotNode, Addon->Scale);
            
            if (slot < ItemSlot.Ears)
            {
                ImGui.SetCursorPos(dyeButtonPos);
                var dyeButtonChildSize = dyeButtonSize * 1.15f;
                using var dyeButtonChild = ImRaii.Child($"##child-dye-{slot}", new Vector2(dyeButtonChildSize));
                if (dyeButtonChild)
                {
                    using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 2.5f)
                                            .Push(ImGuiStyleVar.FrameRounding, dyeButtonSize * 0.5f);

                    GuiUtilities.CenterNextElement(dyeButtonChildSize, dyeButtonSize);
                    if (GuiUtilities.IconButton(FontAwesomeIcon.Palette, new Vector2(dyeButtonSize), "Show Dyes"))
                    {
                        Service.DataManager.DyeData.TryGetValue((uint)slot, out var dyes);
                        DyeSlotWindow.Update(slot, dyes, ImGui.GetWindowPos() + ImGui.GetStyle().FramePadding, dyeButtonSize);
                    }
                }
            }

            if (slotCategory == "") { continue; }

            ImGui.SetCursorPos(itemButtonPos);
            using var child = ImRaii.Child($"##child-{slot}", new Vector2(itemButtonSize * 1.15f));
            if (child)
            {
                using var color = ImRaii.PushColor(ImGuiCol.Button, new Vector4(0.4f, 0.4f, 0.4f, 0.6f))
                                        .Push(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.3f, 0.3f, 0.7f))
                                        .Push(ImGuiCol.ButtonActive, new Vector4(0.2f, 0.2f, 0.2f, 0.8f))
                                        .Push(ImGuiCol.Border, new Vector4(0.125f, 0.094f, 0.067f, 1f));

                using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 2.5f)
                                        .Push(ImGuiStyleVar.FrameRounding, itemButtonSize * 0.2f);

                ImGui.SetCursorPos(ImGui.GetStyle().FramePadding);
                try
                {
                    if (GuiUtilities.IconButton(FontAwesomeIcon.List, new Vector2(itemButtonSize), "Show Gear"))
                    {
                        Service.DataManager.CategoryData.TryGetValue(DataManager.GetCategoryID(slotCategory), out var items);
                        SlotWindow.Update(slot, items, ImGui.GetWindowPos() + ImGui.GetStyle().FramePadding, itemButtonSize);
                    }
                }
                catch (Exception e) when (e is ArgumentNullException || e is NullReferenceException)
                {
                    Service.PluginLog.Error(e, $"Exception {e.GetType} while updating hint window with category: {slotCategory} for slot: {slot}");
                }
            }
        }
        DyeSlotWindow.Draw();
        SlotWindow.Draw();

        ImGui.End();
    }

    private void DrawDataCollectionCheckbox(AtkUnitBase* addon)
    {
        const string checkboxStr = "Opt-in to data collection";

        var weeklyThemeNode = Addon->GetNodeById(2);
        var pos = new Vector2(weeklyThemeNode->X + weeklyThemeNode->Width, weeklyThemeNode->Y + (weeklyThemeNode->Height * 0.5f)) * Addon->Scale;
        var height = ((ImGui.GetStyle().FramePadding.Y * 2f) + ImGui.GetFontSize()) * 1.75f;
        var width = ImGui.CalcTextSize(checkboxStr).X + height + ImGui.GetStyle().ItemSpacing.X;
        var size = new Vector2(width, height);

        using var color = ImRaii.PushColor(ImGuiCol.ChildBg, new Vector4(0.9f, 0.87f, 0.78f, 1f))
                                .Push(ImGuiCol.Text, new Vector4(0.36f, 0.24f, 0.19f, 1f))
                                .Push(ImGuiCol.FrameBg, new Vector4(0.67f, 0.59f, 0.41f, 1f))
                                .Push(ImGuiCol.Border, new Vector4(0.67f, 0.59f, 0.41f, 1f));
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 2f)
                                .Push(ImGuiStyleVar.FrameRounding, 2f)
                                .Push(ImGuiStyleVar.ChildBorderSize, 2f)
                                .Push(ImGuiStyleVar.ChildRounding, 5f);
        
        ImGui.SetCursorPos(pos);
        using var crowdsourcingChild = ImRaii.Child("##child-datacollection", size, true);
        if (crowdsourcingChild)
        {
            ImGui.Checkbox(checkboxStr, ref Service.PluginConfig.DataCollectionOptedIn);
        }
    }

    private Vector2 GetItemButtonPos(AtkResNode* node, float addonScale)
    {
        var buttonComponent = node->GetComponent()->GetNodeById(4);
        if (buttonComponent is null)
            return Vector2.Zero;

        // Child nodes are all relative to their parent/addon, hence the seemingly random numbers ((246, 30) + (10, 48) = (256, 78))
        // Small numbers are for minor adjustments
        return new Vector2(256f + node->X + buttonComponent->X, 78f + node->Y + buttonComponent->Y + 2f) * addonScale;
    }

    private Vector2 GetDyeButtonPos(AtkResNode* node, float addonScale)
    {
        var imageNode = node->GetComponent()->GetNodeById(3);
        if (imageNode is null)
            return Vector2.Zero;

        // Child nodes are all relative to their parent/addon, hence the seemingly random numbers ((246, 30) + (10, 48) = (256, 78))
        // Small numbers are for minor adjustments
        return new Vector2(256f + node->X + imageNode->X - 5f, 78f + node->Y - 3f) * addonScale;
    }
}
