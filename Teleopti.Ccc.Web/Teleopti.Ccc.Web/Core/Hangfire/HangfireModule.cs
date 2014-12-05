using Autofac;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	public class HangfireModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<HangfireServerStarter>().As<IHangfireServerStarter>().SingleInstance();
			builder.RegisterType<HangfireEventProcessor>().SingleInstance();
		}
	}
}