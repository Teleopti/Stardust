using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class GetVersionsTest
	{
		public UpgradeDatabasesController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;

		[Test]
		public void ShouldReportOkIfSameVersion()
		{
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			TenantUnitOfWork.Start();
			
			var result = Target.GetVersions(new VersionCheckModel{AppConnectionString = CurrentTenantSession.CurrentSession().Connection.ConnectionString });

			result.Content.AppVersionOk.Should().Be.True();
			TenantUnitOfWork.CommitAndDisposeCurrent();
		}
	}
}