using System.IO;
using System.Xml;
using NUnit.Framework;
using Teleopti.Ccc.PayrollFormatter;

namespace Teleopti.Ccc.PayrollFormatterTest
{
    [TestFixture]
    public class ExcelFormatterTest
    {
        private ExcelFormatter target;

        [SetUp]
        public void Setup()
        {
            target = new ExcelFormatter();
        }

        [Test]
        public void VerifyInstanceCreated()
        {
            Assert.IsNotNull(target);
        }

        [Test]
        public void VerifyThroughputResult()
        {
            XmlDocument document = new XmlDocument();
            document.Load("BasicExcelExport.xml");

            DocumentFormat format = DocumentFormat.LoadFromXml(document);
            Stream stream = target.Format(document, format);
            stream.Position = 0;

            StreamReader streamReader = new StreamReader(stream, format.Encoding);
            string content = streamReader.ReadToEnd();

            Assert.AreEqual(File.ReadAllText("BasicExcelExportResult.txt").Replace("\r\n","\n"), content);
            Assert.AreEqual("xls", target.FileSuffix);
        }
    }
}
