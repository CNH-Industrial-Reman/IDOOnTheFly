namespace IDOOnTheFly.BL.Schema;

public static class PropertyClassMapper
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["VARCHAR"]         = "String",
        ["NVARCHAR"]        = "String",
        ["CHAR"]            = "String",
        ["NCHAR"]           = "String",
        ["TEXT"]            = "String",
        ["NTEXT"]           = "String",
        ["INT"]             = "Int",
        ["INTEGER"]         = "Int",
        ["SMALLINT"]        = "Int",
        ["TINYINT"]         = "Int",
        ["BIGINT"]          = "Int",
        ["DECIMAL"]         = "Amount",
        ["NUMERIC"]         = "Amount",
        ["MONEY"]           = "Amount",
        ["SMALLMONEY"]      = "Amount",
        ["DATETIME"]        = "Date",
        ["DATETIME2"]       = "Date",
        ["DATE"]            = "Date",
        ["SMALLDATETIME"]   = "Date",
        ["BIT"]             = "ListYesNo",
        ["FLOAT"]           = "Qty",
        ["REAL"]            = "Qty",
        ["UNIQUEIDENTIFIER"] = "Guid",
    };

    public static string GetPropertyClass(string sqlType)
    {
        // Strip length/precision e.g. "VARCHAR(30)" → "VARCHAR"
        var baseType = sqlType.Split('(')[0].Trim();
        return Map.TryGetValue(baseType, out var pc) ? pc : "String";
    }
}
