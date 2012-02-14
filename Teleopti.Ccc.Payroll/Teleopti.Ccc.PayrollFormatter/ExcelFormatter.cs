using System.IO;
using System.Text;
using System.Xml.XPath;

namespace Teleopti.Ccc.PayrollFormatter
{
    public class ExcelFormatter : PayrollFormatterBase
    {
        private const string workbook = "<?xml version=\"1.0\"?>\n" +
                                        "<?mso-application progid=\"Excel.Sheet\"?>\n" +
                                        "<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"\n" +
                                        "xmlns:o=\"urn:schemas-microsoft-com:office:office\"\n" +
                                        "xmlns:x=\"urn:schemas-microsoft-com:office:excel\"\n" +
                                        "xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"\n" +
                                        "xmlns:html=\"http://www.w3.org/TR/REC-html40\">\n" +
                                        "<Worksheet ss:Name=\"{0}\">\n" +
                                        "<Table x:FullColumns=\"1\" x:FullRows=\"1\">\n" +
                                        "{1}\n" +
                                        "</Table>\n" +
                                        "</Worksheet>" +
                                        "</Workbook>";

        #region Overrides of PayrollFormatterBase

        public override Stream Format(IXPathNavigable navigable, DocumentFormat documentFormat)
        {
            var navigator = navigable.CreateNavigator();
            var documentElement = navigator.SelectSingleNode("//" + documentFormat.Element);

            StringBuilder rows = new StringBuilder();
            if (documentFormat.HeaderEnabled)
            {
                rows.Append("<Row>\n");
                foreach (var item in documentFormat.Items)
                {
                    rows.AppendFormat("<Cell><Data ss:Type=\"String\">{0}</Data></Cell>\n", item.Element);
                }
                rows.Append("</Row>\n");
            }

            var itemElements = documentElement.SelectDescendants(documentFormat.ItemsElement, string.Empty, false);
            while (itemElements.MoveNext())
            {
                rows.Append("<Row>\n");
                foreach (var item in documentFormat.Items)
                {
                    var itemNode = itemElements.Current.SelectSingleNode("./" + item.Element);
                    string formattedNodeValue = FormatNodeValue(item, itemNode == null ? string.Empty : itemNode.Value);
                    rows.AppendFormat("<Cell><Data ss:Type=\"String\">{0}</Data></Cell>\n", formattedNodeValue);
                }
                rows.Append("</Row>\n");
            }

            var memoryStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(memoryStream, documentFormat.Encoding);
            writer.Write(workbook, documentFormat.Element, rows);
            writer.Flush();

            return memoryStream;
        }

        public override string FileSuffix
        {
            get
            {
                return "xls";
            }
        }

        #endregion
    }
}