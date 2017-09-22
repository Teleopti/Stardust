using System;
using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class InitializeMessageBroker
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(InitializeMessageBroker));
		private readonly IMessageBrokerComposite _messageBroker;

		public InitializeMessageBroker(IMessageBrokerComposite messageBroker)
		{
			_messageBroker = messageBroker;
		}

		public void Start(IDictionary<string, string> appSettings)
		{
			try
			{
				using (PerformanceOutput.ForOperation("Connecting to message broker"))
				{
					string messageBrokerConnection;
					if (appSettings.TryGetValue("MessageBroker", out messageBrokerConnection) && !string.IsNullOrEmpty(messageBrokerConnection))
					{
						Uri serverUrl;
						if (Uri.TryCreate(messageBrokerConnection, UriKind.Absolute, out serverUrl))
						{
							var useLongPolling = appSettings.GetSettingValue("MessageBrokerLongPolling", x => !string.IsNullOrEmpty(x) && bool.Parse(x));
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