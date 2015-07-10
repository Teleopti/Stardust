using System;
using Autofac;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.MultipleConfig;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon
{
	public class CommonModule : Module
	{
		private readonly IIocConfiguration _configuration;
		public Type RepositoryConstructorType { get; set; }

		public static CommonModule ForTest()
		{
			return ForTest(new FalseToggleManager());
		}

		public static CommonModule ForTest(IToggleManager toggleManager)
		{
			return new CommonModule(
				new IocConfiguration(
					new IocArgs(new AppConfigReader())
					{
						DataSourceConfigurationSetter = DataSourceConfigurationSetter.ForTest()
					},
					toggleManager
					)
				);
		}

		public CommonModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
			RepositoryConstructorType = typeof(ICurrentUnitOfWork);
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterModule(new MbCacheModule(_configuration));
			builder.RegisterModule<DateAndTimeModule>();
			builder.RegisterModule<LogModule>();
			builder.RegisterModule<JsonSerializationModule>();
			builder.RegisterModule(new ToggleNetModule(_configuration.Args()));
			builder.RegisterModule(new MessageBrokerModule(_configuration));
			builder.RegisterModule(new RepositoryModule { RepositoryConstructorType = RepositoryConstructorType });
			builder.RegisterModule(new UnitOfWorkModule(_configuration));
			builder.RegisterModule<AuthenticationModule>();
			builder.RegisterModule<ForecasterModule>();
			builder.RegisterModule(new EventHandlersModule(_configuration));
			builder.RegisterModule(new EventPublisherModule(_configuration));
			builder.RegisterModule<AspectsModule>();
			builder.RegisterModule<ReadModelUnitOfWorkModule>();
			builder.RegisterModule<MessageBrokerUnitOfWorkModule>();
			builder.RegisterModule<WebModule>();
			builder.RegisterModule<ServiceBusModule>();
			builder.RegisterModule(new InitializeModule(_configuration));
			builder.RegisterModule(new TenantClientModule(_configuration));
			builder.RegisterModule<DistributedLockModule>();
			builder.RegisterModule(new RtaModule(_configuration));
			builder.RegisterModule(new MessageBrokerServerModule(_configuration));

			// move into HangfireModule when that module can be moved to IoCCommon
			builder.RegisterType<HangfireEventProcessor>().As<IHangfireEventProcessor>().SingleInstance();
		}

		public static IToggleManager ToggleManagerForIoc(IocArgs iocArgs)
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new ToggleNetModule(iocArgs));
			using (var container = builder.Build())
				return container.Resolve<IToggleManager>();
		}
	}
}