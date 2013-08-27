using System.IO;
using System.Text;
using System.Xml.XPath;

namespace Teleopti.Ccc.PayrollFormatter
{
    public class FlatFileFormatter : PayrollFormatterBase
    {
        public override Stream Format(IXPathNavigable navigable, DocumentFormat documentFormat)
        {
            var navigator = navigable.CreateNavigator();
            var documentElement = navigator.SelectSingleNode("//" + documentFormat.Element);

            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream, documentFormat.Encoding);

            if (documentFormat.HeaderEnabled)
            {
                var headerElement = documentElement.SelectSingleNode("./" + documentFormat.HeaderElement);
                streamWriter.Write(string.Concat(headerElement.Value, documentFormat.RowSeparator));
            }

            var itemElements = documentElement.SelectDescendants(documentFormat.ItemsElement, string.Empty, false);
            while (itemElements.MoveNext())
            {
                var rowBuilder = new StringBuilder();
                foreach (var item in documentFormat.Items)
                {
                    var itemNode = itemElements.Current.SelectSingleNode("./" + item.Element);
                    if (documentFormat.DocumentFormatType == DocumentFormatType.Csv && rowBuilder.Length > 0)
                        rowBuilder.Append(documentFormat.ItemSeparator);

                    var formattedNodeValue = FormatNodeValue(item, itemNode == null ? string.Empty : itemNode.Value);
                    rowBuilder.Append(formattedNodeValue);
                }

                rowBuilder.Append(documentFormat.RowSeparator);
                streamWriter.Write(rowBuilder.ToString());
            }

            if (documentFormat.FooterEnabled)
            {
                var footerElement = documentElement.SelectSingleNode("./" + documentFormat.FooterElement);
                streamWriter.Write(string.Concat(footerElement.Value, documentFormat.RowSeparator));
            }

            streamWriter.Flush();
            return memoryStream;
   
    }
    }
}