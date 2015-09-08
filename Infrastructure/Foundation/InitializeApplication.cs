using System;
using System.Collections.Generic;
using Castle.Core.Internal;
using log4net;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class InitializeApplication : IInitializeApplication
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (InitializeApplication));
		private readonly IDataSourceForTenant _dataSourceForTenant;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly ILoadAllTenants _loadAllTenants;

		public InitializeApplication(IDataSourceForTenant dataSourceForTenant, 
											IMessageBrokerComposite messageBroker,
											ILoadAllTenants loadAllTenants)
		{
			_dataSourceForTenant = dataSourceForTenant;
			_messageBroker = messageBroker;
			_loadAllTenants = loadAllTenants;
		}

		public void Start(IState clientCache, ILoadPasswordPolicyService loadPasswordPolicyService, IDictionary<string, string> appSettings, bool startMessageBroker)
		{
			StateHolder.Initialize(clientCache);

			if (startMessageBroker)
				this.startMessageBroker(appSettings);
			var appData = new ApplicationData(appSettings, _messageBroker, loadPasswordPolicyService, _dataSourceForTenant);
			StateHolder.Instance.State.SetApplicationData(appData);
			_loadAllTenants.Tenants().ForEach(dsConf =>
			{
				_dataSourceForTenant.MakeSureDataSourceExists(dsConf.Name, 
					dsConf.DataSourceConfiguration.ApplicationConnectionString,
					dsConf.DataSourceConfiguration.AnalyticsConnectionString,
					dsConf.DataSourceConfiguration.ApplicationNHibernateConfig);
			});
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
	}
}