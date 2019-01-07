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
		public static ICurrentUnitOfWork UnitOfWork;
		public static DataMakerImpl DataMaker;
		public static DefaultDataCreator DefaultDataCreator;
		public static DefaultAnalyticsDataCreator DefaultAnalyticsDataCreator;
		public static HangfireUtilities Hangfire;
		public static MutableNow Now;
		public static ITenantUnitOfWork TenantUnitOfWork;
		public static ICurrentTenantSession CurrentTenantSession;

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
			}, arguments => { arguments.AllEventPublishingsAsSync = true; }, null);
			ContainerPlugin.UseContainer(IntegrationIoCTest.Container);
			
			Toggles = IntegrationIoCTest.Container.Resolve<IToggleManager>();
			Now = IntegrationIoCTest.Container.Resolve<INow>() as MutableNow;
			UnitOfWork = IntegrationIoCTest.Container.Resolve<ICurrentUnitOfWork>();
			TenantUnitOfWork = IntegrationIoCTest.Container.Resolve<ITenantUnitOfWork>();
			CurrentTenantSession = IntegrationIoCTest.Container.Resolve<ICurrentTenantSession>();

			DataMaker = IntegrationIoCTest.Container.Resolve<DataMakerImpl>();
			DefaultDataCreator = IntegrationIoCTest.Container.Resolve<DefaultDataCreator>();
			DefaultAnalyticsDataCreator = IntegrationIoCTest.Container.Resolve<DefaultAnalyticsDataCreator>();
		}

		public static void Start()
		{
			IntegrationIoCTest.Container.Resolve<IMessageBrokerUrl>().Configure(TestSiteConfigurationSetup.URL.ToString());
			IntegrationIoCTest.Container.Resolve<ISignalRClient>().StartBrokerService();
			IntegrationIoCTest.Container.Resolve<IHangfireClientStarter>().Start();
			Hangfire = IntegrationIoCTest.Container.Resolve<HangfireUtilities>();
		}
	}
}