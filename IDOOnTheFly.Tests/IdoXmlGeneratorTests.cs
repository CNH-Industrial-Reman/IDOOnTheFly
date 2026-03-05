using IDOOnTheFly.BL.Generation;
using IDOOnTheFly.BL.Models;
using System.Xml.Linq;
using Xunit;

namespace IDOOnTheFly.Tests;

public class IdoXmlGeneratorTests
{
    private static IdoDefinition BuildCNHLeadTime()
    {
        return new IdoDefinition
        {
            Name = "CNH_LeadTimeUpdate",
            RevisionNo = 3,
            Tables =
            [
                new IdoTable { Name = "CNH_LeadTimeUpdate", Alias = "ltu", Type = TableType.PRIMARY }
            ],
            Properties =
            [
                new IdoProperty { Name = "CreateDate",     Sequence = 1,  Binding = PropertyBinding.BOUND, BoundToColumn = "CreateDate",     ColumnTableAlias = "ltu", PropertyClass = "CurrentDate" },
                new IdoProperty { Name = "UpdatedBy",      Sequence = 2,  Binding = PropertyBinding.BOUND, BoundToColumn = "UpdatedBy",      ColumnTableAlias = "ltu", PropertyClass = "UserName" },
                new IdoProperty { Name = "CreatedBy",      Sequence = 3,  Binding = PropertyBinding.BOUND, BoundToColumn = "CreatedBy",      ColumnTableAlias = "ltu", PropertyClass = "UserName" },
                new IdoProperty { Name = "RowPointer",     Sequence = 4,  Binding = PropertyBinding.BOUND, BoundToColumn = "RowPointer",     ColumnTableAlias = "ltu", PropertyClass = "RowPointer" },
                new IdoProperty { Name = "CurrentDate",    Sequence = 5,  Binding = PropertyBinding.BOUND, BoundToColumn = "RecordDate",     ColumnTableAlias = "ltu", PropertyClass = "CurrentDate" },
                new IdoProperty { Name = "NoteExistsFlag", Sequence = 6,  Binding = PropertyBinding.BOUND, BoundToColumn = "NoteExistsFlag", ColumnTableAlias = "ltu", PropertyClass = "FlagNy" },
                new IdoProperty { Name = "InWorkflow",     Sequence = 7,  Binding = PropertyBinding.BOUND, BoundToColumn = "InWorkflow",     ColumnTableAlias = "ltu", PropertyClass = "InWorkflowBase" },
                new IdoProperty { Name = "LeadTime",       Sequence = 9,  Binding = PropertyBinding.BOUND, BoundToColumn = "lead_time",      ColumnTableAlias = "ltu", PropertyClass = "LeadTime",
                    Attributes = new PropertyAttributes { DataType = "Short Integer", IsRequired = true } },
                new IdoProperty { Name = "Item",           Sequence = 10, Binding = PropertyBinding.BOUND, BoundToColumn = "item",           ColumnTableAlias = "ltu", PropertyClass = "Item",
                    Attributes = new PropertyAttributes { ColumnDataType = "ItemType", DataLength = 30, DataType = "String", IsRequired = true } },
                new IdoProperty { Name = "VendNum",        Sequence = 11, Binding = PropertyBinding.BOUND, BoundToColumn = "vend_num",       ColumnTableAlias = "ltu", PropertyClass = "VendNum",
                    Attributes = new PropertyAttributes { ColumnDataType = "VendNumType", DataLength = 7, DataType = "NumSortedString", IsRequired = true } },
            ]
        };
    }

    [Fact]
    public void Generate_ProducesValidXml()
    {
        var ido = BuildCNHLeadTime();
        var xml = IdoXmlGenerator.Generate(ido, new DateTime(2026, 2, 13, 16, 23, 34));

        var doc = XDocument.Parse(xml); // throws if invalid XML
        Assert.NotNull(doc);
    }

    [Fact]
    public void Generate_RootElement_IsObjectStudioExport()
    {
        var xml = IdoXmlGenerator.Generate(BuildCNHLeadTime());
        var doc = XDocument.Parse(xml);
        Assert.Equal("ObjectStudioExport", doc.Root?.Name.LocalName);
        Assert.Equal("060000", doc.Root?.Attribute("Version")?.Value);
    }

    [Fact]
    public void Generate_ServerName_DerivedFromIdoName()
    {
        var ido = BuildCNHLeadTime();
        var xml = IdoXmlGenerator.Generate(ido);
        var doc = XDocument.Parse(xml);

        var serverName = doc.Descendants("ServerName").First().Value;
        Assert.Equal("CNH", serverName);
    }

    [Fact]
    public void Generate_ServerName_NoUnderscore_UsesFullName()
    {
        var ido = new IdoDefinition { Name = "MyIDO", RevisionNo = 1 };
        var xml = IdoXmlGenerator.Generate(ido);
        var doc = XDocument.Parse(xml);
        Assert.Equal("MyIDO", doc.Descendants("ServerName").First().Value);
    }

    [Fact]
    public void Generate_CorrectRevisionNo()
    {
        var xml = IdoXmlGenerator.Generate(BuildCNHLeadTime());
        var doc = XDocument.Parse(xml);
        Assert.Equal("3", doc.Descendants("RevisionNo").First().Value);
    }

    [Fact]
    public void Generate_PrimaryTable_NoJoinElements()
    {
        var xml = IdoXmlGenerator.Generate(BuildCNHLeadTime());
        var doc = XDocument.Parse(xml);

        var table = doc.Descendants("Table").First();
        Assert.Equal("PRIMARY", table.Attribute("Type")?.Value);
        Assert.Null(table.Element("ExplicitJoin"));
        Assert.Null(table.Element("JoinType"));
    }

    [Fact]
    public void Generate_SecondaryTable_HasJoinElements()
    {
        var ido = new IdoDefinition
        {
            Name = "TST_Test",
            RevisionNo = 1,
            Tables =
            [
                new IdoTable { Name = "item", Alias = "itm", Type = TableType.PRIMARY },
                new IdoTable { Name = "prodcode", Alias = "prd", Type = TableType.SECONDARY,
                    ExplicitJoin = "prd.product_code = itm.product_code", JoinType = JoinType.INNER }
            ]
        };

        var xml = IdoXmlGenerator.Generate(ido);
        var doc = XDocument.Parse(xml);

        var secondary = doc.Descendants("Table").Skip(1).First();
        Assert.Equal("SECONDARY", secondary.Attribute("Type")?.Value);
        Assert.Equal("prd.product_code = itm.product_code", secondary.Element("ExplicitJoin")?.Value);
        Assert.Equal("INNER", secondary.Element("JoinType")?.Value);
    }

    [Fact]
    public void Generate_BoundProperty_HasBoundToColumnAndAlias()
    {
        var xml = IdoXmlGenerator.Generate(BuildCNHLeadTime());
        var doc = XDocument.Parse(xml);

        var itemProp = doc.Descendants("Property").First(p => p.Attribute("Name")?.Value == "Item");
        Assert.Equal("BOUND", itemProp.Attribute("Binding")?.Value);
        Assert.Equal("item", itemProp.Element("BoundToColumn")?.Value);
        Assert.Equal("ltu", itemProp.Element("ColumnTableAlias")?.Value);
    }

    [Fact]
    public void Generate_DerivedProperty_HasExpression()
    {
        var ido = new IdoDefinition
        {
            Name = "TST_Der",
            RevisionNo = 1,
            Tables = [new IdoTable { Name = "item", Alias = "itm", Type = TableType.PRIMARY }],
            Properties =
            [
                new IdoProperty
                {
                    Name = "IsSpecial", Sequence = 1,
                    Binding = PropertyBinding.DERIVED,
                    Expression = "CASE WHEN itm.item = 'X' THEN 1 ELSE 0 END",
                    PropertyClass = "Int"
                }
            ]
        };

        var xml = IdoXmlGenerator.Generate(ido);
        var doc = XDocument.Parse(xml);

        var prop = doc.Descendants("Property").First();
        Assert.Equal("DERIVED", prop.Attribute("Binding")?.Value);
        Assert.Contains("CASE WHEN", prop.Element("Expression")?.Value);
        Assert.Null(prop.Element("BoundToColumn"));
    }

    [Fact]
    public void Generate_PropertyAttributes_WhenPresent()
    {
        var xml = IdoXmlGenerator.Generate(BuildCNHLeadTime());
        var doc = XDocument.Parse(xml);

        var itemProp = doc.Descendants("Property").First(p => p.Attribute("Name")?.Value == "Item");
        var attrs = itemProp.Element("PropertyAttributes");
        Assert.NotNull(attrs);
        Assert.Equal("ItemType", attrs.Element("ColumnDataType")?.Value);
        Assert.Equal("30", attrs.Element("DataLength")?.Value);
        Assert.Equal("String", attrs.Element("DataType")?.Value);
        Assert.Equal("1", attrs.Element("IsRequired")?.Value);
    }

    [Fact]
    public void Generate_EmptyPropertyAttributes_WhenNoMetadata()
    {
        var xml = IdoXmlGenerator.Generate(BuildCNHLeadTime());
        var doc = XDocument.Parse(xml);

        var rowPtr = doc.Descendants("Property").First(p => p.Attribute("Name")?.Value == "RowPointer");
        var attrs = rowPtr.Element("PropertyAttributes");
        Assert.NotNull(attrs);
        Assert.Empty(attrs.Elements()); // <PropertyAttributes />
    }

    [Fact]
    public void Generate_MethodsAndRulesAreEmpty()
    {
        var xml = IdoXmlGenerator.Generate(BuildCNHLeadTime());
        var doc = XDocument.Parse(xml);

        Assert.Empty(doc.Descendants("Methods").First().Elements());
        Assert.Empty(doc.Descendants("Rules").First().Elements());
    }
}
