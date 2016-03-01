using Autofac;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class StardustModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<StardustSender>().SingleInstance();
		}
	}
}