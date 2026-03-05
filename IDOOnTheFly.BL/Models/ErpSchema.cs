namespace IDOOnTheFly.BL.Models;

public class ErpSchema
{
    public List<SchemaTable> Tables { get; set; } = [];
}

public class SchemaTable
{
    public string TableName { get; set; } = string.Empty;
    public List<SchemaColumn> Columns { get; set; } = [];
}

public class SchemaColumn
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int? Length { get; set; }
    public bool Nullable { get; set; } = true;
}
