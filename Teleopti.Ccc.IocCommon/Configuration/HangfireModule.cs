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
			builder.RegisterType<HangfireEventProcessor>().SingleInstance();

			builder.RegisterType<HangfireEventClient>().As<IHangfireEventClient>().SingleInstance();

			builder.Register(c => JobStorage.Current).SingleInstance();
			builder.Register(c =>
				new BackgroundJobClient(c.Resolve<JobStorage>()))
				.As<IBackgroundJobClient>()
				.SingleInstance();
			builder.Register(c =>
				new RecurringJobManager(
					c.Resolve<JobStorage>(),
					c.Resolve<IBackgroundJobClient>()
					))
				.As<RecurringJobManager>()
				.SingleInstance();

			if (_configuration.Toggle(Toggles.RTA_NewEventHangfireRTA_34333))
				builder.RegisterType<HangfireClientStarter>().As<IHangfireClientStarter>().SingleInstance();
			else
				builder.RegisterType<NoHangfireClient>().As<IHangfireClientStarter>().SingleInstance();
		}
	}
}