using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using log4net;
using NHibernate.Cfg.ConfigurationSchema;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Exceptions;
using System.Xml.XPath;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	/// <summary>
	/// Initialize helper for application
	/// Run Start method once
	/// </summary>
	public class InitializeApplication : IInitializeApplication
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (InitializeApplication));
        private readonly IList<string> _unavailableDataSources = new List<string>();
	    
		public InitializeApplication(IDataSourcesFactory dataSourcesFactory, IMessageBroker messageBroker)
		{
			DataSourcesFactory = dataSourcesFactory;
			MessageBroker = messageBroker;
			MessageBrokerDisabled = false;
		}

		public IDataSourcesFactory DataSourcesFactory { get; private set; }
		public IMessageBroker MessageBroker { get; private set; }
		public bool MessageBrokerDisabled { get; set; }
	    
        public IEnumerable<string> UnavailableDataSources
	    {
	        get { return _unavailableDataSources; }
	    }

	    /// <summary>
		/// Setup the application. Should be run once per application.
		/// Is _not_ thread safe. It's the client's responsibility.
		/// </summary>
		/// <param name="clientCache">The client cache.</param>
		/// <param name="appSettings">The application settings</param>
		/// <param name="hibernateConfigurations">The nhibernate configurations.</param>
		/// <param name="loadPasswordPolicyService">The password policy loading service</param>
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
	    		                    loadPasswordPolicyService));
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

	    /// <summary>
		/// Gets application settings.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// You can get the application settings from two sources, either locally from the application config file, or fetch them over the web service.
		/// To decide the source of the settings, make sure that the "GetConfigFromWebService" entry is "false" in in the appsettings section 
		/// in the app.config file.
		/// </remarks>
		private static IDictionary<string, string> GetAppSettings()
		{
			IDictionary<string, string> appSettings = new Dictionary<string, string>();
			ConfigurationManager.AppSettings.AllKeys.ToList().ForEach(name => appSettings.Add(name, ConfigurationManager.AppSettings[name]));
			var published = (NameValueCollection)ConfigurationManager.GetSection("teleopti/publishedSettings");
			
			if (published != null)
			{
				foreach (string item in published)
				{
					appSettings.Add(item, published.Get(item));
				}
			}
			return appSettings;
		}

		/// <summary>
		/// Setup the application. Should be run once per application.
		/// Is _not_ thread safe. It's the client's responsibility.
		/// </summary>
		/// <param name="clientCache">The client cache.</param>
		/// <param name="xmlDirectory">The directory to nhibernate's conf file(s)</param>
		/// <param name="loadPasswordPolicyService">The password policy loading service</param>
		public void Start(IState clientCache, string xmlDirectory, ILoadPasswordPolicyService loadPasswordPolicyService)
		{
			StateHolder.Initialize(clientCache);

			IList<IDataSource> dataSources = new List<IDataSource>();
			foreach (string file in Directory.GetFiles(xmlDirectory, "*.nhib.xml"))
			{
				IDataSource dataSource;
				if (DataSourcesFactory.TryCreate(file, out dataSource))
				{
					dataSource.AuthenticationTypeOption = AuthenticationTypeOption.Application | AuthenticationTypeOption.Windows;
					dataSource.OriginalFileName = file;
					dataSources.Add(dataSource);
				}
			}
			var appSettings = GetAppSettings();
			StartMessageBroker(appSettings);
			StateHolder.Instance.State.SetApplicationData(
				new ApplicationData(appSettings, new ReadOnlyCollection<IDataSource>(dataSources), MessageBroker, loadPasswordPolicyService));
		}

		/// <summary>
		/// This overload is mainly used by the DatabaseConverter.
		/// Setup the application. Should be run once per application.
		/// Is _not_ thread safe. It's the client's responsibility.
		/// </summary>
		/// <param name="clientCache">The client cache.</param>
		/// <param name="settings">The settings.</param>
		/// <param name="statisticConnectionString">The statistic connectionstring.</param>
		public void Start(IState clientCache,
						  IDictionary<string, string> settings,
						  string statisticConnectionString)
		{
			StateHolder.Initialize(clientCache);
			IDataSource datasources = DataSourcesFactory.Create(settings, statisticConnectionString);
			var appSettings = GetAppSettings();
			StartMessageBroker(appSettings);
			StateHolder.Instance.State.SetApplicationData(new ApplicationData(appSettings, datasources,
																			  MessageBroker));
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
							MessageBroker = new SignalBroker(((MessageBrokerBase)MessageBroker).FilterManager.FilterDictionary);
						}
						MessageBroker.ConnectionString = messageBrokerConnection;
					}

					MessageBroker.StartMessageBroker();
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