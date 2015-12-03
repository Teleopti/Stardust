using Autofac;
using Hangfire;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Hangfire;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class HangfireModule : Module
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
			builder.RegisterType<HangfireEventProcessor>().As<IHangfireEventProcessor>().SingleInstance();

			builder.RegisterType<HangfireEventClient>().As<IHangfireEventClient>().SingleInstance();
			builder.RegisterType<BackgroundJobClient>().As<IBackgroundJobClient>().SingleInstance();

			if (_configuration.Toggle(Toggles.RTA_NewEventHangfireRTA_34333))
				builder.RegisterType<HangFireClient>().As<IHangFireClient>().SingleInstance();
			else
				builder.RegisterType<NoHangFireClient>().As<IHangFireClient>().SingleInstance();
		}
	}
}