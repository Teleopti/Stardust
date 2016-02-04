using Autofac;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class SystemSetup
	{
		private static IContainer _container;

		public static ICurrentPersistCallbacks PersistCallbacks;
		public static HangfireUtilties Hangfire;
		public static MutableNow Now;

		public static void Setup()
		{
			var builder = new ContainerBuilder();
			var args = new IocArgs(new ConfigReader())
			{
				BehaviorTestClient = true,
				FeatureToggle = "http://notinuse"
			};
			// should really use same toggles as the website!
			var toggleManager = new FakeToggleManager();
			toggleManager.Enable(Toggles.RTA_NewEventHangfireRTA_34333);
			toggleManager.Enable(Toggles.RTA_AdherenceDetails_34267);
			toggleManager.Enable(Toggles.RTA_DeletedPersons_36041);
			toggleManager.Enable(Toggles.RTA_TerminatedPersons_36042);
			builder.RegisterModule(new CommonModule(new IocConfiguration(args, toggleManager)));
			builder.RegisterInstance(toggleManager).As<IToggleManager>();

			_container = builder.Build();

			Now = _container.Resolve<INow>() as MutableNow;
			PersistCallbacks = _container.Resolve<ICurrentPersistCallbacks>();
		}

		public static void Start()
		{
			_container.Resolve<IMessageBrokerUrl>().Configure(TestSiteConfigurationSetup.URL.ToString());
			_container.Resolve<ISignalRClient>().StartBrokerService();
			_container.Resolve<IHangfireClientStarter>().Start();
			Hangfire = _container.Resolve<HangfireUtilties>();
		}
	}
}