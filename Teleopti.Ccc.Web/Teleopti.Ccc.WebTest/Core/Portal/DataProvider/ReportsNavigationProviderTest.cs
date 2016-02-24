using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Portal.DataProvider
{
	[TestFixture]
	public class ReportsNavigationProviderTest
	{
		private IPrincipalAuthorization _principalAuthorization;
		private IReportsNavigationProvider _target;
		private IReportsProvider _reportsProvider;
		private IToggleManager _toggleManagger;
		private IReportUrl _reportUrl;

		[SetUp]
		public void Setup()
		{
			_principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			_reportsProvider = MockRepository.GenerateMock<IReportsProvider>();
			_toggleManagger = MockRepository.GenerateMock<IToggleManager>();
			_reportUrl = MockRepository.GenerateMock<IReportUrl>();
			_target = new ReportsNavigationProvider(_principalAuthorization, _reportsProvider, _reportUrl, _toggleManagger);
		}

		[Test]
		public void ShouldReturnMyReportWhenNoOtherReportsPermission()
		{
			_principalAuthorization.Expect(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyReportWeb)).Return(true);
			_reportsProvider.Stub(x => x.GetReports()).Return(new List<IApplicationFunction>());
			var result = _target.GetNavigationItems();

			result.First().Controller.Should().Be("MyReport");
			result.Count.Should().Be(1);
			_principalAuthorization.VerifyAllExpectations();
			
		}

		[Test]
		public void ShouldReturnEmptyListWhenNoReportPermission()
		{
			_principalAuthorization.Expect(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyReportWeb)).Return(false);
			_reportsProvider.Stub(x => x.GetReports()).Return(new List<IApplicationFunction>());
			var result = _target.GetNavigationItems();

			result.Count.Should().Be(0);
			_principalAuthorization.VerifyAllExpectations();
		}

		[Test]
		public void ShouldReturnReportsWhenOnlyHasReportPermission()
		{
			_principalAuthorization.Expect(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyReportWeb)).Return(false);
			var reportList = new List<IApplicationFunction> { new ApplicationFunction("Report CA") { ForeignId = Guid.NewGuid().ToString() } };

			_reportsProvider.Expect(x => x.GetReports()).Return(reportList);

			var result = _target.GetNavigationItems();

			result.Count.Should().Be(1);
			_principalAuthorization.VerifyAllExpectations();
		}

		[Test]
		public void ShouldReturnSortedReportsWhenHasReportPermission()
		{
			_principalAuthorization.Expect(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyReportWeb)).Return(false);
			var reportList = new List<IApplicationFunction> { new ApplicationFunction("Report CA"){ForeignId = Guid.NewGuid().ToString()},
				new ApplicationFunction("Report CB"){ForeignId = Guid.NewGuid().ToString()},
			new ApplicationFunction("Report Aa"){ForeignId = Guid.NewGuid().ToString()} };

			_reportsProvider.Expect(x => x.GetReports()).Return(reportList);

			var result = _target.GetNavigationItems();

			result[0].Title.Should().Be("Report Aa");
			result[1].Title.Should().Be("Report CA");
			result[2].Title.Should().Be("Report CB");
			_principalAuthorization.VerifyAllExpectations();
		}

		[Test]
		public void ShouldReturnReportsWhenHasReportPermissionAndMyReport()
		{
			_principalAuthorization.Expect(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyReportWeb)).Return(true);
			var reportList = new List<IApplicationFunction> { new ApplicationFunction("ResReportQueueStatistics") { ForeignId = Guid.NewGuid().ToString() }, new ApplicationFunction("ResReportAgentStatistics") { ForeignId = Guid.NewGuid().ToString() } };

			_reportsProvider.Expect(x => x.GetReports()).Return(reportList);

			var result = _target.GetNavigationItems();

			result.Count.Should().Be(4);
			result.First().IsWebReport.Should().Be(true);
			result[1].IsDivider.Should().Be(true);
			_principalAuthorization.VerifyAllExpectations();
		}
		
		[Test]
		public void ShouldReturnBadgeLeaderBoardReportsWhenOnlyHasLeaderBoardReportPermission()
		{
			_principalAuthorization.Expect(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard)).Return(true);
			_reportsProvider.Stub(x => x.GetReports()).Return(new List<IApplicationFunction>());
			var result = _target.GetNavigationItems();

			result.First().Controller.Should().Be("BadgeLeaderBoardReport");
			result.Count.Should().Be(1);
			_principalAuthorization.VerifyAllExpectations();
		}
	}
}