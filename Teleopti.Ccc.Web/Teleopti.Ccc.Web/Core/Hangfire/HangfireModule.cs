using Autofac;
using Hangfire;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	public class HangfireModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public HangfireModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<HangfireServerStarter>().SingleInstance();
			builder.RegisterType<ActivityChangesChecker>().SingleInstance();

			builder.RegisterType<HangfireEventServer>().SingleInstance();

			builder.RegisterType<HangfireEventClient>().As<IHangfireEventClient>().SingleInstance();
			builder.RegisterType<BackgroundJobClient>().As<IBackgroundJobClient>().SingleInstance();
		}
	}
}