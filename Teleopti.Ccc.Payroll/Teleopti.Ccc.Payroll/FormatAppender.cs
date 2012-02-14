using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Teleopti.Ccc.Payroll
{
    public static class FormatAppender
    {
        private static readonly Type thisType = typeof(FormatAppender);
        private static readonly Assembly assembly = thisType.Assembly;

        public static IXPathNavigable AppendFormat(IXPathNavigable document, string fileName)
        {
            XmlDocument formatDoc = new XmlDocument();
            Stream stream =
                assembly.GetManifestResourceStream(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                                                 "{0}.{1}", thisType.Namespace, fileName));
            if (stream == null) return document;

            formatDoc.Load(stream);
            return AppendFormat(document, formatDoc);
        }

        public static IXPathNavigable AppendFormat(IXPathNavigable document, IXPathNavigable formatDoc)
        {
            var navigator = document.CreateNavigator();
            var formatNavigator = formatDoc.CreateNavigator();
            if (navigator.HasChildren)
            {
                Stream schemaStream =
                    assembly.GetManifestResourceStream(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                                                     "{0}.payroll-format.xsd", thisType.Namespace));
                if (schemaStream == null) throw new InvalidOperationException("The Xml schema for payroll formats could not be loaded.");
                XmlSchema schema =
                    XmlSchema.Read(schemaStream, (sender, e) => { throw e.Exception; });

                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.Add(schema);
                formatNavigator.CheckValidity(schemaSet, (sender, e) => { throw e.Exception; });

                if (formatNavigator.HasChildren)
                {
                    XmlDocument documentFormat = new XmlDocument();
                    documentFormat.Load(formatNavigator.ReadSubtree());

                    XmlDocument documentContent = new XmlDocument();
                    documentContent.Load(navigator.ReadSubtree());

                    if (documentFormat.DocumentElement != null && 
                        documentContent.DocumentElement !=null)
                    {
                        var node = documentContent.ImportNode(documentFormat.DocumentElement, true);
                        documentContent.DocumentElement.AppendChild(node);
                    }

                    return documentContent;
                }
            }
            return document;
        }
    }
}
