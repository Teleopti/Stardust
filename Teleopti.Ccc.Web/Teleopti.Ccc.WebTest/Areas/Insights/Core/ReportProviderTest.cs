using System;
using System.Linq;
using Microsoft.PowerBI.Api.V2.Models;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider;
using Teleopti.Ccc.WebTest.Areas.Insights.Core.PowerBi;

namespace Teleopti.Ccc.WebTest.Areas.Insights.Core
{
	[TestFixture]
	public class ReportProviderTest
	{
		[Test]
		public void ShouldGetReportList()
		{
			var groupId = Guid.NewGuid().ToString();
			var report = new Report
			{
				Id = Guid.NewGuid().ToString(),
				Name = "Test report",
				EmbedUrl = "https://someworkspace/somereport"
			};
			var target = createReportProvider(groupId, "", report);

			var result = target.GetReports().Result;
			result.Length.Should().Be(1);
			var reportModel = result.Single();
			reportModel.Id.Should().Be(report.Id);
			reportModel.Name.Should().Be(report.Name);
			reportModel.EmbedUrl.Should().Be(report.EmbedUrl);
		}

		[Test]
		public void ShouldGetEmptyConfigWhenNoReport()
		{
			var groupId = Guid.NewGuid().ToString();
			var target = createReportProvider(groupId);

			var config = target.GetReportConfig(Guid.NewGuid().ToString()).Result;
			config.ReportId.Should().Be.NullOrEmpty();
			config.ReportUrl.Should().Be.NullOrEmpty();
			config.AccessToken.Should().Be.NullOrEmpty();
			config.TokenType.Should().Be.NullOrEmpty();
		}

		[Test]
		public void ShouldGetEmptyConfigWhenReportNotFound()
		{
			var groupId = Guid.NewGuid().ToString();
			var report = new Report
			{
				Id = Guid.NewGuid().ToString(),
				Name = "Test report",
				EmbedUrl = "https://someworkspace/somereport"
			};
			var target = createReportProvider(groupId, "", report);

			var config = target.GetReportConfig(Guid.NewGuid().ToString()).Result;
			config.ReportId.Should().Be.NullOrEmpty();
			config.ReportUrl.Should().Be.NullOrEmpty();
			config.AccessToken.Should().Be.NullOrEmpty();
			config.TokenType.Should().Be.NullOrEmpty();
		}

		[Test]
		public void ShouldGetReportConfig()
		{
			var token = "Test access token for report";
			var groupId = Guid.NewGuid().ToString();
			var report = new Report
			{
				Id = Guid.NewGuid().ToString(),
				Name = "Test report",
				EmbedUrl = "https://someworkspace/somereport"
			};
			var target = createReportProvider(groupId, token, report);

			var config = target.GetReportConfig(report.Id).Result;
			config.ReportId.Should().Be(report.Id);
			config.AccessToken.Should().Be(token);
		}

		private ReportProvider createReportProvider(string groupId, string token = "", params Report[] newReports)
		{
			var reports = new FakeReports();
			reports.AddReports(groupId, newReports);
			reports.SetAccessToken(token);

			var pbiClient = new FakePowerBiClient(reports);
			var pbiClientFactory = new FakePowerBiClientFactory(pbiClient);

			var configReader = new FakeConfigReader();
			configReader.FakeSetting("PowerBIGroupId", groupId);

			return new ReportProvider(configReader, pbiClientFactory);
		}
	}
}