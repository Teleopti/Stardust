using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTest]
	public class CheckNewTenantName
	{
		public ImportController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;

		[Test]
		public void ShouldReturnFalseIfNameAlreadyExists()
		{
			//new fresh
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = new Tenant("Old One");
				CurrentTenantSession.CurrentSession().Save(tenant);
			}

			Target.IsNewTenant("old one").Content.Success.Should().Be.False();
		}

		[Test]
		public void ShouldReturnTrueIfNameNotExists()
		{
			//new fresh
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = new Tenant("Old One");
				CurrentTenantSession.CurrentSession().Save(tenant);
			}

			Target.IsNewTenant("New One").Content.Success.Should().Be.True();
		}
	}
}