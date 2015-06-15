using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class CheckNewTenantName
	{
		public ImportController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;

		[Test, Ignore]
		public void ShouldReturnFalseIfNameAlreadyExists()
		{
			TenantUnitOfWork.Start();
			var tenant = new Tenant("Old One");
			CurrentTenantSession.CurrentSession().Save(tenant);
			Target.IsNewTenant("old one").Content.Success.Should().Be.False();
			TenantUnitOfWork.CancelAndDisposeCurrent();
		}

		[Test, Ignore]
		public void ShouldReturnTrueIfNameNotExists()
		{
			TenantUnitOfWork.Start();
			var tenant = new Tenant("Old One");
			CurrentTenantSession.CurrentSession().Save(tenant);
			Target.IsNewTenant("New One").Content.Success.Should().Be.True();
			TenantUnitOfWork.CancelAndDisposeCurrent();
		}
	}
}