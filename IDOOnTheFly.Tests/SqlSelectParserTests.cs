using IDOOnTheFly.BL.Models;
using IDOOnTheFly.BL.Parsing;
using Xunit;

namespace IDOOnTheFly.Tests;

public class SqlSelectParserTests
{
    [Fact]
    public void Parse_SingleTable_ExtractsPrimaryTable()
    {
        var sql = "SELECT ltu.item AS Item FROM CNH_LeadTimeUpdate ltu";
        var result = SqlSelectParser.Parse(sql);

        Assert.Single(result.Tables);
        Assert.Equal("CNH_LeadTimeUpdate", result.Tables[0].Name);
        Assert.Equal("ltu", result.Tables[0].Alias);
        Assert.Equal(TableType.PRIMARY, result.Tables[0].Type);
    }

    [Fact]
    public void Parse_SingleTable_ExtractsPrimaryTable2()
    {
        var sql = "SELECT ltu.item FROM CNH_LeadTimeUpdate";
        var result = SqlSelectParser.Parse(sql);

        Assert.Single(result.Tables);
        Assert.Equal("CNH_LeadTimeUpdate", result.Tables[0].Name);
    }

    [Fact]
    public void Parse_InnerJoin_ExtractsSecondaryTable()
    {
        var sql = @"
            SELECT itm.item AS Item, prd.product_code AS ProductCode
            FROM item itm
            INNER JOIN prodcode prd ON prd.product_code = itm.product_code";

        var result = SqlSelectParser.Parse(sql);

        Assert.Equal(2, result.Tables.Count);
        Assert.Equal(TableType.PRIMARY, result.Tables[0].Type);
        Assert.Equal(TableType.SECONDARY, result.Tables[1].Type);
        Assert.Equal("prodcode", result.Tables[1].Name);
        Assert.Equal("prd", result.Tables[1].Alias);
        Assert.Equal(JoinType.INNER, result.Tables[1].JoinType);
        Assert.Contains("prd.product_code = itm.product_code", result.Tables[1].ExplicitJoin);
    }

    [Fact]
    public void Parse_LeftJoin_ExtractsCorrectJoinType()
    {
        var sql = @"
            SELECT a.item AS Item
            FROM item a
            LEFT JOIN itemcust b ON b.item = a.item";

        var result = SqlSelectParser.Parse(sql);

        Assert.Equal(JoinType.LEFT, result.Tables[1].JoinType);
    }

    [Fact]
    public void Parse_BoundProperty_AliasAndColumn()
    {
        var sql = "SELECT ltu.item AS Item FROM CNH_LeadTimeUpdate ltu";
        var result = SqlSelectParser.Parse(sql);

        Assert.Single(result.Properties);
        var prop = result.Properties[0];
        Assert.Equal("Item", prop.Name);
        Assert.Equal(PropertyBinding.BOUND, prop.Binding);
        Assert.Equal("item", prop.BoundToColumn);
        Assert.Equal("ltu", prop.ColumnTableAlias);
    }

    [Fact]
    public void Parse_DerivedProperty_CaseExpression()
    {
        var sql = "SELECT CASE WHEN ltu.item = 'X' THEN 1 ELSE 0 END AS IsSpecial FROM CNH_LeadTimeUpdate ltu";
        var result = SqlSelectParser.Parse(sql);

        Assert.Single(result.Properties);
        var prop = result.Properties[0];
        Assert.Equal("IsSpecial", prop.Name);
        Assert.Equal(PropertyBinding.DERIVED, prop.Binding);
        Assert.Contains("CASE WHEN", prop.Expression);
    }

    [Fact]
    public void Parse_MultipleProperties_CorrectSequences()
    {
        var sql = @"
            SELECT ltu.item AS Item,
                   ltu.lead_time AS LeadTime,
                   ltu.vend_num AS VendNum
            FROM CNH_LeadTimeUpdate ltu";

        var result = SqlSelectParser.Parse(sql);

        Assert.Equal(3, result.Properties.Count);
        for (int i = 0; i < result.Properties.Count; i++)
            Assert.Equal(i + 1, result.Properties[i].Sequence);
    }

    [Fact]
    public void Parse_NoAlias_PascalCasesFromSnake()
    {
        var sql = "SELECT ltu.lead_time FROM CNH_LeadTimeUpdate ltu";
        var result = SqlSelectParser.Parse(sql);

        Assert.Single(result.Properties);
        Assert.Equal("LeadTime", result.Properties[0].Name);
    }

    [Fact]
    public void Parse_FunctionExpression_IsDerived()
    {
        var sql = "SELECT UPPER(ltu.item) AS UpperItem FROM CNH_LeadTimeUpdate ltu";
        var result = SqlSelectParser.Parse(sql);

        Assert.Equal(PropertyBinding.DERIVED, result.Properties[0].Binding);
        Assert.Equal("UpperItem", result.Properties[0].Name);
    }

    [Fact]
    public void Parse_AsKeyword_TableAliasExtracted()
    {
        var sql = "SELECT itm.item AS Item FROM item AS itm";
        var result = SqlSelectParser.Parse(sql);

        Assert.Equal("itm", result.Tables[0].Alias);
        Assert.Equal("item", result.Tables[0].Name);
    }
}
