using Autofac;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommon
{
	public class CommonModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public static CommonModule ForTest()
		{
			return ForTest(new FalseToggleManager());
		}

		public static CommonModule ForTest(IToggleManager toggleManager)
		{
			var iocArgs = new IocArgs(new ConfigReader())
			{
				DataSourceConfigurationSetter = DataSourceConfigurationSetter.ForTest()
			};
			return new CommonModule(new IocConfiguration(iocArgs, toggleManager));
		}

		public CommonModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterModule(new MbCacheModule(_configuration));
			builder.RegisterModule<DateAndTimeModule>();
			builder.RegisterModule<ServiceLocatorModule>();
			builder.RegisterModule<LogModule>();
			builder.RegisterModule<JsonSerializationModule>();
			builder.RegisterModule(new ToggleNetModule(_configuration.Args()));
			builder.RegisterModule(new MessageBrokerModule(_configuration));
			if (_configuration.Args().WebByPassDefaultPermissionCheck_37984)
			{
				builder.RegisterType<ByPassPersistableScheduleDataPermissionChecker>().As<IPersistableScheduleDataPermissionChecker>().SingleInstance();
			}
			else
			{
				builder.RegisterType<PersistableScheduleDataPermissionChecker>().As<IPersistableScheduleDataPermissionChecker>().SingleInstance();
			}
			
			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule(new AnalyticsUnitOfWorkModule(_configuration));
			builder.RegisterModule(new DataSourceModule(_configuration));
			builder.RegisterModule(new UnitOfWorkModule(_configuration));
			builder.RegisterModule(new AuthenticationModule(_configuration));
			builder.RegisterModule(new EncryptionModule(_configuration));
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
			builder.RegisterModule<SchedulePersistModule>();
			builder.RegisterModule(new HangfireModule(_configuration));
			builder.RegisterModule<ForecastEventModule>();
			builder.RegisterModule(new IntradayWebModule(_configuration));
			builder.RegisterModule<StardustModule>();
			builder.RegisterModule(new RequestModule(_configuration));
			builder.RegisterModule<PersonAccountModule>();
			builder.RegisterModule(new SchedulingCommonModule(_configuration));
			builder.RegisterModule(new StaffingModule(_configuration));
			builder.RegisterModule(new ShiftTradeModule(_configuration));
			builder.RegisterModule<BadgeCalculationModule>();
			builder.RegisterModule<CommandDispatcherModule>();
			builder.RegisterModule<CommandHandlersModule>();
			builder.RegisterModule<PeopleAreaModule>();
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