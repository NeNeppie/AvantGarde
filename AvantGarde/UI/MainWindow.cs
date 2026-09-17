using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;

using AvantGarde.Managers;
using AvantGarde.Utilities;

namespace AvantGarde.UI;

public unsafe class MainWindow
{
    public AtkUnitBase* Addon
    {
        get;
        set
        {
            field = value;
            ConfigurationWindow.Addon = value;
        }
    } = null;

    private static ImGuiWindowFlags WindowFlags => ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMouseInputs;

    private readonly ItemSlotWindow SlotWindow = new();
    private readonly DyeSlotWindow DyeSlotWindow = new();
    private readonly ConfigurationWindow ConfigurationWindow = new();

    public void Draw()
    {
        if (Addon is null) { return; }

        var windowPos = new Vector2(Addon->X, Addon->Y);
        var windowSize = new Vector2(Addon->RootNode->Width, Addon->RootNode->Height) * Addon->Scale * 1.1f;
        ImGuiHelpers.ForceNextWindowMainViewport();
        ImGui.SetNextWindowSize(windowSize);
        ImGuiHelpers.SetNextWindowPosRelativeMainViewport(windowPos);

        if (!ImGui.Begin("Avant-Garde", WindowFlags))
        {
            ImGui.End();
            return;
        }

        var configButtonSize = 30f * Addon->Scale;
        var configButtonPos = new Vector2(735, 47.5f) * Addon->Scale;
        
        DrawConfigButton(configButtonSize, configButtonPos);

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
            
            DrawDyeSlotButton(slot, dyeButtonSize, dyeButtonPos);
            DrawItemSlotButton(slot, slotCategory, itemButtonSize, itemButtonPos);
        }
        
        DyeSlotWindow.Draw();
        SlotWindow.Draw();
        ConfigurationWindow.Draw();

        ImGui.End();
    }

    private void DrawConfigButton(float size, Vector2 position)
    {
        using var buttonWindow = ImGuiUtils.BeginButtonWindow(size, position, "##child-openconfig");
        if (!buttonWindow)
            return;

        if (ImGuiUtils.IconButtonThemed(FontAwesomeIcon.Cog, size, "Open Settings"))
            ConfigurationWindow.ToggleVisible();
    }

    private void DrawDyeSlotButton(ItemSlot slot, float size, Vector2 position)
    {
        if (slot >= ItemSlot.Ears)
            return;
        
        using var buttonWindow = ImGuiUtils.BeginButtonWindow(size, position, $"##child-dye-{slot}");
        if (!buttonWindow)
            return;

        if (ImGuiUtils.IconButtonThemed(FontAwesomeIcon.Palette, size, "Show Dyes", circular: true))
        {
            Service.DataManager.DyeData.TryGetValue((uint)slot, out var dyes);
            DyeSlotWindow.Update(slot, dyes, ImGui.GetWindowPos() + ImGui.GetStyle().FramePadding, size);
        }
    }

    private void DrawItemSlotButton(ItemSlot slot, string slotCategory, float size, Vector2 position)
    {
        if (slotCategory == "")
            return;

        using var buttonWindow = ImGuiUtils.BeginButtonWindow(size, position, $"##child-item-{slot}");
        if (!buttonWindow)
            return;

        try
        {
            if (ImGuiUtils.IconButtonThemed(FontAwesomeIcon.List, size, "Show Gear"))
            {
                Service.DataManager.CategoryData.TryGetValue(DataManager.GetCategoryID(slotCategory), out var items);
                SlotWindow.Update(slot, items, ImGui.GetWindowPos() + ImGui.GetStyle().FramePadding, size);
            }
        }
        catch (Exception e) when (e is ArgumentNullException || e is NullReferenceException)
        {
            Service.PluginLog.Error(e, $"Exception {e.GetType} while updating hint window with category: {slotCategory} for slot: {slot}");
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
