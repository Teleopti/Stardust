using System;
using System.Linq;
using Autofac;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.WebBehaviorTest.SpecFlowPlugin;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class LocalSystem
	{
		public static IToggleManager Toggles;
		public static ICurrentUnitOfWorkFactory UnitOfWorkFactory;
		public static ICurrentUnitOfWork UnitOfWork;
		public static ISetupResolver SetupResolver;
		public static DataMakerImpl DataMaker;
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
			IntegrationIoCTest.Setup(builder =>
			{
				builder.RegisterTypes(typeof(LocalSystem).Assembly
					.GetTypes()
					.Where(t => Attribute.IsDefined(t, typeof(BindingAttribute)))
					.ToArray()
				).SingleInstance();
				builder.RegisterType<DataMakerImpl>().SingleInstance();
				builder.RegisterType<ScenarioDataFactory>().InstancePerDependency();
			}, null);
			ContainerPlugin.UseContainer(IntegrationIoCTest.Container);

			Toggles = IntegrationIoCTest.Container.Resolve<IToggleManager>();
			Now = IntegrationIoCTest.Container.Resolve<INow>() as MutableNow;
			UnitOfWorkFactory = IntegrationIoCTest.Container.Resolve<ICurrentUnitOfWorkFactory>();
			UnitOfWork = IntegrationIoCTest.Container.Resolve<ICurrentUnitOfWork>();
			EventPublisher = IntegrationIoCTest.Container.Resolve<IEventPublisher>();
			TenantUnitOfWork = IntegrationIoCTest.Container.Resolve<ITenantUnitOfWork>();
			CurrentTenantSession = IntegrationIoCTest.Container.Resolve<ICurrentTenantSession>();

			SetupResolver = IntegrationIoCTest.Container.Resolve<ISetupResolver>();
			DataMaker = IntegrationIoCTest.Container.Resolve<DataMakerImpl>();
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