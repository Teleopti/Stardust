using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Areas.Reporting.Controllers;
using Teleopti.Ccc.Web.Areas.Reporting.Core;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Reporting.Controller
{
	public class ReportControllerTest
	{
		[Test]
		public void ShouldCheckAndUpdateReportPermissionInAnalytics()
		{
			var reportsNavigationProvider = MockRepository.GenerateMock<IReportsNavigationProvider>();
			var reportId = Guid.NewGuid();
			reportsNavigationProvider.Stub(x => x.GetNavigationItems()).Return(new[]
			{
				new ReportNavigationItem
				{
					Id = reportId
				}
			});

			var toggleManager = MockRepository.GenerateMock<IToggleManager>();
			toggleManager.Stub(x => x.IsEnabled(Toggles.ETL_SpeedUpPermissionReport_33584)).Return(true);
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = PersonFactory.CreatePersonWithGuid("first", "last");
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			var currentBusinessUnit = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			var businessUnit = BusinessUnitFactory.CreateWithId("bu");
			currentBusinessUnit.Stub(x => x.Current()).Return(businessUnit);
			var analyticsPermissionsUpdater = MockRepository.GenerateMock<IAnalyticsPermissionsUpdater>();
			System.Threading.Thread.CurrentPrincipal = new TeleoptiPrincipal(new TeleoptiIdentity("test", new FakeDataSource()
			{
				Analytics = new FakeAnalyticsUnitOfWorkFactory() { ConnectionString = ""}

			}, null, null, null), person);
			var commonReportsFactory = MockRepository.GenerateMock<ICommonReportsFactory>();
			commonReportsFactory.Stub(x => x.CreateAndLoad("", reportId)).Return(new FakeCommonReports());
			var target = new ReportController(reportsNavigationProvider, MockRepository.GenerateMock<IPersonNameProvider>(), loggedOnUser, currentBusinessUnit, toggleManager, analyticsPermissionsUpdater, commonReportsFactory);

			target.Index(reportId);

			analyticsPermissionsUpdater.AssertWasCalled(x => x.Handle(person.Id.GetValueOrDefault(), businessUnit.Id.GetValueOrDefault()));

		}
	}

	public class FakeCommonReports : ICommonReports
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