using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class SystemSetup
	{
		private static IContainer _container;

		public static IToggleManager Toggles;
		public static ICurrentTransactionHooks TransactionHooks;
		public static ICurrentUnitOfWorkFactory UnitOfWorkFactory;
		public static ICurrentUnitOfWork UnitOfWork;
		public static DefaultDataCreator DefaultDataCreator;
		public static DefaultAnalyticsDataCreator DefaultAnalyticsDataCreator;
		public static IHangfireUtilities Hangfire;
		public static MutableNow Now;
		public static IEventPublisher EventPublisher;
		public static ITenantUnitOfWork TenantUnitOfWork;
		public static ICurrentTenantSession CurrentTenantSession;

		public static void Setup()
		{
			var builder = new ContainerBuilder();
			var args = new IocArgs(new ConfigReader())
			{
				AllEventPublishingsAsSync = true,
				FeatureToggle = TestSiteConfigurationSetup.URL.ToString()
			};
			var iocConfiguration = new IocConfiguration(args, CommonModule.ToggleManagerForIoc(args));
			builder.RegisterModule(new CommonModule(iocConfiguration));
			builder.RegisterType<DefaultDataCreator>().SingleInstance();
			builder.RegisterModule(new TenantServerModule(iocConfiguration));

			_container = builder.Build();

			Toggles = _container.Resolve<IToggleManager>();
			Now = _container.Resolve<INow>() as MutableNow;
			TransactionHooks = _container.Resolve<ICurrentTransactionHooks>();
			UnitOfWorkFactory = _container.Resolve<ICurrentUnitOfWorkFactory>();
			UnitOfWork = _container.Resolve<ICurrentUnitOfWork>();
			EventPublisher = _container.Resolve<IEventPublisher>();
			TenantUnitOfWork = _container.Resolve<ITenantUnitOfWork>();
			CurrentTenantSession = _container.Resolve<ICurrentTenantSession>();

			DefaultDataCreator = _container.Resolve<DefaultDataCreator>();
			DefaultAnalyticsDataCreator = new DefaultAnalyticsDataCreator();
		}

		public static void Start()
		{
			_container.Resolve<IMessageBrokerUrl>().Configure(TestSiteConfigurationSetup.URL.ToString());
			_container.Resolve<ISignalRClient>().StartBrokerService();
			_container.Resolve<HangfireClientStarter>().Start();
			Hangfire = _container.Resolve<IHangfireUtilities>();
		}
	}
}