using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public static class ContainerBuilderExtensions
	{
		public static void RegisterToggledTypeTest<TToggleOn, TToggleOff, IT>(
			this ContainerBuilder builder, FakeToggleManager toggleManager, Toggles toggle)
			where TToggleOn : IT 
			where TToggleOff : IT
		{
			if(toggleManager.IsEnabled(toggle))
			{
				builder.RegisterType<TToggleOn>().As<IT>().SingleInstance();
			}
			else
			{
				builder.RegisterType<TToggleOff>().As<IT>().SingleInstance();
			}
		}
	}
}