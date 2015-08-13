using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class SaveTenant
	{
		public HomeController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;
		public LoadAllTenants LoadAllTenants;

		[Test]
		public void ShouldReturnFalseIfNameAlreadyExists()
		{
			//new fresh
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			using (TenantUnitOfWork.Start())
			{
				var tenant = new Tenant("Old One");
				CurrentTenantSession.CurrentSession().Save(tenant);
			}
			var model = new UpdateTenantModel {NewName = "old one", OriginalName = "someother name"};
            Target.NameIsFree(model).Content.Success.Should().Be.False();
		}

		[Test]
		public void ShouldReturnTrueIdIfFreeName()
		{
			//new fresh
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			using (TenantUnitOfWork.Start())
			{
				var tenant = new Tenant("Old One");
				CurrentTenantSession.CurrentSession().Save(tenant);
			}
			var model = new UpdateTenantModel { NewName = "new one", OriginalName = "old one" };
			Target.NameIsFree(model).Content.Success.Should().Be.True();
		}

		[Test]
		public void ShouldUpdateExistingTenant()
		{
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			using (TenantUnitOfWork.Start())
			{
				var tenant = new Tenant("Old One");
				tenant.DataSourceConfiguration.SetAnalyticsConnectionString("Integrated Security=true;Initial Catalog=Northwind;server=(local");
				tenant.DataSourceConfiguration.SetApplicationConnectionString("Integrated Security=true;Initial Catalog=Northwind;server=(local");
				CurrentTenantSession.CurrentSession().Save(tenant);
			}
			using (TenantUnitOfWork.Start())
			{
				var model = new UpdateTenantModel
				{
					NewName = "Old One",
					OriginalName = "Old One",
					AnalyticsDatabase = "Integrated Security=true;Initial Catalog=Southwind;server=(local)",
					AppDatabase = "Integrated Security=true;Initial Catalog=Southwind;server=(local)"
				};
				Target.Save(model);
			}
			using (TenantUnitOfWork.Start())
			{
				var loadedTenant = LoadAllTenants.Tenants().FirstOrDefault(t => t.Name.Equals("Old One"));
				loadedTenant.DataSourceConfiguration.ApplicationConnectionString.Should().Be.EqualTo("Integrated Security=true;Initial Catalog=Southwind;server=(local)");
				loadedTenant.DataSourceConfiguration.AnalyticsConnectionString.Should().Be.EqualTo("Integrated Security=true;Initial Catalog=Southwind;server=(local)");
			}
		}
	}
}