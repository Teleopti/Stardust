using Autofac;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Analytics.Etl.Common
{
	public class EtlAppModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			var iocArgs = new IocArgs(new ConfigReader());
			var configuration = new IocConfiguration(iocArgs, CommonModule.ToggleManagerForIoc(iocArgs));
			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterModule(new TenantServerModule(configuration));
			builder.RegisterModule(new EtlModule(configuration));
		}
	}
}