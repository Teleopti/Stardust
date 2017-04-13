using Autofac;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class StardustModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<StardustSender>().As<IStardustSender>().SingleInstance();
			builder.RegisterType<StardustJobFeedback>().As<IStardustJobFeedback>().SingleInstance();
		}
	}
}