using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.Reports.Core;
using Teleopti.Ccc.Web.Areas.Reports.Models;

namespace Teleopti.Ccc.WebTest.Areas.Reports.Core
{
	[TestFixture]
	public class ReportNavigationProviderTest
	{
		private FakeReportProvider _reportProvider;
		private IReportUrl _reportUrl;
		private FakePermissions _authorizor;
		private IReportNavigationProvider target;
		private ReportNavigationModel _reportNavigationModel;
		private MockRepository _mocks;
		private IToggleManager _toggleManager;

		[SetUp]
		public void SetUp()
		{
			_reportUrl = new FakeReportUrl();
			_reportProvider = new FakeReportProvider();
			_authorizor = new FakePermissions();
			_reportNavigationModel = new ReportNavigationModel(new List<IReportVisible>(),
				new ScheduleAnalysisAuditTrailProvider());
			_mocks = new MockRepository();

			_toggleManager = new FakeToggleManager();
			
			target = new ReportNavigationProvider(_reportProvider, _reportUrl, _authorizor,
				_reportNavigationModel, _toggleManager);
		}

		[Test]
		public void ShouldGetNavigationItemsWhenHasPermission()
		{
			var report = MockRepository.GenerateMock<IApplicationFunction>();
			report.Stub(x => x.LocalizedFunctionDescription).Return("report1");
			report.Stub(x => x.ForeignId).Return("report1");
			_reportProvider.PermitReport(report);

			var result = target.GetNavigationItemViewModels();

			result.Should().Not.Be.Null();
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetNoReportItemsWhenHasNoPermission()
		{
			var report = MockRepository.GenerateMock<IApplicationFunction>();
			report.Stub(x => x.LocalizedFunctionDescription).Return("report1");
			report.Stub(x => x.ForeignId).Return("report1");

			var result = target.GetNavigationItemViewModels();

			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotGetSpecifyReportItemWhenHasNoPermissionOfTheReport()
		{
			var report = MockRepository.GenerateMock<IApplicationFunction>();
			report.Stub(x => x.LocalizedFunctionDescription).Return("report1");
			report.Stub(x => x.ForeignId).Return("report1");

			var report2 = MockRepository.GenerateMock<IApplicationFunction>();
			report2.Stub(x => x.LocalizedFunctionDescription).Return("report2");
			report2.Stub(x => x.ForeignId).Return("report2");

			_reportProvider.PermitReport(report);

			var result = target.GetNavigationItemViewModels();

			result.Count.Should().Be.EqualTo(1);

			var reportItemVm = result.Single();
			reportItemVm.Should().Not.Be.Null();
			reportItemVm.Url.Should().Be.EqualTo("Selection.aspx?ReportId=report1&BuId=00000001");
			reportItemVm.Name.Should().Be.EqualTo("report1");
		}

		[Test]
		public void ShouldGetBadgeLeaderboardReportItemWhenHasPermission()
		{
			_authorizor.HasPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboardUnderReports);

			var result = target.GetNavigationItemViewModels();

			result.Count.Should().Be.EqualTo(1);
			var reportItemVm = result.Single();
			reportItemVm.Should().Not.Be.Null();
			reportItemVm.Name.Should().Be.EqualTo(Resources.BadgeLeaderBoardReport);
		}

		[Test]
		public void ShouldNotGetBadgeLeaderboardReportItemWhenHasNoPermission()
		{
			var result = target.GetNavigationItemViewModels();

			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetCategorizedReportItems()
		{
			var auth = _mocks.StrictMock<IAuthorization>();

			var analysisReport = MockRepository.GenerateMock<IApplicationFunction>();
			analysisReport.Stub(x => x.ForeignId).Return("132E3AF2-3557-4EA7-813E-05CD4869D5DB");
			analysisReport.Stub(x => x.ForeignSource).Return(DefinedForeignSourceNames.SourceMatrix);

			using (_mocks.Record())
			{
				Expect.Call(auth.GrantedFunctions())
					.IgnoreArguments()
					.Return(new List<IApplicationFunction>()
					{
						analysisReport
					}).Repeat.Any();
			}

			IList<CategorizedReportItemViewModel> result;

			using (_mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(auth))
				{
					result = target.GetCategorizedNavigationsItemViewModels();
				}
			}

			result.Count.Should().Be.EqualTo(1);
			result.First().Category.Should().Be.EqualTo(Resources.ScheduleAnalysis);
		}

		[Test]
		public void ShouldReturnSortedReportsWhenHasReportPermission()
		{
			var reportTitles = new[] {"Report 3", "Report 1", "Report 2"};

			foreach (var title in reportTitles)
			{
				_reportProvider.PermitReport(new ApplicationFunction(title)
				{
					ForeignId = Guid.NewGuid().ToString()
				});
			}

			var result = target.GetNavigationItems();
			result.Count.Should().Be.EqualTo(reportTitles.Length);

			var sortedReportTitles = reportTitles.OrderBy(x => x).ToArray();
			for (var i = 0; i < sortedReportTitles.Length; i++)
			{
				result[i].Title.Should().Be.EqualTo(sortedReportTitles[i]);
			}
		}

		[Test]
		public void ShouldReturnEmptyReportListWithoutReportPermission()
		{
			var result = target.GetNavigationItems();
			result.Should().Not.Be.Null();
			result.Count.Should().Be.EqualTo(0);
		}
	}

	public class FakeReportProvider : IReportsProvider
	{
		private readonly IList<IApplicationFunction> permittedReports = new List<IApplicationFunction>();

		public void PermitReport(IApplicationFunction report)
		{
			permittedReports.Add(report);
		}

		public IEnumerable<IApplicationFunction> GetReports()
		{
			return permittedReports;
		}
	}

	public class FakeReportUrl : IReportUrl
	{
		private const string matrixUrl = "Selection.aspx?ReportId={0}&BuId=00000001";

		public string Build(IApplicationFunction applicationFunction)
		{
			return string.Format(matrixUrl, applicationFunction.ForeignId);
		}
	}
}