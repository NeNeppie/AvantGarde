using System;
using Lumina.Excel.Sheets;

namespace AvantGarde.Managers;

public abstract class ItemSource(uint ItemId)
{
    public SourceType SourceType { get; protected init; }
    public uint DisplayIcon { get; protected init; }
    public uint ItemId = ItemId;

    public abstract string AsText();
    public abstract void OnClick();
}

// NOTE:
public class ItemSourceCrafting : ItemSource
{
    private readonly uint _recipeId;
    private readonly uint _recipeLevel;
    private readonly string _craftType;
    private readonly string? _recipeMasterBook;

    public ItemSourceCrafting(uint itemId, Recipe recipe) : base(itemId)
    {
        SourceType = SourceType.Crafting;
        DisplayIcon = SourceIcons.SourceTypeIconCrafting;
        _recipeId = recipe.RowId;
        _recipeLevel = recipe.RecipeLevelTable.Value.ClassJobLevel;
        _craftType = recipe.CraftType.Value.Name.ExtractText();
        
        var secretRecipeBook = recipe.SecretRecipeBook.Value.Name.ExtractText();
        _recipeMasterBook = string.IsNullOrEmpty(secretRecipeBook) ? null : secretRecipeBook;
    }

    public override string AsText() =>
#if DEBUG
        $"Made via {_craftType}\n[{_recipeId}] {_recipeMasterBook ?? $"Recipe Lv. {_recipeLevel}"}";
#else
        $"Made via {_craftType}\n{_recipeMasterBook ?? $"Recipe Lv. {_recipeLevel}"}";
#endif

    public override void OnClick()
    {
        throw new NotImplementedException(); // Open crafting menu on recipe
    }
}

// TODO:
public class ItemSourceExchange : ItemSource
{
    private readonly uint _shopId;
    private readonly uint _currency;
    private readonly uint _cost;

    public ItemSourceExchange(uint shopId, uint currency, uint cost) : base(0)
    {
        SourceType = SourceType.Exchange;
        DisplayIcon = SourceIcons.SourceTypeIconExchange;
        _shopId = shopId;
        _currency = currency;
        _cost = cost;
    }

    public override string AsText()
    {
        throw new NotImplementedException();
    }

    public override void OnClick() { }
}

// NOTE:
public class ItemSourceShop : ItemSource
{
    private readonly uint _shopId;
    private readonly uint _cost;
    private readonly string _npcName;
    private readonly string _location;

    public ItemSourceShop(uint shopId, uint cost, string npcName, string zoneName) : base(0)
    {
        SourceType = SourceType.Shop;
        DisplayIcon = SourceIcons.SourceTypeIconGil;
        _shopId = shopId;
        _cost = cost;
        _npcName = npcName;
        _location = zoneName;
    }

    public override string AsText()
    {
#if DEBUG
        return $"Sold for {_cost}\xE049\n[{_shopId}] {_location} - {_npcName}";
#else
        return $"Sold for {_cost}\xE049\n{_location} - {_npcName}"
#endif
    }

    public override void OnClick() { }
}

// NOTE:
public class ItemSourceMarketboard : ItemSource
{
    public ItemSourceMarketboard() : base(0)
    {
        SourceType = SourceType.Marketboard;
        DisplayIcon = SourceIcons.SourceTypeIconSearch;
    }

    public override string AsText()
    {
        return "Sold on the Marketboard";
    }

    public override void OnClick()
    {
        throw new NotImplementedException(); // Can include data from Universalis in the future
    }
}

// NOTE:
public class ItemSourceMogstore : ItemSource
{
    private readonly uint _productId;

    public ItemSourceMogstore(uint productId) : base(0)
    {
        SourceType = SourceType.Mogstore;
        DisplayIcon = SourceIcons.SourceTypeIconMogstore;
        _productId = productId;
    }

    public override string AsText()
    {
        return $"Bought on the\nFFXIV Online Store";
    }

    public override void OnClick()
    {
        throw new NotImplementedException(); // Include widget functionality to open link to store
    }
}

// TODO:
public class ItemSourceDesynth : ItemSource
{
    private readonly uint _desynthSource;

    public ItemSourceDesynth(uint desynthSource) : base(0)
    {
        SourceType = SourceType.Desynth;
        DisplayIcon = SourceIcons.SourceTypeIconGathering; // TEMP
        _desynthSource = desynthSource;
    }

    public override string AsText()
    {
        throw new NotImplementedException();
    }

    public override void OnClick()
    {
        throw new NotImplementedException();
    }
}

// TODO:
public class ItemSourceCoffer : ItemSource
{
    private readonly uint _coffer;
    private readonly uint _territoryId;

    public ItemSourceCoffer(uint coffer, uint territoryId) : base(0)
    {
        SourceType = SourceType.Coffer;
        DisplayIcon = SourceIcons.SourceTypeIconTreasure; // TEMP
        _coffer = coffer;
        _territoryId = territoryId;
    }

    public override string AsText()
    {
        throw new NotImplementedException();
    }

    public override void OnClick()
    {
        throw new NotImplementedException();
    }
}

// TODO:
public class ItemSourceDrop : ItemSource
{
    private readonly uint _contentId;
    private readonly string? _contentName = null;

    public ItemSourceDrop(ContentFinderCondition contentFinderCondition) : base(0)
    {
        SourceType = SourceType.Drop;
        DisplayIcon = contentFinderCondition.ContentType.Value.Icon;
        _contentId = contentFinderCondition.RowId;

        var str = contentFinderCondition.Name.ExtractText();
        _contentName = string.Concat(str[0].ToString().ToUpper(), str.AsSpan(1));
    }

    public override string AsText()
    {
#if DEBUG
        return $"Drops from: [{_contentId}]\n{_contentName}";
#else
        return $"Drops from:\n{_contentName}";
#endif
    }

    public override void OnClick()
    {
        throw new NotImplementedException();
    }
}

// TODO:
public class ItemSourceAchievement : ItemSource
{
    private readonly uint _achievementId;

    public ItemSourceAchievement(uint achievementId) : base(0)
    {
        SourceType = SourceType.Achievement;
        DisplayIcon = SourceIcons.SourceTypeIconAchievement;
        _achievementId = achievementId;
    }

    public override string AsText()
    {
        throw new NotImplementedException();
    }

    public override void OnClick()
    {
        throw new NotImplementedException();
    }
}

// TODO:
public class ItemSourceVenture : ItemSource
{
    private readonly uint _ventureType;

    public ItemSourceVenture(uint ventureType) : base(0)
    {
        SourceType = SourceType.Venture;
        DisplayIcon = SourceIcons.SourceTypeIconQuest; // TEMP
        _ventureType = ventureType;
    }

    public override string AsText()
    {
        throw new NotImplementedException();
    }

    public override void OnClick()
    {
        throw new NotImplementedException();
    }
}

// TODO:
public class ItemSourceLockbox : ItemSource
{
    private readonly uint _coffer;
    private readonly uint _territoryId;

    public ItemSourceLockbox(uint coffer, uint territoryId) : base(0)
    {
        SourceType = SourceType.Lockbox;
        DisplayIcon = SourceIcons.SourceTypeIconTreasure;
        _coffer = coffer;
        _territoryId = territoryId;
    }

    public override string AsText()
    {
        throw new NotImplementedException();
    }

    public override void OnClick()
    {
        throw new NotImplementedException();
    }
}

// TODO:
public class ItemSourceTreasure : ItemSource
{
    private readonly uint _source;
    private readonly uint _territoryId;

    public ItemSourceTreasure(uint source, uint territoryId) : base(0)
    {
        SourceType = SourceType.Treasure;
        DisplayIcon = SourceIcons.SourceTypeIconTreasure;
        _source = source;
        _territoryId = territoryId;
    }

    public override string AsText()
    {
        throw new NotImplementedException();
    }

    public override void OnClick()
    {
        throw new NotImplementedException();
    }
}

public enum SourceType : byte
{
    Crafting = 0,   // Self explanatory.
    Exchange,       // Special Shop - non-gil-currencies, raids tokens, tomestones, etc.
    Shop,           // Gil Shops (Gridania/Uldah/Limsa etc.)
    Marketboard,    // e.g. old Starlight Celebration set
    Mogstore,       // Items bought at the online store
    Desynth,        // e.g. rare fishing drops
    Coffer,         // Guaranteed-loot coffers? (e.g. savage gear, paladin arms)
    Drop,           // Direct drops in duties? (e.g. dungeon gear, extreme weapons)
    Achievement,    // Items claimed from the achievement menu. Certificates would go under `Exchange`
    Venture,        // e.g. rare ARR gear exclusive to Ventures
    Lockbox,        // RNG coffers? (e.g. eureka, bozja, island sanctuary)
    Treasure        // RNG world drops? (e.g. bunny fates, occult crescent)

    // There's like 1 fucking item that can directly drop from overworld enemies
    // Also quest rewards? I think i'll exclude due to their one-off nature
}

#pragma warning disable format
// TEMP: Not all icons are used here, and some sources might need icons that aren't here rn
internal class SourceIcons
{
    public const ushort SourceTypeIconGil           = 061758;
    public const ushort SourceTypeIconAchievement   = 061767;
    public const ushort SourceTypeIconDungeon       = 061801;
    public const ushort SourceTypeIconRaid          = 061802;
    public const ushort SourceTypeIconTrial         = 061804;
    public const ushort SourceTypeIconQuestionMark  = 061807;
    public const ushort SourceTypeIconTreasure      = 061808;
    public const ushort SourceTypeIconGathering     = 061815;
    public const ushort SourceTypeIconCrafting      = 061816;
    public const ushort SourceTypeIconVenture       = 061818;
    public const ushort SourceTypeIconMGP           = 061820;
    public const ushort SourceTypeIconHeart         = 061827;
    public const ushort SourceTypeIconSpecial       = 061830;
    public const ushort SourceTypeIconFieldOps      = 061837;
    public const ushort SourceTypeIconQuest         = 061839;
    public const ushort SourceTypeIconExchange      = 061843;
    public const ushort SourceTypeIconSearch        = 061845;
    public const ushort SourceTypeIconMogstore      = 061849;
}
#pragma warning restore format
