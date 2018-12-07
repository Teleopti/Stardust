using System;
using System.Linq;
using Microsoft.PowerBI.Api.V2;
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

			var reports = createReports(groupId, report);
			var target = createReportProvider(reports, groupId);

			var result = target.GetReports().GetAwaiter().GetResult();
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
			var reports = createReports(groupId);
			var target = createReportProvider(reports, groupId);

			var config = target.GetReportConfig(Guid.NewGuid().ToString()).GetAwaiter().GetResult();
			config.ReportId.Should().Be.NullOrEmpty();
			config.ReportName.Should().Be.NullOrEmpty();
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

			var reports = createReports(groupId, report);
			var target = createReportProvider(reports, groupId);

			var config = target.GetReportConfig(Guid.NewGuid().ToString()).GetAwaiter().GetResult();
			config.ReportId.Should().Be.NullOrEmpty();
			config.ReportName.Should().Be.NullOrEmpty();
			config.ReportUrl.Should().Be.NullOrEmpty();
			config.AccessToken.Should().Be.NullOrEmpty();
			config.TokenType.Should().Be.NullOrEmpty();
		}

		[Test]
		public void ShouldGetReportConfig()
		{
			const string token = "Test access token for report";
			var groupId = Guid.NewGuid().ToString();
			var report = new Report
			{
				Id = Guid.NewGuid().ToString(),
				Name = "Test report",
				EmbedUrl = "https://someworkspace/somereport"
			};

			var reports = createReports(groupId, report);
			reports.SetAccessToken(token);

			var target = createReportProvider(reports, groupId);

			var config = target.GetReportConfig(report.Id).GetAwaiter().GetResult();
			config.ReportId.Should().Be(report.Id);
			config.ReportName.Should().Be(report.Name);
			config.AccessToken.Should().Be(token);
		}

		[Test]
		public void ShouldCloneExistingReport()
		{
			const string token = "Test access token for report clone";
			var groupId = Guid.NewGuid().ToString();
			var report = new Report
			{
				Id = Guid.NewGuid().ToString(),
				Name = "Test report",
				EmbedUrl = "https://someworkspace/somereport"
			};

			var reports = createReports(groupId, report);
			reports.SetAccessToken(token);

			var target = createReportProvider(reports, groupId);

			const string newReportName = "New cloned report";
			var config = target.CloneReport(report.Id, newReportName).GetAwaiter().GetResult();
			config.ReportId.Should().Not.Be.Null();
			config.ReportName.Should().Be(newReportName);
			config.AccessToken.Should().Be(token);

			var reportGroups = reports.ReportGroups;
			reportGroups.ContainsKey(groupId).Should().Be.True();
			reportGroups[groupId].Count.Should().Be(2);

			reportGroups[groupId].Any(x=>x.Id == report.Id).Should().Be.True();
			reportGroups[groupId].Any(x=>x.Id == config.ReportId && x.Name == newReportName).Should().Be.True();
		}

		private FakeReports createReports(string groupId, params Report[] powerBiReports)
		{
			var reports = new FakeReports();
			reports.AddReports(groupId, powerBiReports);
			return reports;
		}

		private ReportProvider createReportProvider(IReports reports, string groupId)
		{
			var pbiClient = new FakePowerBiClient(reports);
			var pbiClientFactory = new FakePowerBiClientFactory(pbiClient);

			var configReader = new FakeConfigReader();
			configReader.FakeSetting("PowerBIGroupId", groupId);

			return new ReportProvider(configReader, pbiClientFactory);
		}
	}
}