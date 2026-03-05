using IDOOnTheFly.BL.Generation;
using IDOOnTheFly.BL.Models;
using IDOOnTheFly.BL.Parsing;
using IDOOnTheFly.BL.Schema;

namespace IDOOnTheFly.UI2.Services;

public class IdoGeneratorService(SchemaService schemaService)
{
    /// <summary>
    /// Parse the SQL, merge schema metadata, and return the populated IdoDefinition.
    /// The caller sets Name/RevisionNo on the returned definition.
    /// </summary>
    public async Task<IdoDefinition> ParseAsync(string sql)
    {
        var ido = SqlSelectParser.Parse(sql);

        // Enrich properties with schema metadata
        var schema = await schemaService.GetSchemaAsync();
        if (schema != null)
            EnrichFromSchema(ido, schema);

        return ido;
    }

    public string Generate(IdoDefinition ido) => IdoXmlGenerator.Generate(ido);

    private static void EnrichFromSchema(IdoDefinition ido, ErpSchema schema)
    {
        // Build alias→table map
        var aliasMap = ido.Tables.ToDictionary(
            t => t.Alias, t => t.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var prop in ido.Properties.Where(p => p.Binding == PropertyBinding.BOUND))
        {
            if (!aliasMap.TryGetValue(prop.ColumnTableAlias, out var tableName))
                continue;

            var schemaTable = schema.Tables.FirstOrDefault(t =>
                t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (schemaTable == null) continue;

            var col = schemaTable.Columns.FirstOrDefault(c =>
                c.ColumnName.Equals(prop.BoundToColumn, StringComparison.OrdinalIgnoreCase));
            if (col == null) continue;

            // Update PropertyClass from SQL type
            prop.PropertyClass = PropertyClassMapper.GetPropertyClass(col.DataType);
        }
    }
}
