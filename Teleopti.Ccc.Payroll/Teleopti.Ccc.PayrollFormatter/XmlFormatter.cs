using System.IO;
using System.Xml.XPath;

namespace Teleopti.Ccc.PayrollFormatter
{
    public class XmlFormatter : PayrollFormatterBase
    {
        public override Stream Format(IXPathNavigable navigable, DocumentFormat documentFormat)
        {
            byte[] rawData = documentFormat.Encoding.GetBytes(navigable.CreateNavigator().OuterXml);
            MemoryStream memoryStream = new MemoryStream(rawData);
            return memoryStream;
        }

        public override string FileSuffix
        {
            get
            {
                return "xml";
            }
        }
    }
}
