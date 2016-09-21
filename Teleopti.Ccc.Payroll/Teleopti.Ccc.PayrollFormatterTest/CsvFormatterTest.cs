using System.IO;
using System.Xml;
using NUnit.Framework;
using Teleopti.Ccc.PayrollFormatter;

namespace Teleopti.Ccc.PayrollFormatterTest
{
    [TestFixture, Parallelizable]
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
            var document = new XmlDocument();
            document.Load(Path.Combine(TestContext.CurrentContext.TestDirectory, "BasicCsvExport.xml"));

            var format = DocumentFormat.LoadFromXml(document);
            var result = target.Format(document, format);
            result.Position = 0;

            var streamReader = new StreamReader(result, format.Encoding);
            var content = streamReader.ReadToEnd();
	        var fileContent = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "BasicCsvExportResult.txt")).Replace("\r", "");

            Assert.AreEqual(fileContent,content);
            Assert.AreEqual("txt",target.FileSuffix);
        }
    }
}
