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
using System.Xml.XPath;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class InitializeApplication : IInitializeApplication
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (InitializeApplication));
	    
		public InitializeApplication(IDataSourcesFactory dataSourcesFactory, IMessageBrokerComposite messageBroker)
		{
			DataSourcesFactory = dataSourcesFactory;
			MessageBroker = messageBroker;
			MessageBrokerDisabled = false;
		}

		public IDataSourcesFactory DataSourcesFactory { get; private set; }
		public IMessageBrokerComposite MessageBroker { get; private set; }
		public bool MessageBrokerDisabled { get; set; }

		// from LogonInitializeStateHolder
		public void Start(IState clientCache, IDictionary<string, string> appSettings,
						  IDataSource dataSource,
						  ILoadPasswordPolicyService loadPasswordPolicyService)
	    {
	    	StateHolder.Initialize(clientCache);

	    	IList<IDataSource> dataSources = new List<IDataSource>{dataSource};
	    	
	    	StartMessageBroker(appSettings);
	    	StateHolder.Instance.State.SetApplicationData(
	    		new ApplicationData(appSettings, new List<IDataSource>(dataSources), MessageBroker,
	    		                    loadPasswordPolicyService, DataSourcesFactory));
	    }

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
				if (DataSourcesFactory.TryCreate(element, out dataSource))
				{
					dataSource.AuthenticationTypeOption = AuthenticationTypeOption.Application | AuthenticationTypeOption.Windows;
					dataSources.Add(dataSource);
				}
			}
			var appSettings = configurationWrapper.AppSettings;
			if (startMessageBroker)
				StartMessageBroker(appSettings);
			StateHolder.Instance.State.SetApplicationData(
				new ApplicationData(appSettings, new ReadOnlyCollection<IDataSource>(dataSources), MessageBroker,
					loadPasswordPolicyService, DataSourcesFactory));
		}

		//from applicationconfig
		public void Start(IState clientCache,
						  IDictionary<string, string> settings,
						  string statisticConnectionString,
			IConfigurationWrapper configurationWrapper)
		{
			StateHolder.Initialize(clientCache);
			IDataSource dataSource = DataSourcesFactory.Create(settings, statisticConnectionString);
			var appSettings = configurationWrapper.AppSettings;
			StartMessageBroker(appSettings);
			StateHolder.Instance.State.SetApplicationData(new ApplicationData(appSettings, new[]{dataSource},MessageBroker, null, DataSourcesFactory));
		}

		private void StartMessageBroker(IDictionary<string, string> appSettings)
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
							MessageBroker.ServerUrl = messageBrokerConnection;
							MessageBroker.StartBrokerService(useLongPolling);
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
	}
}