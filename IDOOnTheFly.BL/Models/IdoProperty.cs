namespace IDOOnTheFly.BL.Models;

public enum PropertyBinding { BOUND, DERIVED, UNBOUND }

public class IdoProperty
{
    public string Name { get; set; } = string.Empty;
    public bool IsKey { get; set; } = false;
    public PropertyBinding Binding { get; set; } = PropertyBinding.BOUND;
    public int Sequence { get; set; }

    // BOUND
    public string BoundToColumn { get; set; } = string.Empty;
    public string ColumnTableAlias { get; set; } = string.Empty;

    // DERIVED
    public string Expression { get; set; } = string.Empty;

    public string PropertyClass { get; set; } = "String";

    // Optional PropertyAttributes (populated from schema)
    public PropertyAttributes? Attributes { get; set; }
}

public class PropertyAttributes
{
    public string? ColumnDataType { get; set; }
    public int? DataLength { get; set; }
    public string? DataType { get; set; }
    public bool IsRequired { get; set; }
}
