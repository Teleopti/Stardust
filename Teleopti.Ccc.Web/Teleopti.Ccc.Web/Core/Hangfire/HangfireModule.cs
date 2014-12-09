using Autofac;
using Hangfire;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	public class HangfireModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<HangfireServerStarter>().As<IHangfireServerStarter>().SingleInstance();

			builder.RegisterType<HangfireEventServer>().SingleInstance();
			builder.RegisterType<HangfireEventProcessor>().As<IHangfireEventProcessor>().SingleInstance();

			builder.RegisterType<HangfireEventClient>().As<IHangfireEventClient>().SingleInstance();
			builder.RegisterType<BackgroundJobClient>().As<IBackgroundJobClient>().SingleInstance();
		}
	}
}