namespace MoMS.Server.Utilities;

public static class CategoryColors
{
    public static string ForCategory(string? category)
    {
        return (category?.ToUpperInvariant() ?? string.Empty) switch
        {
            "PRODUCTION RUNNING" => "#00ff00",
            "PRODUCT BUYOFF" => "#808080",
            "NO OPERATOR" or "NO SCHEDULE" or "MATERIAL DRYING" or "OTHERS PROD" => "#ffff00",
            "QUALITY ISSUE" or "SAMPLE RUNNING" or "MOULD CHANGE" or "OTHERS TECH" => "#ff0000",
            "SCHEDULED MAINTENANCE" or "MACHINE BREAKDOWN" or "OTHERS MAIN" => "#ffa500",
            _ => "#808080"
        };
    }
}