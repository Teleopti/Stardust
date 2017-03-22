using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;

namespace Teleopti.Ccc.WebTest.Core.Portal.DataProvider
{
	[TestFixture]
	public class ReportsNavigationProviderTest
	{
		private IAuthorization _authorization;
		private IReportsNavigationProvider _target;
		private IReportsProvider _reportsProvider;
		private IReportUrl _reportUrl;

		[SetUp]
		public void Setup()
		{
			_authorization = MockRepository.GenerateMock<IAuthorization>();
			_reportsProvider = MockRepository.GenerateMock<IReportsProvider>();
			_reportUrl = MockRepository.GenerateMock<IReportUrl>();
			_target = new ReportsNavigationProvider(_authorization, _reportsProvider, _reportUrl);
		}

		[Test]
		public void ShouldReturnMyReportWhenNoOtherReportsPermission()
		{
			_authorization.Expect(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyReportWeb)).Return(true);
			_reportsProvider.Stub(x => x.GetReports()).Return(new List<IApplicationFunction>());
			var result = _target.GetNavigationItems();

			result.First().Controller.Should().Be("MyReport");
			result.Count.Should().Be(1);
			_authorization.VerifyAllExpectations();
			
		}

		[Test]
		public void ShouldReturnEmptyListWhenNoReportPermission()
		{
			_authorization.Expect(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyReportWeb)).Return(false);
			_reportsProvider.Stub(x => x.GetReports()).Return(new List<IApplicationFunction>());
			var result = _target.GetNavigationItems();

			result.Count.Should().Be(0);
			_authorization.VerifyAllExpectations();
		}

		[Test]
		public void ShouldReturnReportsWhenOnlyHasReportPermission()
		{
			_authorization.Expect(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyReportWeb)).Return(false);
			var reportList = new List<IApplicationFunction> { new ApplicationFunction("Report CA") { ForeignId = Guid.NewGuid().ToString() } };

			_reportsProvider.Expect(x => x.GetReports()).Return(reportList);

			var result = _target.GetNavigationItems();

			result.Count.Should().Be(1);
			_authorization.VerifyAllExpectations();
		}

		[Test]
		public void ShouldReturnSortedReportsWhenHasReportPermission()
		{
			_authorization.Expect(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyReportWeb)).Return(false);
			var reportList = new List<IApplicationFunction> { new ApplicationFunction("Report CA"){ForeignId = Guid.NewGuid().ToString()},
				new ApplicationFunction("Report CB"){ForeignId = Guid.NewGuid().ToString()},
			new ApplicationFunction("Report Aa"){ForeignId = Guid.NewGuid().ToString()} };

			_reportsProvider.Expect(x => x.GetReports()).Return(reportList);

			var result = _target.GetNavigationItems();

			result[0].Title.Should().Be("Report Aa");
			result[1].Title.Should().Be("Report CA");
			result[2].Title.Should().Be("Report CB");
			_authorization.VerifyAllExpectations();
		}

		[Test]
		public void ShouldReturnReportsWhenHasReportPermissionAndMyReport()
		{
			_authorization.Expect(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyReportWeb)).Return(true);
			var reportList = new List<IApplicationFunction> { new ApplicationFunction("ResReportQueueStatistics") { ForeignId = Guid.NewGuid().ToString() }, new ApplicationFunction("ResReportAgentStatistics") { ForeignId = Guid.NewGuid().ToString() } };

			_reportsProvider.Expect(x => x.GetReports()).Return(reportList);

			var result = _target.GetNavigationItems();

			result.Count.Should().Be(4);
			result.First().IsWebReport.Should().Be(true);
			result[1].IsDivider.Should().Be(true);
			_authorization.VerifyAllExpectations();
		}
		
		[Test]
		public void ShouldReturnBadgeLeaderBoardReportsWhenOnlyHasLeaderBoardReportPermission()
		{
			_authorization.Expect(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard)).Return(true);
			_reportsProvider.Stub(x => x.GetReports()).Return(new List<IApplicationFunction>());
			var result = _target.GetNavigationItems();

			result.First().Controller.Should().Be("BadgeLeaderBoardReport");
			result.Count.Should().Be(1);
			_authorization.VerifyAllExpectations();
		}
	}
}