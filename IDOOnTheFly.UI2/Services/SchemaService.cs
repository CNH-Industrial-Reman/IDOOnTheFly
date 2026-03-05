using Blazored.LocalStorage;
using IDOOnTheFly.BL.Models;
using System.Text;

namespace IDOOnTheFly.UI2.Services;

public class SchemaService(ILocalStorageService localStorage)
{
    private const string StorageKey = "erpSchema";
    private ErpSchema? _cache;

    public async Task<ErpSchema?> GetSchemaAsync()
    {
        _cache ??= await localStorage.GetItemAsync<ErpSchema>(StorageKey);
        return _cache;
    }

    public async Task SaveSchemaAsync(ErpSchema schema)
    {
        _cache = schema;
        await localStorage.SetItemAsync(StorageKey, schema);
    }

    public async Task ClearSchemaAsync()
    {
        _cache = null;
        await localStorage.RemoveItemAsync(StorageKey);
    }

    /// <summary>Parse CSV: table_name,column_name,data_type,length,nullable</summary>
    public ErpSchema ParseCsv(string csvContent)
    {
        var schema = new ErpSchema();
        var tableMap = new Dictionary<string, SchemaTable>(StringComparer.OrdinalIgnoreCase);

        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        bool header = true;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (header)
            {
                header = false;
                // Skip header row if it starts with "table_name" (case-insensitive)
                if (line.StartsWith("table_name", StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            var parts = line.Split(',');
            if (parts.Length < 3) continue;

            var tableName = parts[0].Trim();
            var colName = parts[1].Trim();
            var dataType = parts[2].Trim();
            int? length = parts.Length > 3 && int.TryParse(parts[3].Trim(), out var l) ? l : null;
            var nullable = parts.Length <= 4 || !parts[4].Trim().Equals("false", StringComparison.OrdinalIgnoreCase);

            if (!tableMap.TryGetValue(tableName, out var table))
            {
                table = new SchemaTable { TableName = tableName };
                tableMap[tableName] = table;
                schema.Tables.Add(table);
            }

            table.Columns.Add(new SchemaColumn
            {
                ColumnName = colName,
                DataType = dataType,
                Length = length,
                Nullable = nullable
            });
        }

        return schema;
    }

    public string ExportCsv(ErpSchema schema)
    {
        var sb = new StringBuilder();
        sb.AppendLine("table_name,column_name,data_type,length,nullable");
        foreach (var table in schema.Tables)
            foreach (var col in table.Columns)
                sb.AppendLine($"{table.TableName},{col.ColumnName},{col.DataType},{col.Length},{col.Nullable}");
        return sb.ToString();
    }
}
