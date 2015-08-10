using System.Configuration;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.AdministrationTest
{
	public class TenantTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.AddModule(new WfmAdminModule());

			var service = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
			system.AddService(service);

			system.AddService<DatabaseHelperWrapper>();
			system.AddService<AdminTenantAuthentication>();
			system.AddService<LoadAllPersonInfos>();
			system.AddService<GetImportUsers>();
			system.AddService<LoadAllTenants>();
			system.AddService<DbPathProviderFake>();
			system.AddService<CheckPasswordStrengthFake>();
		}
	}
}