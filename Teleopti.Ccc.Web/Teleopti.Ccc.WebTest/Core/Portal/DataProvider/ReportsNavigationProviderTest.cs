using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal.DataProvider
{
	[TestFixture]
	public class ReportsNavigationProviderTest
	{
		private IPrincipalAuthorization _principalAuthorization;
		private PermissionProvider _permissionProvider;
		private IReportsNavigationProvider _target;
		private IReportsProvider _reportsProvider;

		[SetUp]
		public void Setup()
		{
			_principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			_permissionProvider = new PermissionProvider(_principalAuthorization);
			_reportsProvider = MockRepository.GenerateMock<IReportsProvider>();
			_target = new ReportsNavigationProvider(_principalAuthorization,_reportsProvider);
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
			var reportList = new List<IApplicationFunction> { new ApplicationFunction("Report CA")};

			_reportsProvider.Expect(x => x.GetReports()).Return(reportList);

			var result = _target.GetNavigationItems();

			result.Count.Should().Be(1);
			_principalAuthorization.VerifyAllExpectations();
		}

		[Test]
		public void ShouldReturnSortedReportsWhenHasReportPermission()
		{
			_principalAuthorization.Expect(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyReportWeb)).Return(false);
			var reportList = new List<IApplicationFunction> { new ApplicationFunction("Report CA"),
				new ApplicationFunction("Report CB"),
			new ApplicationFunction("Report Aa") };

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
			var reportList = new List<IApplicationFunction> { new ApplicationFunction("ResReportQueueStatistics"), new ApplicationFunction("ResReportAgentStatistics") };

			_reportsProvider.Expect(x => x.GetReports()).Return(reportList);

			var result = _target.GetNavigationItems();

			result.Count.Should().Be(4);
			result.First().IsMyReport.Should().Be(true);
			result[1].IsDivider.Should().Be(true);
			_principalAuthorization.VerifyAllExpectations();
		}
	}
}