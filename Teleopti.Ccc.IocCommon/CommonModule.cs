using Autofac;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommon
{
	public class CommonModule : Module
	{
		private readonly IocConfiguration _configuration;

		public static CommonModule ForTest()
		{
			return ForTest(new FalseToggleManager());
		}

		public static CommonModule ForTest(IToggleManager toggleManager)
		{
			var iocArgs = new IocArgs(new ConfigReader())
			{
				DataSourceApplicationName = DataSourceApplicationName.ForTest()
			};
			return new CommonModule(new IocConfiguration(iocArgs, toggleManager));
		}

		public CommonModule(IocConfiguration configuration)
		{
			_configuration = configuration;
			_configuration.FillToggles();
		}

		protected override void Load(ContainerBuilder builder)
		{
			_configuration.AddToggleManagerToBuilder(builder);
			builder.RegisterModule<ToggleRuntimeModule>();
			builder.RegisterModule(new SharedModuleUsedInBothRuntimeContainerAndToggleManagerModule(_configuration.Args()));
			builder.RegisterModule(new RuleSetModule(_configuration));
			builder.RegisterModule<DateAndTimeModule>();
			builder.RegisterModule<ServiceLocatorModule>();
			builder.RegisterModule<LogModule>();
			builder.RegisterModule<JsonSerializationModule>();
			builder.RegisterModule(new MessageBrokerModule(_configuration));
			
			builder.RegisterType<ByPassPersistableScheduleDataPermissionChecker>().As<IPersistableScheduleDataPermissionChecker>().SingleInstance();

			builder.RegisterModule(new RepositoryModule(_configuration));
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
			builder.RegisterModule(new BadgeCalculationModule(_configuration));
			builder.RegisterModule(new WebModule(_configuration));
			builder.RegisterModule<ServiceBusModule>();
			builder.RegisterModule(new InitializeModule(_configuration));
			builder.RegisterModule(new TenantModule(_configuration));
			builder.RegisterModule<DistributedLockModule>();
			builder.RegisterModule(new RtaModule(_configuration));
			builder.RegisterModule(new MessageBrokerServerModule(_configuration));
			builder.RegisterModule<SchedulePersistModule>();
			builder.RegisterModule(new HangfireModule(_configuration));
			builder.RegisterModule(new ForecastEventModule(_configuration));
			builder.RegisterModule(new IntradayWebModule(_configuration));
			builder.RegisterModule<StardustModule>();
			builder.RegisterModule(new RequestModule(_configuration));
			builder.RegisterModule( new PersonAccountModule(_configuration));
			builder.RegisterModule(new SchedulingCommonModule(_configuration));
			builder.RegisterModule(new StaffingModule(_configuration));
			builder.RegisterModule(new ShiftTradeModule());
			builder.RegisterModule<CommandDispatcherModule>();
			builder.RegisterModule(new CommandHandlersModule(_configuration));
			builder.RegisterModule(new PeopleAreaModule(_configuration));
			builder.RegisterModule<GamificationAreaModule>();
			builder.RegisterModule(new ReportModule(_configuration));
			builder.RegisterModule(new ApplicationInsightsModule());
			builder.RegisterModule<NotificationModule>();
			builder.RegisterModule(new AuditTrailModule(_configuration));
			builder.RegisterModule<AbsenceModule>();
			builder.RegisterModule<SystemSettingModel>();
		}

		public static IToggleManager ToggleManagerForIoc(IocArgs iocArgs)
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new ToggleManagerModule(iocArgs));
			builder.RegisterModule(new SharedModuleUsedInBothRuntimeContainerAndToggleManagerModule(iocArgs));
			using (var container = builder.Build())
				return container.Resolve<IToggleManager>();
		}
	}
}
