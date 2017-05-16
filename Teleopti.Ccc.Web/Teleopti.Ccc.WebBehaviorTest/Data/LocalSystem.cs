using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class LocalSystem
	{
		public static IToggleManager Toggles;
		public static ICurrentTransactionHooks TransactionHooks;
		public static ICurrentUnitOfWorkFactory UnitOfWorkFactory;
		public static ICurrentUnitOfWork UnitOfWork;
		public static DefaultDataCreator DefaultDataCreator;
		public static DefaultAnalyticsDataCreator DefaultAnalyticsDataCreator;
		public static HangfireUtilities Hangfire;
		public static MutableNow Now;
		public static IEventPublisher EventPublisher;
		public static ITenantUnitOfWork TenantUnitOfWork;
		public static ICurrentTenantSession CurrentTenantSession;
		public static StateQueueUtilities StateQueue;

		public static void Setup()
		{
			IntegrationIoCTest.Setup();

			Toggles = IntegrationIoCTest.Container.Resolve<IToggleManager>();
			Now = IntegrationIoCTest.Container.Resolve<INow>() as MutableNow;
			TransactionHooks = IntegrationIoCTest.Container.Resolve<ICurrentTransactionHooks>();
			UnitOfWorkFactory = IntegrationIoCTest.Container.Resolve<ICurrentUnitOfWorkFactory>();
			UnitOfWork = IntegrationIoCTest.Container.Resolve<ICurrentUnitOfWork>();
			EventPublisher = IntegrationIoCTest.Container.Resolve<IEventPublisher>();
			TenantUnitOfWork = IntegrationIoCTest.Container.Resolve<ITenantUnitOfWork>();
			CurrentTenantSession = IntegrationIoCTest.Container.Resolve<ICurrentTenantSession>();

			DefaultDataCreator = IntegrationIoCTest.Container.Resolve<DefaultDataCreator>();
			DefaultAnalyticsDataCreator = IntegrationIoCTest.Container.Resolve<DefaultAnalyticsDataCreator>();

			StateQueue = IntegrationIoCTest.Container.Resolve<StateQueueUtilities>();
		}

		public static void Start()
		{
			IntegrationIoCTest.Container.Resolve<IMessageBrokerUrl>().Configure(TestSiteConfigurationSetup.URL.ToString());
			IntegrationIoCTest.Container.Resolve<ISignalRClient>().StartBrokerService();
			IntegrationIoCTest.Container.Resolve<HangfireClientStarter>().Start();
			Hangfire = IntegrationIoCTest.Container.Resolve<HangfireUtilities>();
		}
	}
}