using System.Configuration;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.AdministrationTest
{
	public class TenantTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.AddModule(new WfmAdminModule());
			system.UseTestDouble<ConsoleLogger>().For<IUpgradeLog>();

			var service = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
			system.AddService(service);

			system.AddService<DbPathProviderFake>();
			system.AddService<CheckPasswordStrengthFake>();
			system.AddService<TestPollutionCleaner>();
		}
	}
}