using Autofac;
using Hangfire;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
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
			builder.RegisterType<HangfireServerStarter>().As<IHangfireServerStarter>().SingleInstance();
			if (_configuration.Toggle(Toggles.RTA_HangfireEventProcessinUsingMsmq_31237))
				builder.RegisterType<MsmqStorageConfiguration>().As<IHangfireServerStorageConfiguration>().SingleInstance();
			else
				builder.RegisterType<SqlStorageConfiguration>().As<IHangfireServerStorageConfiguration>().SingleInstance();

			builder.RegisterType<HangfireEventServer>().SingleInstance();

			builder.RegisterType<HangfireEventClient>().As<IHangfireEventClient>().SingleInstance();
			builder.RegisterType<BackgroundJobClient>().As<IBackgroundJobClient>().SingleInstance();
		}
	}
}