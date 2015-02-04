using System.Collections.Generic;
using System.Threading;
using Autofac;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using IMessageSender = Teleopti.Ccc.Infrastructure.UnitOfWork.IMessageSender;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestDataSetup
	{
		private static IDataSource datasource;
		private static readonly DefaultData globalData = new DefaultData();

		public static void Setup()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs { PublishEventsToServiceBus = false }, new FalseToggleManager())));
			builder.RegisterType<EventsMessageSender>().As<IMessageSender>().SingleInstance();
			var container = builder.Build();

			datasource = DataSourceHelper.CreateDataSource(container.Resolve<IEnumerable<IMessageSender>>(), "TestData");
			TestSiteConfigurationSetup.StartApplicationAsync();

			StateHolderProxyHelper.SetupFakeState(datasource, DefaultPersonThatCreatesDbData.PersonThatCreatesDbData, DefaultBusinessUnit.BusinessUnitFromFakeState, new ThreadPrincipalContext(new TeleoptiPrincipalFactory()));
			GlobalPrincipalState.Principal = Thread.CurrentPrincipal as TeleoptiPrincipal;
			GlobalUnitOfWorkState.CurrentUnitOfWorkFactory = UnitOfWorkFactory.CurrentUnitOfWorkFactory();

			globalData.ForEach(dataSetup => GlobalDataMaker.Data().Apply(dataSetup));
			DataSourceHelper.BackupCcc7Database(globalData.HashValue);

			container.Resolve<IMessageBrokerUrl>().Configure(TestSiteConfigurationSetup.URL.ToString());
			container.Resolve<ISignalRClient>().StartBrokerService();
		}

		public static void ClearAnalyticsData()
		{
			DataSourceHelper.ClearAnalyticsData();
		}

		public static void RestoreCcc7Data()
		{
			TestControllerMethods.ClearConnections();
			DataSourceHelper.RestoreCcc7Database(globalData.HashValue);
		}

	}
}