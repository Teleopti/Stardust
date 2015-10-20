using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class DeleteTenantTest
	{
		public HomeController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;
		public ILoadAllTenants LoadAllTenants;

		[Test]
		public void ShouldNotAllowDeleteOnUnknowTenant()
		{
			DataSourceHelper.CreateDataSource(new NoPersistCallbacks(), "TestData");
			Target.DeleteTenant("SomeUnknownOne").Content.Success.Should().Be.False();
		}

		[Test]
		public void ShouldNotAllowDeleteOnBaseTenant()
		{
			DataSourceHelper.CreateDataSource(new NoPersistCallbacks(), "TestData");

         using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = new Tenant("Teleopti WFM");
				tenant.DataSourceConfiguration.SetApplicationConnectionString(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
				CurrentTenantSession.CurrentSession().Save(tenant);
			}
			Target.DeleteTenant("Teleopti WFM").Content.Success.Should().Be.False();
		}

		[Test]
		public void ShoulAllowDeleteOnNotBaseTenant()
		{
			DataSourceHelper.CreateDataSource(new NoPersistCallbacks(), "TestData");

			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = new Tenant("Teleopti WFM");
				tenant.DataSourceConfiguration.SetAnalyticsConnectionString("Integrated Security=true;Initial Catalog=Northwind;server=(local");
				tenant.DataSourceConfiguration.SetApplicationConnectionString("Integrated Security=true;Initial Catalog=Northwind;server=(local");
				CurrentTenantSession.CurrentSession().Save(tenant);
			}
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				LoadAllTenants.Tenants().Count().Should().Be.EqualTo(2);
			}
			Target.DeleteTenant("Teleopti WFM").Content.Success.Should().Be.True();
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				LoadAllTenants.Tenants().Count().Should().Be.EqualTo(1);
			}
		}

	}
}