using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;
using log4net;
using Teleopti.Ccc.Domain.Helper;
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

		public InitializeApplication(IDataSourcesFactory dataSourcesFactory, IMessageBrokerComposite messageBroker)
		{
			_dataSourcesFactory = dataSourcesFactory;
			_messageBroker = messageBroker;
		}

		// from Web, ServiceBus, ETL, sdk
		public void Start(IState clientCache, string xmlDirectory, ILoadPasswordPolicyService loadPasswordPolicyService,
			IDictionary<string, string> appSettings, bool startMessageBroker)
		{
			StateHolder.Initialize(clientCache);

			IList<IDataSource> dataSources = new List<IDataSource>();
			foreach (string file in Directory.GetFiles(xmlDirectory, "*.nhib.xml"))
			{
				XElement element = XElement.Load(file);
				if (element.Name != "datasource")
				{
					throw new DataSourceException(@"Missing <dataSource> in file " + file);
				}
				IDataSource dataSource;
				if (_dataSourcesFactory.TryCreate(element, out dataSource))
				{
					dataSources.Add(dataSource);
				}
			}
			if (startMessageBroker)
				this.startMessageBroker(appSettings);
			StateHolder.Instance.State.SetApplicationData(
				new ApplicationData(appSettings, new ReadOnlyCollection<IDataSource>(dataSources), _messageBroker,
					loadPasswordPolicyService, _dataSourcesFactory));
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