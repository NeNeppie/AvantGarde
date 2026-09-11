namespace AvantGarde;

public enum SortingMode : byte
{
    InternalId = 0,
    PopularityDescending,
    PopularityAscending,
    OwnershipAT
}

internal static class SortingModeEx
{
    public static string GetDescription(this SortingMode value)
    {
        return value switch
        {
            SortingMode.InternalId => "Internal ID",
            SortingMode.PopularityDescending => "Popularity (Descending)",
            SortingMode.PopularityAscending => "Popularity (Ascending)",
            _ => value.ToString()
        };
    }
}
