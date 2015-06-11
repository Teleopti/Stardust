using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using log4net;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class InitializeApplication : IInitializeApplication
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (InitializeApplication));
		private readonly IDataSourcesFactory _dataSourcesFactory;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly IReadDataSourceConfiguration _readDataSourceConfiguration;

		public InitializeApplication(IDataSourcesFactory dataSourcesFactory, IMessageBrokerComposite messageBroker, IReadDataSourceConfiguration readDataSourceConfiguration)
		{
			_dataSourcesFactory = dataSourcesFactory;
			_messageBroker = messageBroker;
			_readDataSourceConfiguration = readDataSourceConfiguration;
		}

		// from Web, ServiceBus, ETL, sdk
		//TODO: tenant remove xmlDirectory
		public void Start(IState clientCache, string xmlDirectory, ILoadPasswordPolicyService loadPasswordPolicyService,
			IDictionary<string, string> appSettings, bool startMessageBroker)
		{
			StateHolder.Initialize(clientCache);

			if (startMessageBroker)
				this.startMessageBroker(appSettings);
			var appData = new ApplicationData(appSettings, Enumerable.Empty<IDataSource>(), _messageBroker, loadPasswordPolicyService, _dataSourcesFactory);
			StateHolder.Instance.State.SetApplicationData(appData);
			_readDataSourceConfiguration.Read().ForEach(dsConf =>
			{
				appData.MakeSureDataSourceExists(dsConf.Key, dsConf.Value.ApplicationNHibernateConfig, dsConf.Value.AnalyticsConnectionString);
			});
		}

		//from applicationconfig
		public void Start(IState clientCache,
						  IDictionary<string, string> databaseSettings,
						  string statisticConnectionString,
			IDictionary<string, string> appSettings)
		{
			StateHolder.Initialize(clientCache);
			IDataSource dataSource = _dataSourcesFactory.Create(databaseSettings, statisticConnectionString);
			startMessageBroker(appSettings);
			StateHolder.Instance.State.SetApplicationData(new ApplicationData(appSettings, new[]{dataSource}, _messageBroker, null, _dataSourcesFactory));
		}

		private void startMessageBroker(IDictionary<string, string> appSettings)
		{
			try
			{
				using(PerformanceOutput.ForOperation("Connecting to message broker"))
				{
					string messageBrokerConnection;
					if (appSettings.TryGetValue("MessageBroker", out messageBrokerConnection) && !string.IsNullOrEmpty(messageBrokerConnection))
					{
						Uri serverUrl;
						if (Uri.TryCreate(messageBrokerConnection, UriKind.Absolute, out serverUrl))
						{
							var useLongPolling = appSettings.GetSettingValue("MessageBrokerLongPolling", bool.Parse);
							_messageBroker.ServerUrl = messageBrokerConnection;
							_messageBroker.StartBrokerService(useLongPolling);
						}
					}

					log.Debug("Message broker instantiated");
				}
			}
			catch (BrokerNotInstantiatedException brokerEx)
			{
				log.Warn("Could not start message broker due to: " + brokerEx.InnerException.Message);
			}
		}

		// from desktop
		public void Start(IState clientCache, IDictionary<string, string> appSettings, ILoadPasswordPolicyService loadPasswordPolicyService, bool tryStartMessageBroker)
		{
			StateHolder.Initialize(clientCache);
			if (tryStartMessageBroker)
			{
				startMessageBroker(appSettings);
			}
			StateHolder.Instance.State.SetApplicationData(
				new ApplicationData(appSettings, new List<IDataSource>(), _messageBroker,
										  loadPasswordPolicyService, _dataSourcesFactory));
		}
	}
}