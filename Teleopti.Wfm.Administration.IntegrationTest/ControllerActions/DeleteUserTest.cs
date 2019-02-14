using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTest]
	public class DeleteUserTest
	{
		public AccountController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;


		[Test]
		public void ShouldDelete()
		{
			//new fresh
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			int id;
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var user = new TenantAdminUser { AccessToken = "dummy", Email = "ola@teleopti.com", Name = "Ola", Password = "vadsomhelst" };
				CurrentTenantSession.CurrentSession().Save(user);
				id = user.Id;
			}
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				CurrentTenantSession.CurrentSession().Get<TenantAdminUser>(id).Should().Not.Be.Null();
				Target.DeleteUser(id);
			}
			
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				CurrentTenantSession.CurrentSession().Get<TenantAdminUser>(id).Should().Be.Null();
			}

		}
	}
}