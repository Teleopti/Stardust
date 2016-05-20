using Autofac;
using Hangfire;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Interfaces.Infrastructure;

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
			builder.RegisterType<HangfireStarter>().SingleInstance();
			builder.RegisterType<HangfireServerStarter>().SingleInstance();
			builder.RegisterType<HangfireClientStarter>().SingleInstance();

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

			//builder.RegisterType<HangfireUtilities>().SingleInstance().ApplyAspects();
			builder.RegisterType<HangfireUtilities>().As<IHangfireUtilities>().SingleInstance().ApplyAspects();
		}
	}
}