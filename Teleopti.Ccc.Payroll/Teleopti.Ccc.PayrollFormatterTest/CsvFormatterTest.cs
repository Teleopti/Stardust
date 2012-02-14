using System.IO;
using System.Xml;
using NUnit.Framework;
using Teleopti.Ccc.PayrollFormatter;

namespace Teleopti.Ccc.PayrollFormatterTest
{
    [TestFixture]
    public class CsvFormatterTest
    {
        private FlatFileFormatter target;

        [SetUp]
        public void Setup()
        {
            target = new FlatFileFormatter();
        }

        [Test]
        public void VerifyBasicCsvExport()
        {
            XmlDocument document = new XmlDocument();
            document.Load("BasicCsvExport.xml");

            DocumentFormat format = DocumentFormat.LoadFromXml(document);
            Stream result = target.Format(document, format);
            result.Position = 0;

            StreamReader streamReader = new StreamReader(result, format.Encoding);
            string content = streamReader.ReadToEnd();

            Assert.AreEqual(File.ReadAllText("BasicCsvExportResult.txt").Replace("\r",""),content);
            Assert.AreEqual("txt",target.FileSuffix);
        }
    }
}
