using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AvantGarde.Data;

public record GoldStamp(uint HintId, uint ItemId);

public class FashionCheckAtk
{
    private const uint GearSlotCount = 11;
    private const int StructMemberCount = 11;
    public string WeeklyTheme;
    public FashionCheckSlot[] Slots = new FashionCheckSlot[GearSlotCount];

    public FashionCheckAtk(Span<AtkValue> atkValues)
    {
        // if (atkValues.Length < GearSlotCount * StructMemberCount) {}

        WeeklyTheme = atkValues[0].String.ToString();
        for (var i = 1; i < GearSlotCount * StructMemberCount; i += StructMemberCount)
        {
            var slot = new FashionCheckSlot { 
                IsActive = atkValues[i].Bool, 
                Hint = atkValues[i + 1].String.ToString(), 
                StampType = atkValues[i + 2].UInt, 
                SlotName = atkValues[i + 4].String.ToString(),
                ItemId = atkValues[i + 5].UInt,
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
                stamps.Add(new GoldStamp(DataManager.GetCategoryID(slot.Hint), slot.ItemId));
            }
        }
        return stamps;
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
    //public uint unk4;
    //public uint DyeID;  // Stain?
    //public uint DyeUnk; // Connected
    //public uint unk7;
    //public uint unk8;
}

/*
Structure of FashionCheck AtkValues (as of Endwalker):
    [0] String - Weekly Theme
    [1] Sub-struct for every gear piece (except offhand)
        [0] Bool - Hint active
        [1] String - Hint
------> [2] UInt - Score stamp (0 is Gold, 4 is Low, 5 is None)
        [3] UInt - unk
        [4] String - Slot
------> [5] UInt - Item ID
        [6] UInt - unk
------> [7] UInt - Dye ID
        [8] UInt - unk (Connected to dye)
        [9] Bool - unk
        [10] Bool - unk
    [122] String - Remaining Attempts
    [123] String - High Score
    [124-132] Irrelevant

FashionCheckScoreGauge addon only shows during the judgment cutscene. AtkValues[0] stores the score
*/
