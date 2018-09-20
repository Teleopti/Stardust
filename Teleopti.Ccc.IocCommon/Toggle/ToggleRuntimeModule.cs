using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	internal class ToggleRuntimeModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<TogglesActive>()
				.SingleInstance().As<ITogglesActive>();
			builder.RegisterType<AllToggles>()
				.SingleInstance().As<IAllToggles>();
		}
	}
}