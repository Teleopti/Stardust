using Autofac;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommon
{
	public class SharedModuleUsedInBothRuntimeContainerAndToggleManagerModule : Module
	{
		private readonly IocArgs _iocArgs;

		public SharedModuleUsedInBothRuntimeContainerAndToggleManagerModule(IocArgs iocArgs)
		{
			_iocArgs = iocArgs;
		}
		
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterInstance(_iocArgs.ConfigReader).As<IConfigReader>().SingleInstance();
			builder.RegisterModule(new MbCacheModule(_iocArgs));
			builder.RegisterModule(new ToggleNetModule(_iocArgs));
		}
	}
}