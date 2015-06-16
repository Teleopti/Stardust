using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class CheckNewTenantName
	{
		public ImportController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;

		[Test]
		public void ShouldReturnFalseIfNameAlreadyExists()
		{
			//new fresh
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			TenantUnitOfWork.Start();
			var tenant = new Tenant("Old One");
			CurrentTenantSession.CurrentSession().Save(tenant);
			TenantUnitOfWork.CommitAndDisposeCurrent();
			TenantUnitOfWork.Start();
			Target.IsNewTenant("old one").Content.Success.Should().Be.False();
			TenantUnitOfWork.CancelAndDisposeCurrent();
		}

		[Test]
		public void ShouldReturnTrueIfNameNotExists()
		{
			//new fresh
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			TenantUnitOfWork.Start();
			var tenant = new Tenant("Old One");
			CurrentTenantSession.CurrentSession().Save(tenant);
			TenantUnitOfWork.CommitAndDisposeCurrent();
			TenantUnitOfWork.Start();
			Target.IsNewTenant("New One").Content.Success.Should().Be.True();
			TenantUnitOfWork.CancelAndDisposeCurrent();
		}
	}
}