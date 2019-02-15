using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.Web.Areas.Reporting.Core;

namespace Teleopti.Ccc.Web.IntegrationTest.Areas.Reporting.Core
{
	[TestFixture]
	public class ReportGeneratorTests
	{
		IDataSource _dataSource;
		ReportGenerator _reportGenerator;

		[SetUp]
		public void Setup()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			_dataSource = DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			_reportGenerator = new ReportGenerator(new testPath());
		}

		[Test]
		public void GeneratingPdf()
		{
			var reportId = Guid.Parse("479809d8-4dae-4852-bf67-c98c3744918d");
			var userId = Guid.NewGuid();
			var buId = Guid.NewGuid();

			var sqlParams = getTestParams();
			var textParams = getTestTextParams();

			var report = _reportGenerator.GenerateReport(reportId, _dataSource.Analytics.ConnectionString, sqlParams,
				textParams, userId, buId, ReportGenerator.ReportFormat.Pdf, TimeZoneInfo.Utc);

			SaveTestFile(report);

			report.MimeType.Should().Be.EqualTo("application/pdf");
			report.Bytes.Count().Should().Be.GreaterThan(0);
			report.Extension.Should().Be.EqualTo("pdf");
		}

		[Test]
		public void GeneratingWord()
		{
			var reportId = Guid.Parse("479809d8-4dae-4852-bf67-c98c3744918d");
			var userId = Guid.NewGuid();
			var buId = Guid.NewGuid();

			var sqlParams = getTestParams();
			var textParams = getTestTextParams();

			var report = _reportGenerator.GenerateReport(reportId, _dataSource.Analytics.ConnectionString, sqlParams,
				textParams, userId, buId, ReportGenerator.ReportFormat.Word, TimeZoneInfo.Utc);

			SaveTestFile(report);

			report.MimeType.Should().Be.EqualTo("application/msword");
			report.Bytes.Count().Should().Be.GreaterThan(0);
			report.Extension.Should().Be.EqualTo("doc");
		}

		[Test]
		public void GeneratingWordOpenXml()
		{
			var reportId = Guid.Parse("479809d8-4dae-4852-bf67-c98c3744918d");
			var userId = Guid.NewGuid();
			var buId = Guid.NewGuid();

			var sqlParams = getTestParams();
			var textParams = getTestTextParams();

			var report = _reportGenerator.GenerateReport(reportId, _dataSource.Analytics.ConnectionString, sqlParams,
				textParams, userId, buId, ReportGenerator.ReportFormat.WordOpenXml, TimeZoneInfo.Utc);

			SaveTestFile(report);

			report.MimeType.Should().Be.EqualTo("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
			report.Bytes.Count().Should().Be.GreaterThan(0);
			report.Extension.Should().Be.EqualTo("docx");
		}

		[Test]
		public void GeneratingExcel()
		{
			var reportId = Guid.Parse("479809d8-4dae-4852-bf67-c98c3744918d");
			var userId = Guid.NewGuid();
			var buId = Guid.NewGuid();

			var sqlParams = getTestParams();
			var textParams = getTestTextParams();

			var report = _reportGenerator.GenerateReport(reportId, _dataSource.Analytics.ConnectionString, sqlParams,
				textParams, userId, buId, ReportGenerator.ReportFormat.Excel, TimeZoneInfo.Utc);

			SaveTestFile(report);

			report.MimeType.Should().Be.EqualTo("application/vnd.ms-excel");
			report.Bytes.Count().Should().Be.GreaterThan(0);
			report.Extension.Should().Be.EqualTo("xls");
		}

		[Test]
		public void GeneratingExcelOpenXml()
		{
			var reportId = Guid.Parse("479809d8-4dae-4852-bf67-c98c3744918d");
			var userId = Guid.NewGuid();
			var buId = Guid.NewGuid();

			var sqlParams = getTestParams();
			var textParams = getTestTextParams();

			var report = _reportGenerator.GenerateReport(reportId, _dataSource.Analytics.ConnectionString, sqlParams,
				textParams, userId, buId, ReportGenerator.ReportFormat.ExcelOpenXml, TimeZoneInfo.Utc);

			SaveTestFile(report);

			report.MimeType.Should().Be.EqualTo("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			report.Bytes.Count().Should().Be.GreaterThan(0);
			report.Extension.Should().Be.EqualTo("xlsx");
		}

		[Test]
		public void GeneratingImage()
		{
			var reportId = Guid.Parse("479809d8-4dae-4852-bf67-c98c3744918d");
			var userId = Guid.NewGuid();
			var buId = Guid.NewGuid();

			var sqlParams = getTestParams();
			var textParams = getTestTextParams();

			var report = _reportGenerator.GenerateReport(reportId, _dataSource.Analytics.ConnectionString, sqlParams,
				textParams, userId, buId, ReportGenerator.ReportFormat.Image, TimeZoneInfo.Utc);

			SaveTestFile(report);

			report.MimeType.Should().Be.EqualTo("image/PNG");
			report.Bytes.Count().Should().Be.GreaterThan(0);
			report.Extension.Should().Be.EqualTo("PNG");
		}

		private void SaveTestFile(GeneratedReport report)
		{
			var uniqueFilename = $"{report.ReportName}_{DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")}.{report.Extension}";

			// Uncomment next line when you want to see the generated files.
			//var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "WFMTestReports"));
			//tempDir.Create();
			//var path = Path.Combine(tempDir.FullName, uniqueFilename);
			//Console.WriteLine($"Saving report to disk at destination '{path}'");
			//using (var fs = new FileStream(path, FileMode.Create))
			//{
			//	fs.Write(report.Bytes, 0, report.Bytes.Length);
			//}
		}

		private List<SqlParameter> getTestParams()
		{
			return new List<SqlParameter>
			{
				new SqlParameter("@date_from", SqlDbType.DateTime) {Value = new DateTime(2013, 05, 02)},
				new SqlParameter("@date_to", SqlDbType.DateTime) {Value = new DateTime(2013, 05, 02)},
				new SqlParameter("@interval_from", SqlDbType.Int) {Value = 0},
				new SqlParameter("@interval_to", SqlDbType.Int) {Value = 0},
				new SqlParameter("@group_page_code", SqlDbType.UniqueIdentifier) {Value = Guid.Empty},
				new SqlParameter("@group_page_group_set", SqlDbType.NVarChar) {Value = ""},
				new SqlParameter("@group_page_agent_set", SqlDbType.NVarChar) {Value = ""},
				new SqlParameter("@site_id", SqlDbType.Int) {Value = -1},
				new SqlParameter("@team_set", SqlDbType.NVarChar) {Value = ""},
				new SqlParameter("@agent_set", SqlDbType.NVarChar) {Value = ""},
				new SqlParameter("@time_zone_id", SqlDbType.Int) {Value = 0},
				new SqlParameter("@details", SqlDbType.Bit) {Value = true}
			};
		}

		private List<string> getTestTextParams()
		{
			return new List<string>
			{
				"05/02/2013",
				"05/02/2013",
				"00:00",
				"00:00",
				"Business Hierarchy",
				"",
				"",
				"All",
				"Not Defined",
				"Not Defined",
				"(UTC+01:00)",
				"True"
			};
		}

		private class testPath : IPathProvider
		{
			public string MapPath(string path)
			{
				return Path.Combine(Paths.WebPath(), "Areas/Reporting/", path.Replace("~/", ""));
			}
		}
	}
}