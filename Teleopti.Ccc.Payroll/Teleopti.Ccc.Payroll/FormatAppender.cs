using System;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Teleopti.Ccc.Payroll
{
    public static class FormatAppender
    {
        private static readonly Type ThisType = typeof(FormatAppender);
        private static readonly Assembly Assembly = ThisType.Assembly;

        public static IXPathNavigable AppendFormat(IXPathNavigable document, string fileName)
        {
            var formatDoc = new XmlDocument();
            var stream =
                Assembly.GetManifestResourceStream(string.Format(CultureInfo.InvariantCulture,
                                                                 "{0}.{1}", ThisType.Namespace, fileName));
            if (stream == null) 
				return document;

            formatDoc.Load(stream);
            return AppendFormat(document, formatDoc);
        }

        public static IXPathNavigable AppendFormat(IXPathNavigable document, IXPathNavigable formatDoc)
        {
            var navigator = document.CreateNavigator();
            var formatNavigator = formatDoc.CreateNavigator();
            if (navigator.HasChildren)
            {
                var schemaStream =
                    Assembly.GetManifestResourceStream(string.Format(CultureInfo.InvariantCulture,
                                                                     "{0}.payroll-format.xsd", ThisType.Namespace));
                if (schemaStream == null) 
					throw new InvalidOperationException("The Xml schema for payroll formats could not be loaded.");

                var schema =
                    XmlSchema.Read(schemaStream, (sender, e) => { throw e.Exception; });

                var schemaSet = new XmlSchemaSet();
                schemaSet.Add(schema);
                formatNavigator.CheckValidity(schemaSet, (sender, e) => { throw e.Exception; });

                if (formatNavigator.HasChildren)
                {
                    var documentFormat = new XmlDocument();
                    documentFormat.Load(formatNavigator.ReadSubtree());

                    var documentContent = new XmlDocument();
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
