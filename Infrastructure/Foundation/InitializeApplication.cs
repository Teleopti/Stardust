using System;
using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class InitializeApplication : IInitializeApplication
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (InitializeApplication));
		private readonly IMessageBrokerComposite _messageBroker;

		public InitializeApplication(IMessageBrokerComposite messageBroker)
		{
			_messageBroker = messageBroker;
		}

		public void Start(IState clientCache, ILoadPasswordPolicyService loadPasswordPolicyService, IDictionary<string, string> appSettings, bool startMessageBroker)
		{
			StateHolder.Initialize(clientCache);

			if (startMessageBroker)
				this.startMessageBroker(appSettings);
			var appData = new ApplicationData(appSettings, _messageBroker, loadPasswordPolicyService);
			StateHolder.Instance.State.SetApplicationData(appData);
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