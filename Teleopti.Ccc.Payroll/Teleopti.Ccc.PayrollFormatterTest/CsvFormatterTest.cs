using System;
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
            target = new FlatFileFormatter(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
        }

        [Test]
        public void VerifyBasicCsvExport()
		{
			var testFilesDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

			var document = new XmlDocument();
            document.Load(Path.Combine(testFilesDir, "BasicCsvExport.xml"));

            var format = DocumentFormat.LoadFromXml(document);
            var result = target.Format(document, format);
            result.Position = 0;

            var streamReader = new StreamReader(result, format.Encoding);
            var content = streamReader.ReadToEnd();
	        var fileContent = File.ReadAllText(Path.Combine(testFilesDir, "BasicCsvExportResult.txt")).Replace("\r", "");

            Assert.AreEqual(fileContent,content);
            Assert.AreEqual("txt",target.FileSuffix);
        }

		[Test]
		public void CsvExportShouldAddInitalCommaWhenEmptyEmployeeNumber()
		{
			var testFilesDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

			var document = new XmlDocument();
			document.Load(Path.Combine(testFilesDir, "TeleoptiTimeExportFormat.xml"));

			var format = DocumentFormat.LoadFromXml(document);
			var result = target.Format(document, format);
			result.Position = 0;

			var streamReader = new StreamReader(result, format.Encoding);
			var content = streamReader.ReadToEnd();

			Assert.AreEqual(content, "123,FirstName,LastName,BusinessUnitName,SiteName,TeamName,ContractName,PartTimePercentageName,75,2009-02-02,2009-02-02 00:00,2009-02-02 00:00,ShiftCategoryName,8:00,8:00,8:00,123,123\n,FirstName,LastName,BusinessUnitName,SiteName,TeamName,ContractName,PartTimePercentageName,75,2009-02-02,2009-02-02 00:00,2009-02-02 00:00,ShiftCategoryName,8:00,8:00,8:00,123,123\n");
			Assert.AreEqual("txt", target.FileSuffix);
		}
	}
}
