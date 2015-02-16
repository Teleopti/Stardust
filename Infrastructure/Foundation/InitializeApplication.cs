using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using log4net;
using NHibernate.Cfg.ConfigurationSchema;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Messaging.Exceptions;
using System.Xml.XPath;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	/// <summary>
	/// Configure helper for application
	/// Run Start method once
	/// </summary>
	public class InitializeApplication : IInitializeApplication
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (InitializeApplication));
        private readonly IList<string> _unavailableDataSources = new List<string>();
	    
		public InitializeApplication(IDataSourcesFactory dataSourcesFactory, IMessageBrokerComposite messageBroker)
		{
			DataSourcesFactory = dataSourcesFactory;
			MessageBroker = messageBroker;
			MessageBrokerDisabled = false;
		}

		public IDataSourcesFactory DataSourcesFactory { get; private set; }
		public IMessageBrokerComposite MessageBroker { get; private set; }
		public bool MessageBrokerDisabled { get; set; }
	    
        public IEnumerable<string> UnavailableDataSources
	    {
	        get { return _unavailableDataSources; }
	    }

		public void Start(IState clientCache, IDictionary<string, string> appSettings,
						  IEnumerable<string> hibernateConfigurations,
						  ILoadPasswordPolicyService loadPasswordPolicyService)
	    {
	    	StateHolder.Initialize(clientCache);

	    	IList<IDataSource> dataSources = new List<IDataSource>();
	    	foreach (string nhibConfig in hibernateConfigurations)
	    	{
	    		var element = XElement.Parse(nhibConfig);
	    		IDataSource dataSource;
	    		var success = DataSourcesFactory.TryCreate(element, out dataSource);
	    		if (success)
	    		{
	    			var children = element.CreateNavigator().Select("authenticationType");
	    			foreach (XPathItem child in children)
	    			{
	    				dataSource.AuthenticationTypeOption |=
	    					(AuthenticationTypeOption) Enum.Parse(typeof (AuthenticationTypeOption), child.Value);
	    			}
	    			dataSources.Add(dataSource);
	    		}
	    		if (!success)
	    		{
	    			string name = extractName(element);
	    			_unavailableDataSources.Add(name);
	    		}
	    	}

	    	StartMessageBroker(appSettings);
	    	StateHolder.Instance.State.SetApplicationData(
	    		new ApplicationData(appSettings, new ReadOnlyCollection<IDataSource>(dataSources), MessageBroker,
	    		                    loadPasswordPolicyService, DataSourcesFactory));
	    }

		public void Start(IState clientCache, IDictionary<string, string> appSettings,
						  ILoadPasswordPolicyService loadPasswordPolicyService)
		{
			StateHolder.Initialize(clientCache);
			
			StartMessageBroker(appSettings);
			StateHolder.Instance.State.SetApplicationData(
				new ApplicationData(appSettings, Enumerable.Empty<IDataSource>(), MessageBroker,
										  loadPasswordPolicyService,DataSourcesFactory));
		}

		private static string extractName(XElement element)
	    {
	        var navigator = element.CreateNavigator();
            XPathNavigator navigator2 = navigator.SelectSingleNode(CfgXmlHelper.SessionFactoryExpression);
            if ((navigator2 != null) && navigator2.MoveToFirstAttribute())
            {
                return navigator2.Value;
            }
	        return string.Empty;
	    }

		public void Start(IState clientCache, string xmlDirectory, ILoadPasswordPolicyService loadPasswordPolicyService, IConfigurationWrapper configurationWrapper, bool startMessageBroker)
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
					dataSource.OriginalFileName = file;
					dataSources.Add(dataSource);
				}
			}
			var appSettings = configurationWrapper.AppSettings;
			if (startMessageBroker)
				StartMessageBroker(appSettings);
			StateHolder.Instance.State.SetApplicationData(
				new ApplicationData(appSettings, new ReadOnlyCollection<IDataSource>(dataSources), MessageBroker, loadPasswordPolicyService, DataSourcesFactory));
		}

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