using Autofac;
using Hangfire;
using Teleopti.Ccc.Domain.Infrastructure;
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
			builder.RegisterType<HangfireStarter>().SingleInstance();
			builder.RegisterType<HangfireServerStarter>().SingleInstance();
			builder.RegisterType<HangfireClientStarter>().SingleInstance();

			builder.RegisterType<HangfireEventServer>().SingleInstance();
			builder.RegisterType<HangfireEventClient>().As<IHangfireEventClient>().SingleInstance();

			builder.RegisterType<JobStorageWrapper>().As<IJobStorageWrapper>().SingleInstance();
			builder.Register(c => new BackgroundJobClient(c.Resolve<IJobStorageWrapper>().GetJobStorage()))
				.As<IBackgroundJobClient>()
				.SingleInstance();
			builder.Register(c => new RecurringJobManager(c.Resolve<IJobStorageWrapper>().GetJobStorage()))
				.As<RecurringJobManager>()
				.SingleInstance();

			builder.RegisterType<HangfireUtilities>()
				.AsSelf()
				.As<ICleanHangfire>()
				.SingleInstance()
				.ApplyAspects();
		}
	}
}