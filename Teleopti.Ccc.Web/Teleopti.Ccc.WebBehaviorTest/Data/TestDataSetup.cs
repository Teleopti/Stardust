using System.Threading;
using Autofac;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestDataSetup
	{
		private static IDataSource datasource;
		private static readonly DefaultData globalData = new DefaultData();

		public static void Setup()
		{

			SystemSetup.Setup();

			datasource = DataSourceHelper.CreateDataSource(SystemSetup.PersistCallbacks, UserConfigurable.DefaultTenantName);

			TestSiteConfigurationSetup.StartApplicationAsync();

			StateHolderProxyHelper.SetupFakeState(
				datasource,
				DefaultPersonThatCreatesDbData.PersonThatCreatesDbData,
				DefaultBusinessUnit.BusinessUnitFromFakeState,
				new ThreadPrincipalContext(new TeleoptiPrincipalFactory())
				);
			GlobalPrincipalState.Principal = Thread.CurrentPrincipal as TeleoptiPrincipal;
			GlobalUnitOfWorkState.CurrentUnitOfWorkFactory = UnitOfWorkFactory.CurrentUnitOfWorkFactory();

			globalData.ForEach(dataSetup => GlobalDataMaker.Data().Apply(dataSetup));
			DataSourceHelper.BackupCcc7Database(globalData.HashValue);

			SystemSetup.MessageBrokerUrl.Configure(TestSiteConfigurationSetup.URL.ToString());
			SystemSetup.SignalRClient.StartBrokerService();
			SystemSetup.HangfireClient.Start();
		}

		public static void ClearAnalyticsData()
		{
			DataSourceHelper.ClearAnalyticsData();
		}

		public static void RestoreCcc7Data()
		{
			DataSourceHelper.RestoreCcc7Database(globalData.HashValue);
		}
	}

	public static class SystemSetup
	{
		private static IContainer _container;

		public static ICurrentPersistCallbacks PersistCallbacks;
		public static IMessageBrokerUrl MessageBrokerUrl;
		public static ISignalRClient SignalRClient;
		public static IHangfireClientStarter HangfireClient;
		public static HangfireUtilties Hangfire;

		public static void Setup()
		{
			var builder = new ContainerBuilder();
			var args = new IocArgs(new ConfigReader())
			{
				BehaviorTest = true,
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

			PersistCallbacks = _container.Resolve<ICurrentPersistCallbacks>();
			MessageBrokerUrl = _container.Resolve<IMessageBrokerUrl>();
			SignalRClient = _container.Resolve<ISignalRClient>();
			HangfireClient = _container.Resolve<IHangfireClientStarter>();
			Hangfire = _container.Resolve<HangfireUtilties>();
		}
	}
}
