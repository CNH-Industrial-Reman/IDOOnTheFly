using IDOOnTheFly.BL.Schema;
using Xunit;

namespace IDOOnTheFly.Tests;

public class PropertyClassMapperTests
{
    [Theory]
    [InlineData("VARCHAR", "String")]
    [InlineData("NVARCHAR", "String")]
    [InlineData("CHAR", "String")]
    [InlineData("INT", "Int")]
    [InlineData("SMALLINT", "Int")]
    [InlineData("BIGINT", "Int")]
    [InlineData("DECIMAL", "Amount")]
    [InlineData("NUMERIC", "Amount")]
    [InlineData("MONEY", "Amount")]
    [InlineData("DATETIME", "Date")]
    [InlineData("DATETIME2", "Date")]
    [InlineData("BIT", "ListYesNo")]
    [InlineData("FLOAT", "Qty")]
    [InlineData("UNIQUEIDENTIFIER", "Guid")]
    public void GetPropertyClass_KnownTypes_ReturnExpected(string sqlType, string expected)
    {
        Assert.Equal(expected, PropertyClassMapper.GetPropertyClass(sqlType));
    }

    [Fact]
    public void GetPropertyClass_WithLength_StripsLength()
    {
        Assert.Equal("String", PropertyClassMapper.GetPropertyClass("VARCHAR(30)"));
    }

    [Fact]
    public void GetPropertyClass_CaseInsensitive()
    {
        Assert.Equal("String", PropertyClassMapper.GetPropertyClass("varchar"));
        Assert.Equal("Int", PropertyClassMapper.GetPropertyClass("int"));
    }

    [Fact]
    public void GetPropertyClass_Unknown_ReturnsString()
    {
        Assert.Equal("String", PropertyClassMapper.GetPropertyClass("CURSOR"));
    }
}
