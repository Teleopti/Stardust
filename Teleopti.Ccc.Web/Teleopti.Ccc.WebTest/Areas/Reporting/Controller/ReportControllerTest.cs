using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Areas.Reporting.Controllers;
using Teleopti.Ccc.Web.Areas.Reporting.Core;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.WebTest.Core.Common;

namespace Teleopti.Ccc.WebTest.Areas.Reporting.Controller
{
	[TestFixture]
	public class ReportControllerTest
	{
		[Test]
		public void ShouldCheckAndUpdateReportPermissionInAnalytics()
		{
			var reportsNavigationProvider = MockRepository.GenerateMock<IReportsNavigationProvider>();
			var analyticsPermissionsUpdater = MockRepository.GenerateMock<IAnalyticsPermissionsUpdater>();

			var reportId = Guid.NewGuid();
			reportsNavigationProvider.Stub(x => x.GetNavigationItems()).Return(new[]
			{
				new ReportNavigationItem
				{
					Id = reportId
				}
			});
			
			var person = PersonFactory.CreatePersonWithGuid("first", "last");
			var businessUnit = BusinessUnitFactory.CreateWithId("bu");
			var currentBusinessUnit = new FakeCurrentBusinessUnit();
			currentBusinessUnit.FakeBusinessUnit(businessUnit);
			
			System.Threading.Thread.CurrentPrincipal = new TeleoptiPrincipal(new TeleoptiIdentity("test", new FakeDataSource
			{
				Analytics = new FakeAnalyticsUnitOfWorkFactory { ConnectionString = ""}

			}, null, null, null), person);
			var target = new ReportController(reportsNavigationProvider, new FakePersonNameProvider(), new FakeLoggedOnUser(person), currentBusinessUnit, new TrueToggleManager(), analyticsPermissionsUpdater, new FakeCommonReportsFactory());

			target.Index(reportId);

			analyticsPermissionsUpdater.AssertWasCalled(x => x.Handle(person.Id.GetValueOrDefault(), businessUnit.Id.GetValueOrDefault()));

		}
	}

	public class FakeCommonReportsFactory : ICommonReportsFactory
	{
		public ICommonReports CreateAndLoad(string connectionString, Guid reportId)
		{
			return new fakeCommonReports();
		}

		private class fakeCommonReports : ICommonReports
		{
			public void Dispose()
			{
			}

			public void LoadReportInfo()
			{
			}

			public string ResourceKey { get; set; }
			public string Name { get; set; }
			public string HelpKey { get; set; }
		}
	}
}