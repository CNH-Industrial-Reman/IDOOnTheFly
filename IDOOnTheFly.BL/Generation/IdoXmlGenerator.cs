using System.Text;
using System.Xml;
using IDOOnTheFly.BL.Models;

namespace IDOOnTheFly.BL.Generation;

public static class IdoXmlGenerator
{
    public static string Generate(IdoDefinition ido, DateTime? revisionDate = null)
    {
        var date = (revisionDate ?? DateTime.UtcNow).ToString("s");

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "   ",
            Encoding = new UTF8Encoding(false),
            OmitXmlDeclaration = false
        };

        using var ms = new MemoryStream();
        using (var writer = XmlWriter.Create(ms, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("ObjectStudioExport");
            writer.WriteAttributeString("Version", "060000");

            writer.WriteStartElement("IDODefinitions");
            writer.WriteStartElement("IDODefinition");
            writer.WriteAttributeString("Name", ido.Name);

            writer.WriteElementString("ServerName", ido.ServerName);
            writer.WriteElementString("RevisionNo", ido.RevisionNo.ToString());
            writer.WriteElementString("RevisionDate", date);
            writer.WriteElementString("ReplaceFlag", "0");
            writer.WriteElementString("QuoteTableAliases", "0");

            // Tables
            writer.WriteStartElement("Tables");
            foreach (var table in ido.Tables)
            {
                writer.WriteStartElement("Table");
                writer.WriteAttributeString("Name", table.Name);
                writer.WriteAttributeString("Alias", table.Alias);
                writer.WriteAttributeString("Type", table.Type.ToString());

                if (table.Type == TableType.SECONDARY)
                {
                    if (!string.IsNullOrEmpty(table.ExplicitJoin))
                        writer.WriteElementString("ExplicitJoin", table.ExplicitJoin);
                    writer.WriteElementString("JoinType", table.JoinType.ToString());
                }

                writer.WriteEndElement(); // Table
            }
            writer.WriteEndElement(); // Tables

            // Methods (always empty)
            writer.WriteStartElement("Methods");
            writer.WriteEndElement();

            // Properties
            writer.WriteStartElement("Properties");
            foreach (var prop in ido.Properties)
            {
                writer.WriteStartElement("Property");
                writer.WriteAttributeString("Name", prop.Name);
                writer.WriteAttributeString("Key", prop.IsKey ? "1" : "0");
                writer.WriteAttributeString("Binding", prop.Binding.ToString());
                writer.WriteAttributeString("Sequence", prop.Sequence.ToString());

                writer.WriteElementString("PropertyClass", prop.PropertyClass);
                writer.WriteElementString("PseudoKeyFlag", "0");

                if (prop.Binding == PropertyBinding.BOUND)
                {
                    writer.WriteElementString("BoundToColumn", prop.BoundToColumn);
                    writer.WriteElementString("ColumnTableAlias", prop.ColumnTableAlias);
                }
                else if (prop.Binding == PropertyBinding.DERIVED)
                {
                    writer.WriteElementString("Expression", prop.Expression);
                }

                // PropertyAttributes
                writer.WriteStartElement("PropertyAttributes");
                if (prop.Attributes != null)
                {
                    if (!string.IsNullOrEmpty(prop.Attributes.ColumnDataType))
                        writer.WriteElementString("ColumnDataType", prop.Attributes.ColumnDataType);
                    if (prop.Attributes.DataLength.HasValue)
                        writer.WriteElementString("DataLength", prop.Attributes.DataLength.Value.ToString());
                    if (!string.IsNullOrEmpty(prop.Attributes.DataType))
                        writer.WriteElementString("DataType", prop.Attributes.DataType);
                    if (prop.Attributes.IsRequired)
                        writer.WriteElementString("IsRequired", "1");
                }
                writer.WriteEndElement(); // PropertyAttributes

                writer.WriteEndElement(); // Property
            }
            writer.WriteEndElement(); // Properties

            writer.WriteStartElement("Rules");
            writer.WriteEndElement();

            writer.WriteEndElement(); // IDODefinition
            writer.WriteEndElement(); // IDODefinitions
            writer.WriteEndElement(); // ObjectStudioExport
        }

        return Encoding.UTF8.GetString(ms.ToArray());
    }
}
