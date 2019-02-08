using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Insights;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.Insights.Models;
using Teleopti.Ccc.WebTest.Areas.Insights.Core.PowerBi;

namespace Teleopti.Ccc.WebTest.Areas.Insights.Core
{
	[TestFixture]
	public class ReportProviderTest
	{
		private const string overrideReportName = "Override report name";
		private IInsightsReportRepository insightsReportRepository;
		private IPerson person;

		[SetUp]
		public void Setup()
		{
			insightsReportRepository = new FakeInsightsReportRepository();
			person = PersonFactory.CreatePerson("Ashley", "Andeen");
			person.SetId(Guid.NewGuid());
		}

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

			var reports = createReports(groupId, DateTime.Now, report);
			var target = createReportProvider(reports, groupId);

			var result = target.GetReports().GetAwaiter().GetResult();
			result.Length.Should().Be(1);
			var reportModel = result.Single();
			reportModel.Id.Should().Be(report.Id);
			reportModel.Name.Should().Be(report.Name);
			reportModel.EmbedUrl.Should().Be(report.EmbedUrl);
		}

		[Test]
		public void ShouldOverrideReportMetadata()
		{
			var groupId = Guid.NewGuid().ToString();
			var reportId = Guid.NewGuid();
			var report = new Report
			{
				Id = reportId.ToString(),
				Name = "Test report",
				EmbedUrl = "https://someworkspace/somereport"
			};

			var now = DateTime.Now;
			var reports = createReports(groupId, now, report);
			updateReportMetadata(reportId, overrideReportName);

			var target = createReportProvider(reports, groupId);
			var result = target.GetReports().GetAwaiter().GetResult();
			result.Length.Should().Be(1);
			var reportModel = result.Single();
			reportModel.Id.Should().Be(report.Id);
			reportModel.Name.Should().Be(overrideReportName);
			reportModel.EmbedUrl.Should().Be(report.EmbedUrl);
			reportModel.CreatedBy.Should().Be($"{person.Name.FirstName}@{person.Name.LastName}");
			reportModel.CreatedOn.Should().Be(now);
		}

		[Test]
		public void ShouldGetReportsExcludeUsageReportAndTemplateReport()
		{
			var groupId = Guid.NewGuid().ToString();
			var report = new Report
			{
				Id = Guid.NewGuid().ToString(),
				Name = "Test report",
				EmbedUrl = "https://someworkspace/somereport"
			};
			var usageReport = new Report
			{
				Id = Guid.NewGuid().ToString(),
				Name = ReportProvider.UsageReportName,
				EmbedUrl = "https://someworkspace/stasticsreport"
			};
			var templateReport = new Report
			{
				Id = Guid.NewGuid().ToString(),
				Name = ReportProvider.TemplateReportName,
				EmbedUrl = "https://someworkspace/templatereport"
			};

			var reports = createReports(groupId, DateTime.Now, report, usageReport, templateReport);

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
			var reports = createReports(groupId, DateTime.Now);
			var target = createReportProvider(reports, groupId);

			var config = target.GetReportConfig(Guid.NewGuid()).GetAwaiter().GetResult();
			reportConfigShouldBeEmpty(config);
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

			var reports = createReports(groupId, DateTime.Now, report);

			var target = createReportProvider(reports, groupId);
			var config = target.GetReportConfig(Guid.NewGuid()).GetAwaiter().GetResult();
			reportConfigShouldBeEmpty(config);
		}

		[TestCase(10)]
		[TestCase(-5)]
		public void ShouldGetReportConfig(int tokenExpirationTimeSpanInMinutes)
		{
			const string token = "Test access token for report";
			var groupId = Guid.NewGuid().ToString();
			var reportId = Guid.NewGuid();
			var report = new Report
			{
				Id = reportId.ToString(),
				Name = "Test report",
				EmbedUrl = "https://someworkspace/somereport"
			};

			var now = DateTime.Now;
			var reports = createReports(groupId, now, report);
			reports.SetAccessToken(token);
			reports.SetTokenExpiration(now.AddMinutes(tokenExpirationTimeSpanInMinutes));

			var target = createReportProvider(reports, groupId);
			var config = target.GetReportConfig(reportId).GetAwaiter().GetResult();
			config.ReportId.Should().Be(report.Id);
			config.ReportName.Should().Be(report.Name);
			config.AccessToken.Should().Be(token);
			(config.Expiration.Value - now).TotalMinutes.Should().Be.GreaterThan(9);
		}

		[TestCase(10)]
		[TestCase(-5)]
		public void ShouldGetReportConfigWithMetadataOverride(int tokenExpirationTimeSpanInMinutes)
		{
			const string token = "Test access token for report";
			var groupId = Guid.NewGuid().ToString();
			var reportId = Guid.NewGuid();
			var report = new Report
			{
				Id = reportId.ToString(),
				Name = "Test report",
				EmbedUrl = "https://someworkspace/somereport"
			};

			var now = DateTime.Now;
			var reports = createReports(groupId, now, report);
			reports.SetAccessToken(token);
			reports.SetTokenExpiration(DateTime.Now.AddMinutes(tokenExpirationTimeSpanInMinutes));

			updateReportMetadata(reportId, overrideReportName);

			var target = createReportProvider(reports, groupId);
			var config = target.GetReportConfig(reportId).GetAwaiter().GetResult();
			config.ReportId.Should().Be(report.Id);
			config.ReportName.Should().Be(overrideReportName);
			config.AccessToken.Should().Be(token);
			(config.Expiration.Value - DateTime.Now).TotalMinutes.Should().Be.GreaterThan(9);
			config.CreatedBy.Should().Be($"{person.Name.FirstName}@{person.Name.LastName}");
			config.CreatedOn.Should().Be(now);
		}

		[Test]
		public void ShouldCloneExistingReport()
		{
			const string token = "Test access token for report clone";
			var groupId = Guid.NewGuid().ToString();
			var reportId = Guid.NewGuid();
			var report = new Report
			{
				Id = reportId.ToString(),
				Name = "Test report",
				EmbedUrl = "https://someworkspace/somereport"
			};

			var reports = createReports(groupId, DateTime.Now, report);
			reports.SetAccessToken(token);

			var target = createReportProvider(reports, groupId);

			const string newReportName = "New cloned report";
			var config = target.CloneReport(reportId, newReportName).GetAwaiter().GetResult();
			config.ReportId.Should().Not.Be.Null();
			config.ReportName.Should().Be(newReportName);
			config.AccessToken.Should().Be(token);

			var reportGroups = reports.ReportGroups;
			reportGroups.ContainsKey(groupId).Should().Be.True();
			reportGroups[groupId].Count.Should().Be(2);

			reportGroups[groupId].Any(x => x.Id == report.Id).Should().Be.True();
			reportGroups[groupId].Any(x => x.Id == config.ReportId && x.Name == newReportName).Should().Be.True();

			insightsReportRepository.LoadAll()
				.Single(x => x.Id.ToString() == config.ReportId && x.Name == newReportName)
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldCreateNewReportFromTemplate()
		{
			const string token = "Test access token for report clone";
			var groupId = Guid.NewGuid().ToString();
			var reportId = Guid.NewGuid();
			var report = new Report
			{
				Id = reportId.ToString(),
				Name = ReportProvider.TemplateReportName,
				EmbedUrl = "https://someworkspace/templatereport"
			};

			var reports = createReports(groupId, DateTime.Now, report);
			reports.SetAccessToken(token);

			var target = createReportProvider(reports, groupId);

			const string newReportName = "New report test";
			var config = target.CreateReport(newReportName).GetAwaiter().GetResult();
			config.ReportId.Should().Not.Be.Null();
			config.ReportName.Should().Be(newReportName);
			config.AccessToken.Should().Be(token);

			var reportGroups = reports.ReportGroups;
			reportGroups.ContainsKey(groupId).Should().Be.True();
			reportGroups[groupId].Count.Should().Be(2);

			reportGroups[groupId].Any(x => x.Id == report.Id).Should().Be.True();
			reportGroups[groupId].Any(x => x.Id == config.ReportId && x.Name == newReportName).Should().Be.True();

			insightsReportRepository.LoadAll()
				.SingleOrDefault(x => x.Id.ToString() == config.ReportId && x.Name == newReportName)
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotCreateNewReportIfTemplateNotExists()
		{
			const string token = "Test access token for report clone";
			var groupId = Guid.NewGuid().ToString();
			var reports = createReports(groupId, DateTime.Now);
			reports.SetAccessToken(token);

			var target = createReportProvider(reports, groupId);

			const string newReportName = "New report test - copy";
			var config = target.CreateReport(newReportName).GetAwaiter().GetResult();
			config.ReportId.Should().Be.Null();
			config.ReportName.Should().Be.Null();
			config.AccessToken.Should().Be.Null();

			insightsReportRepository.LoadAll().SingleOrDefault(x => x.Name == newReportName)
				.Should().Be.Null();
		}

		[Test]
		public void ShouldDeleteExistingReport()
		{
			var groupId = Guid.NewGuid().ToString();
			var reportId = Guid.NewGuid();
			var report = new Report
			{
				Id = reportId.ToString(),
				Name = "Test report",
				EmbedUrl = "https://someworkspace/templatereport"
			};

			var reports = createReports(groupId, DateTime.Now, report);

			var target = createReportProvider(reports, groupId);
			var result = target.DeleteReport(reportId).GetAwaiter().GetResult();
			result.Should().Be.True();

			var reportList = target.GetReports().GetAwaiter().GetResult();
			reportList.Length.Should().Be(0);
		}

		[Test]
		public void ShouldFailedOnDeleteUnExistingReport()
		{
			var groupId = Guid.NewGuid().ToString();
			var reportId = Guid.NewGuid();
			var report = new Report
			{
				Id = reportId.ToString(),
				Name = "Test report",
				EmbedUrl = "https://someworkspace/templatereport"
			};

			var reports = createReports(groupId, DateTime.Now, report);

			var target = createReportProvider(reports, groupId);
			var result = target.DeleteReport(Guid.NewGuid()).GetAwaiter().GetResult();
			result.Should().Be.False();

			var reportList = target.GetReports().GetAwaiter().GetResult();
			reportList.Length.Should().Be(1);
			var reportModel = reportList.Single();
			reportModel.Id.Should().Be(report.Id);
			reportModel.Name.Should().Be(report.Name);
			reportModel.EmbedUrl.Should().Be(report.EmbedUrl);
		}

		private void createReportMetadata(Guid reportId, string reportName, DateTime now)
		{
			var insightsReport = new InsightsReport
			{
				Name = reportName,
				CreatedBy = person,
				CreatedOn = now
			};
			insightsReport.SetId(reportId);
			insightsReportRepository.Add(insightsReport);
		}

		private void updateReportMetadata(Guid reportId, string overrideReportName)
		{
			var report = insightsReportRepository.Get(reportId);
			report.Name = overrideReportName;
		}

		private static void reportConfigShouldBeEmpty(EmbedReportConfig config)
		{
			config.ReportId.Should().Be.NullOrEmpty();
			config.ReportName.Should().Be.NullOrEmpty();
			config.ReportUrl.Should().Be.NullOrEmpty();
			config.AccessToken.Should().Be.NullOrEmpty();
			config.TokenType.Should().Be.NullOrEmpty();
			config.CreatedBy.Should().Be.NullOrEmpty();
			config.CreatedOn.HasValue.Should().Be.False();
		}

		private FakeReports createReports(string groupId, DateTime currentTime, params Report[] powerBiReports)
		{
			var reports = new FakeReports();
			reports.AddReports(groupId, powerBiReports);

			foreach (var report in powerBiReports)
			{
				createReportMetadata(Guid.Parse(report.Id), report.Name, currentTime);
			}

			return reports;
		}

		private ReportProvider createReportProvider(IReports reports, string groupId)
		{
			var pbiClient = new FakePowerBiClient(reports);
			var pbiClientFactory = new FakePowerBiClientFactory(pbiClient);

			var appConfigDb = new ApplicationConfigurationDb
			{
				Tenant = new Dictionary<TenantApplicationConfigKey, string>
				{
					{
						TenantApplicationConfigKey.InsightsPowerBIGroupId, groupId
					}
				}
			};

			var appConfigProvider = new ApplicationConfigurationDbProviderFake();
			appConfigProvider.LoadFakeData(appConfigDb);

			return new ReportProvider(appConfigProvider, new FakeCurrentUnitOfWorkFactory(new FakeStorage()),
				pbiClientFactory, insightsReportRepository, new FakeCommonAgentNameProvider());
		}
	}
}