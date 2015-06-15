using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.AdministrationTest
{
	public class TenantTestAttribute : IoCTestAttribute
	{
		private const string tenancyConnectionStringKey = "Tenancy";

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.AddModule(new WfmAdminModule());

			var configReader = new ConfigReader();
			var connStringToTenant = configReader.ConnectionStrings[tenancyConnectionStringKey];
			var connstringAsString = connStringToTenant == null ? null : connStringToTenant.ConnectionString;
			var service =  TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(connstringAsString);
			system.AddService(service);

			system.AddService<DatabaseHelperWrapper>();
			system.AddService<AdminTenantAuthentication>();
		}
	}
}