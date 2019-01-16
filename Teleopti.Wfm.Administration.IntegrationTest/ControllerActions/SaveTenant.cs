using System.Linq;
using NHibernate.Cfg;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTest]
	public class SaveTenant
	{
		public HomeController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;
		public ILoadAllTenants Tenants;

		[Test]
		public void ShouldReturnFalseIfNameAlreadyExists()
		{
			//new fresh
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = new Tenant("Old One");
				CurrentTenantSession.CurrentSession().Save(tenant);
			}
			var model = new UpdateTenantModel { NewName = "old one", OriginalName = "someother name" };
			Target.NameIsFree(model).Content.Success.Should().Be.False();
		}

		[Test]
		public void ShouldReturnTrueIdIfFreeName()
		{
			//new fresh
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = new Tenant("Old One");
				CurrentTenantSession.CurrentSession().Save(tenant);
			}
			var model = new UpdateTenantModel { NewName = "new one", OriginalName = "old one" };
			Target.NameIsFree(model).Content.Success.Should().Be.True();
		}

		[Test]
		public void ShouldReturnFalseIfCommandTimeoutIsZero()
		{
			SetupTenant();
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var model = new UpdateTenantModel
				{
					NewName = "Old One",
					OriginalName = "Old One",
					Server = "(local)",
					UserName = "ola",
					Password = "password",
					AnalyticsDatabase = "Southwind",
					AppDatabase = "Southwind"
				};
				var result = Target.Save(model);
				result.Content.Success.Should().Be.False();
			}

		}

		[Test]
		public void ShouldOnlyUpdateExistingTenantActiveAndTimeout()
		{
			SetupTenant();
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var model = new UpdateTenantModel
				{
					NewName = "Old One",
					OriginalName = "Old One",
					Server = "(local)",
					UserName = "ola",
					Password = "password",
					AnalyticsDatabase = "Southwind",
					AppDatabase = "Southwind",
					CommandTimeout = 180,
					MobileQRCodeUrl = string.Empty,
					Active = false
				};
				Target.Save(model);
			}
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var loadedTenant = Tenants.Tenants().FirstOrDefault(t => t.Name.Equals("Old One"));
				loadedTenant.DataSourceConfiguration.ApplicationConnectionString.Should().Contain("Initial Catalog=Northwind");
				loadedTenant.DataSourceConfiguration.AnalyticsConnectionString.Should().Contain("Initial Catalog=Northwind");
				loadedTenant.ApplicationConfig[Environment.CommandTimeout].Should().Be.EqualTo("180");
				loadedTenant.Active.Should().Be.False();
			}
		}

		[Test]
		public void ShouldSaveMobileQRCodeUrlToApplicationConfig()
		{
			var QRCodeUrl = "http://localhost:8089/WFM";
			SetupTenant();
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var model = new UpdateTenantModel
				{
					NewName = "Old One",
					OriginalName = "Old One",
					Server = "(local)",
					UserName = "ola",
					Password = "password",
					AnalyticsDatabase = "Southwind",
					AppDatabase = "Southwind",
					CommandTimeout = 180,
					Active = false,
					MobileQRCodeUrl = QRCodeUrl
				};
				Target.Save(model);
			}

			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var loadedTenant = Tenants.Tenants().FirstOrDefault(t => t.Name.Equals("Old One"));
				loadedTenant.GetApplicationConfig(TenantApplicationConfigKey.MobileQRCodeUrl).Should().Be.EqualTo(QRCodeUrl);
			}
		}

		[Test]
		public void ShouldSaveMaximumSessionTimeToApplicationConfig()
		{
			var maximumSessionTimeInMinutes = 480;
			SetupTenant();
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var model = new UpdateTenantModel
				{
					NewName = "Old One",
					OriginalName = "Old One",
					Server = "(local)",
					UserName = "ola",
					Password = "password",
					AnalyticsDatabase = "Southwind",
					AppDatabase = "Southwind",
					CommandTimeout = 180,
					Active = false,
					MobileQRCodeUrl = string.Empty,
					MaximumSessionTime = maximumSessionTimeInMinutes
				};
				Target.Save(model);
			}

			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var loadedTenant = Tenants.Tenants().FirstOrDefault(t => t.Name.Equals("Old One"));
				loadedTenant.GetApplicationConfig(TenantApplicationConfigKey.MaximumSessionTimeInMinutes).Should().Be.EqualTo(maximumSessionTimeInMinutes.ToString());
			}
		}

		protected void SetupTenant()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = new Tenant("Old One");
				tenant.DataSourceConfiguration.SetAnalyticsConnectionString("Integrated Security=true;Initial Catalog=Northwind;server=(local");
				tenant.DataSourceConfiguration.SetApplicationConnectionString("Integrated Security=true;Initial Catalog=Northwind;server=(local");
				CurrentTenantSession.CurrentSession().Save(tenant);
			}
		}

	}
}