using System.Text.RegularExpressions;
using IDOOnTheFly.BL.Models;

namespace IDOOnTheFly.BL.Parsing;

/// <summary>
/// Lightweight T-SQL SELECT parser. WASM-safe — no external dependencies.
/// Handles: FROM, JOIN, SELECT col AS Name, SELECT expr AS Name (DERIVED).
/// </summary>
public static partial class SqlSelectParser
{
    [GeneratedRegex(@"\bFROM\s+(\[?(?<table>\w+)\]?)\s+(?:AS\s+)?(?<alias>\w+)", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex FromRegex();

    [GeneratedRegex(@"\b(?<jointype>INNER|LEFT\s+OUTER|LEFT|RIGHT\s+OUTER|RIGHT|CROSS|FULL\s+OUTER|FULL)?\s*JOIN\s+(\[?(?<table>\w+)\]?)\s+(?:AS\s+)?(?<alias>\w+)\s+ON\s+(?<condition>.+?)(?=\s+(?:INNER|LEFT|RIGHT|CROSS|FULL|WHERE|GROUP|ORDER|HAVING)|\s*$)", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex JoinRegex();

    public static IdoDefinition Parse(string sql)
    {
        var result = new IdoDefinition();
        var normalizedSql = NormalizeWhitespace(sql);

        ParseTables(normalizedSql, result);
        ParseProperties(normalizedSql, result);

        return result;
    }

    private static void ParseTables(string sql, IdoDefinition result)
    {
        // FROM clause → PRIMARY table
        var fromMatch = FromRegex().Match(sql);
        if (fromMatch.Success)
        {
            result.Tables.Add(new IdoTable
            {
                Name = fromMatch.Groups["table"].Value,
                Alias = fromMatch.Groups["alias"].Value,
                Type = TableType.PRIMARY
            });
        }

        // JOIN clauses → SECONDARY tables
        foreach (Match m in JoinRegex().Matches(sql))
        {
            var joinTypeStr = NormalizeWhitespace(m.Groups["jointype"].Value).Trim().ToUpperInvariant();
            var joinType = joinTypeStr switch
            {
                "LEFT" or "LEFT OUTER" => JoinType.LEFT,
                "RIGHT" or "RIGHT OUTER" => JoinType.RIGHT,
                "CROSS" => JoinType.CROSS,
                _ => JoinType.INNER
            };

            result.Tables.Add(new IdoTable
            {
                Name = m.Groups["table"].Value,
                Alias = m.Groups["alias"].Value,
                Type = TableType.SECONDARY,
                ExplicitJoin = m.Groups["condition"].Value.Trim(),
                JoinType = joinType
            });
        }
    }

    private static void ParseProperties(string sql, IdoDefinition result)
    {
        // Extract the SELECT list (between SELECT and FROM)
        var selectMatch = Regex.Match(sql, @"\bSELECT\s+(?<cols>.+?)\s+FROM\b", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (!selectMatch.Success)
            return;

        var columnList = selectMatch.Groups["cols"].Value;

        // Split on commas that are NOT inside parentheses
        var columns = SplitOnTopLevelCommas(columnList);

        int seq = 1;
        foreach (var col in columns)
        {
            var trimmed = col.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed == "*")
                continue;

            var prop = ParseColumn(trimmed, seq);
            if (prop != null)
            {
                result.Properties.Add(prop);
                seq++;
            }
        }
    }

    private static IdoProperty? ParseColumn(string col, int seq)
    {
        // Detect AS alias: everything before " AS <alias>" is the expression
        var asMatch = Regex.Match(col, @"^(?<expr>.+?)\s+AS\s+(?<alias>\w+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        string expr;
        string propName;

        if (asMatch.Success)
        {
            expr = asMatch.Groups["expr"].Value.Trim();
            propName = asMatch.Groups["alias"].Value.Trim();
        }
        else
        {
            expr = col.Trim();
            // No alias: derive property name from last segment after '.'
            propName = expr.Contains('.') ? ToPascalCase(expr[(expr.LastIndexOf('.') + 1)..]) : ToPascalCase(expr);
        }

        // BOUND: simple alias.column or just column (no spaces, no function calls)
        var boundMatch = Regex.Match(expr, @"^(?:(?<tableAlias>\w+)\.)?(?<column>\w+)$");
        if (boundMatch.Success)
        {
            return new IdoProperty
            {
                Name = propName,
                Sequence = seq,
                Binding = PropertyBinding.BOUND,
                BoundToColumn = boundMatch.Groups["column"].Value,
                ColumnTableAlias = boundMatch.Groups["tableAlias"].Value,
                PropertyClass = "String"
            };
        }

        // DERIVED: expression contains spaces, functions, operators, etc.
        return new IdoProperty
        {
            Name = propName,
            Sequence = seq,
            Binding = PropertyBinding.DERIVED,
            Expression = expr,
            PropertyClass = "String"
        };
    }

    private static List<string> SplitOnTopLevelCommas(string input)
    {
        var parts = new List<string>();
        int depth = 0;
        int start = 0;
        for (int i = 0; i < input.Length; i++)
        {
            switch (input[i])
            {
                case '(':
                    depth++;
                    break;
                case ')':
                    depth--;
                    break;
                case ',' when depth == 0:
                    parts.Add(input[start..i]);
                    start = i + 1;
                    break;
            }
        }
        parts.Add(input[start..]);
        return parts;
    }

    private static string ToPascalCase(string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        // Handle snake_case
        var parts = s.Split('_');
        return string.Concat(parts.Select(p =>
            p.Length == 0 ? "" : char.ToUpperInvariant(p[0]) + p[1..].ToLowerInvariant()));
    }

    private static string NormalizeWhitespace(string s)
        => Regex.Replace(s, @"\s+", " ").Trim();
}
