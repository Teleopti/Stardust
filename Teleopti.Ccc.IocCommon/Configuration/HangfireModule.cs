using Autofac;
using Hangfire;
using Hangfire.Client;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Aop;
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

			builder.RegisterType<HangfireEventClient>().As<IHangfireEventClient>().SingleInstance();

			builder.Register(c => JobStorage.Current).SingleInstance();
			builder.Register(c =>
				new BackgroundJobClient(c.Resolve<JobStorage>()))
				.As<IBackgroundJobClient>()
				.SingleInstance();
			builder.Register(c =>
				new RecurringJobManager(
					c.Resolve<JobStorage>()
					))
				.As<RecurringJobManager>()
				.SingleInstance();

			builder.RegisterType<HangfireClientStarter>().As<IHangfireClientStarter>().SingleInstance();

			builder.RegisterType<HangfireUtilties>().SingleInstance().ApplyAspects();
		}
	}
}