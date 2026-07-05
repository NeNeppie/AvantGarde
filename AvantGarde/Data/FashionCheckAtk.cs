using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AvantGarde.Data;

public record GoldStamp(uint HintId, uint ItemId);

public class FashionCheckAtk
{
    private const uint GearSlotCount = 11;
    private const int StructMemberCount = 11;
    public static readonly int AtkValueCount = 134;
    public string WeeklyTheme;
    public FashionCheckSlot[] Slots = new FashionCheckSlot[GearSlotCount];

    public FashionCheckAtk(Span<AtkValue> atkValues)
    {
        WeeklyTheme = atkValues[0].String.ToString();
        for (var i = 1; i < GearSlotCount * StructMemberCount; i += StructMemberCount)
        {
            var slot = new FashionCheckSlot
            {
                IsActive = atkValues[i].Bool,
                Hint = atkValues[i + 1].String.ToString(),
                StampType = atkValues[i + 2].UInt,
                SlotName = atkValues[i + 4].String.ToString(),
                ItemId = atkValues[i + 5].UInt,
                StainIdPrimary = atkValues[i + 7].UInt,
                StainIdSecondary = atkValues[i + 8].UInt,
            };
            Slots[i / StructMemberCount] = slot;
        }
    }

    public List<GoldStamp> GetGoldStamps()
    {
        var stamps = new List<GoldStamp>();
        foreach (var slot in Slots)
        {
            if (slot.IsActive && slot.StampType == 0)
            {
                // TODO: Substruct HQ from the ID!!!
                stamps.Add(new GoldStamp(DataManager.GetCategoryID(slot.Hint), slot.ItemId));
            }
        }
        return stamps;
    }

    public Export Export()
    {
        Export export = new();
        List<Category> categories = [];
        List<uint> itemIds = [];
        List<uint> stainIds = [];

        foreach (var slot in Slots)
        {
            categories.Add(new(DataManager.GetCategoryID(slot.Hint), slot.StampType));
            itemIds.Add(slot.ItemId);
            stainIds.AddRange(slot.StainIdPrimary, slot.StainIdSecondary);
        }

        export.Categories = categories;
        export.ItemIds = itemIds;
        export.StainIds = stainIds;
        return export;
    }
}

public readonly record struct FashionCheckSlot
{
    public bool IsActive { get; init; }
    public string Hint { get; init; }
    public uint StampType { get; init; } // 0-5 where 0 is Gold and 5 is empty
    //public uint unk1;
    public string SlotName { get; init; }
    public uint ItemId { get; init; }
    //public uint unk2;
    public uint StainIdPrimary { get; init; }
    public uint StainIdSecondary { get; init; }
    //public uint unk3;
    //public uint unk4;
}

/*
Structure of FashionCheck AtkValues (as of Endwalker):
    [0] String - Weekly Theme
    [1] Sub-struct for every gear piece (except offhand)
        [0] Bool - Hint active
        [1] String - Hint
------> [2] UInt - Score stamp*
        [3] UInt - unk
        [4] String - Slot
------> [5] UInt - Item ID
        [6] UInt - unk
------> [7] UInt - Stain #1 RowId
------> [8] UInt - Stain #2 RowId
        [9] UInt - unk
        [10] UInt - unk
    [122] String - Remaining Attempts
    [123] String - High Score
    [124-132] Irrelevant

* Score stamp: Value is 5 for an inactive hint, and starts at 4 for an active hint. 0 is Gold.
FashionCheckScoreGauge addon only shows during the judgment cutscene. AtkValues[0] stores the score
*/

/*
Score Bullshittery:
    Base Score                      -> 68
    Base score, 1 Right Side Hint   -> 70
    Weapon                          -> 10
    Weapon, 1 Right Hint            -> 10
    Weapon + Body, 1 Right Hint     -> 20
    Weapon + Feet(H), 1 Right Hint  -> 12
    Weapon + Ear, 1 Right Hint      -> 18
    Weapon + Neck(H), 1 Right Hint  -> 12

Theorycrafting:
    Left side by default gives 10 points. Right side 8 by default.
    An active hint drops the score down to 2 points. The stamp "upgrades" for every 2 points gained.
    Points can be gained either by choosing the correct item for the hint or the correct dye for the slot.
    Barring extremely rare circumstances all correct items grant at leaast +8p.
    For each slot with an active hint there exists an item (very rarely multiple) which grants an additional point (+9p).
    Lastly, choosing the correct shade for a dye grants a point (determined by the icon of the dye item, NOT the Stain Shade column). Choosing the exact dye grants +2p
*/
