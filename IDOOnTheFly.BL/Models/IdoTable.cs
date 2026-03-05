namespace IDOOnTheFly.BL.Models;

public enum TableType { PRIMARY, SECONDARY }
public enum JoinType { INNER, LEFT, RIGHT, CROSS }

public class IdoTable
{
    public string Name { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public TableType Type { get; set; } = TableType.PRIMARY;
    public string? ExplicitJoin { get; set; }
    public JoinType JoinType { get; set; } = JoinType.INNER;
}
