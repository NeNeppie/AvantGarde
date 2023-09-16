using Lumina.Excel.GeneratedSheets;

namespace FashionReporter.Data;

public enum ItemSlot
{
    Head,
    Body,
    Hands,
    Legs,
    Feet,
    Ears,
    Neck,
    Wrists,
    RightRing,
    LeftRing
}

internal static class ItemSlotEx
{
    public static bool IsMatchingSlot(this ItemSlot slot, Item item)
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

    public static string GetDescription(this ItemSlot slot)
    {
        return slot switch
        {
            ItemSlot.RightRing => "Right Ring",
            ItemSlot.LeftRing => "Left Ring",
            _ => slot.ToString()
        };
    }
}
