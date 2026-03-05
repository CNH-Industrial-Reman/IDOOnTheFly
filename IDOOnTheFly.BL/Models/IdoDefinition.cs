namespace IDOOnTheFly.BL.Models;

public class IdoDefinition
{
    public string Name { get; set; } = string.Empty;
    public int RevisionNo { get; set; } = 1;
    public List<IdoTable> Tables { get; set; } = [];
    public List<IdoProperty> Properties { get; set; } = [];

    /// <summary>Text before the first '_' in Name, or the full Name if no '_'.</summary>
    public string ServerName =>
        Name.Contains('_') ? Name[..Name.IndexOf('_')] : Name;
}
