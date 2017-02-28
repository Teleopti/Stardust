﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.Reports.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Reports.Core
{
	[TestFixture]
	public class ReportNavigationProviderTest
	{
		private FakeReportProvider _reportProvider;
		private IReportUrl _reportUrl;
		private ConfigurablePermissions _authorizor;
		private IReportNavigationProvider target;

		[SetUp]
		public void SetUp()
		{
			_reportUrl = new FakeReportUrl();
			_reportProvider = new FakeReportProvider();
			_authorizor = new ConfigurablePermissions();
			target = new ReportNavigationProvider(_reportProvider, _reportUrl, _authorizor, new TrueToggleManager());
		}

		[Test]
		public void ShouldGetNavigationItemsWhenHasPermission()
		{
			var report = MockRepository.GenerateMock<IApplicationFunction>();
			report.Stub(x => x.LocalizedFunctionDescription).Return("report1");
			report.Stub(x => x.ForeignId).Return("report1");
			_reportProvider.PermitReport(report);

			var result = target.GetNavigationItems();

			result.Should().Not.Be.Null();
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetNoReportItemsWhenHasNoPermission()
		{
			var report = MockRepository.GenerateMock<IApplicationFunction>();
			report.Stub(x => x.LocalizedFunctionDescription).Return("report1");
			report.Stub(x => x.ForeignId).Return("report1");

			var result = target.GetNavigationItems();

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

			var result = target.GetNavigationItems();

			result.Count.Should().Be.EqualTo(1);
			result.SingleOrDefault().Url.Should().Be.EqualTo("Selection.aspx?ReportId=report1&BuId=00000001");
			result.SingleOrDefault().Name.Should().Be.EqualTo("report1");
		}

		[Test]
		public void ShouldGetBadgeLeaderboardReportItemWhenHasPermission()
		{
			_authorizor.HasPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboardUnderReports);

			var result = target.GetNavigationItems();

			result.Count.Should().Be.EqualTo(1);
			result.SingleOrDefault().Name.Should().Be.EqualTo(Resources.BadgeLeaderBoardReport);
		}
		[Test]
		public void ShouldNotGetBadgeLeaderboardReportItemWhenHasNoPermission()
		{
			var result = target.GetNavigationItems();

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
		private string matrixUrl = "Selection.aspx?ReportId={0}&BuId=00000001";
		public string Build(string reportId)
		{
			return string.Format(matrixUrl, reportId);
		}
	}
}