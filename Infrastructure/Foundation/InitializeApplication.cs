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
	    
		public InitializeApplication(IDataSourcesFactory dataSourcesFactory, IMessageBrokerComposite messageBroker)
		{
			this.dataSourcesFactory = dataSourcesFactory;
			this.messageBroker = messageBroker;
			MessageBrokerDisabled = false;
		}

		private IDataSourcesFactory dataSourcesFactory { get; set; }
		private IMessageBrokerComposite messageBroker { get; set; }
		public bool MessageBrokerDisabled { get; set; }

		// from Web, ServiceBus, Sdk, ETL, LogonInitializeStateHolder
		public void Start(IState clientCache, string xmlDirectory, ILoadPasswordPolicyService loadPasswordPolicyService,
			IConfigurationWrapper configurationWrapper, bool startMessageBroker)
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
				if (dataSourcesFactory.TryCreate(element, out dataSource))
				{
					dataSource.AuthenticationTypeOption = AuthenticationTypeOption.Application | AuthenticationTypeOption.Windows;
					dataSources.Add(dataSource);
				}
			}
			var appSettings = configurationWrapper.AppSettings;
			if (startMessageBroker)
				this.startMessageBroker(appSettings);
			StateHolder.Instance.State.SetApplicationData(
				new ApplicationData(appSettings, new ReadOnlyCollection<IDataSource>(dataSources), messageBroker,
					loadPasswordPolicyService, dataSourcesFactory));
		}

		//from applicationconfig
		public void Start(IState clientCache,
						  IDictionary<string, string> settings,
						  string statisticConnectionString,
			IConfigurationWrapper configurationWrapper)
		{
			StateHolder.Initialize(clientCache);
			IDataSource dataSource = dataSourcesFactory.Create(settings, statisticConnectionString);
			var appSettings = configurationWrapper.AppSettings;
			startMessageBroker(appSettings);
			StateHolder.Instance.State.SetApplicationData(new ApplicationData(appSettings, new[]{dataSource},messageBroker, null, dataSourcesFactory));
		}

		private void startMessageBroker(IDictionary<string, string> appSettings)
		{
			if (MessageBrokerDisabled)
			{
				log.Debug("Message broker disabled.");
				return;
			}
			try
			{
				using(PerformanceOutput.ForOperation("Connecting to message broker"))
				{
					string messageBrokerConnection;
					if (appSettings.TryGetValue("MessageBroker", out messageBrokerConnection))
					{
						Uri serverUrl;
						if (Uri.TryCreate(messageBrokerConnection, UriKind.Absolute, out serverUrl))
						{
							var useLongPolling = appSettings.GetSettingValue("MessageBrokerLongPolling", bool.Parse);
							messageBroker.ServerUrl = messageBrokerConnection;
							messageBroker.StartBrokerService(useLongPolling);
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

		// from LogonInitializeStateHolder
		public void Start(IState clientCache, IDictionary<string, string> appSettings, ILoadPasswordPolicyService loadPasswordPolicyService)
		{
			StateHolder.Initialize(clientCache);

			startMessageBroker(appSettings);
			StateHolder.Instance.State.SetApplicationData(
				new ApplicationData(appSettings, new List<IDataSource>(), messageBroker,
										  loadPasswordPolicyService, dataSourcesFactory));
		}
	}
}