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

		protected override void RegisterInContainer(ISystem builder, IIocConfiguration configuration)
		{
			base.RegisterInContainer(builder, configuration);

			builder.RegisterModule(new WfmAdminModule());

			var configReader = new ConfigReader();
			var connStringToTenant = configReader.ConnectionStrings[tenancyConnectionStringKey];
			var connstringAsString = connStringToTenant == null ? null : connStringToTenant.ConnectionString;
			var service =  TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(connstringAsString);
			builder.AddService(service);

			builder.AddService<DatabaseHelperWrapper>();
			builder.AddService<AdminTenantAuthentication>();
		}
	}
}