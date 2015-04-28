using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.WinCode.Common.ServiceBus;
using Teleopti.Ccc.WinCode.Services;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.WinCode.Main
{
	public static class LogonInitializeStateHolder
	{
		public static bool InitWithoutDataSource(LogonModel model, IMessageBrokerComposite messageBroker, SharedSettings settings)
		{
			LoadPasswordPolicyService passwordPolicyService;
			if (settings.PasswordPolicy == null)
			{
				//to be able start desktop app without shared setting server
				passwordPolicyService = new LoadPasswordPolicyService(Environment.CurrentDirectory);
			}
			else
			{
				var passwordPolicyDocument = XDocument.Parse(settings.PasswordPolicy);
				passwordPolicyService = new LoadPasswordPolicyService(passwordPolicyDocument);
			}

			var appsett = ConfigurationManager.AppSettings;
			
			var appSettings = appsett.Keys.Cast<string>().ToDictionary(key => key, key => appsett[key]);

			if (!appsett.AllKeys.Contains("Queue")) appSettings.Add("Queue", settings.Queue);
			if (!appsett.AllKeys.Contains("MessageBroker")) appSettings.Add("MessageBroker", settings.MessageBroker);
			if (!appsett.AllKeys.Contains("MessageBrokerLongPolling")) appSettings.Add("MessageBrokerLongPolling", settings.MessageBrokerLongPolling);
			if (!appsett.AllKeys.Contains("RtaPollingInterval")) appSettings.Add("RtaPollingInterval", settings.RtaPollingInterval);

			bool messageBrokerDisabled = false;
			string messageBrokerDisabledString;
			if (!appSettings.TryGetValue("MessageBroker", out messageBrokerDisabledString) ||
				 string.IsNullOrEmpty(messageBrokerDisabledString))
			{
				messageBrokerDisabled = true;
			}

			var sendToServiceBus = new ServiceBusSender();
			var populator = EventContextPopulator.Make();
			var businessUnit = CurrentBusinessUnit.Make();
			var messageSender = new MessagePopulatingServiceBusSender(sendToServiceBus, populator);
			var eventPublisher = new EventPopulatingPublisher(new ServiceBusEventPublisher(sendToServiceBus), populator);
			var initializer =
				new InitializeApplication(
					new DataSourcesFactory(new EnversConfiguration(),
						new List<IMessageSender>
						{
							new ScheduleMessageSender(eventPublisher, new ClearEvents()),
							new EventsMessageSender(new SyncEventsPublisher(eventPublisher)),
							new MeetingMessageSender(eventPublisher),
							new GroupPageChangedMessageSender(messageSender),
							new TeamOrSiteChangedMessageSender(eventPublisher, businessUnit),
							new PersonChangedMessageSender(eventPublisher, businessUnit),
							new PersonPeriodChangedMessageSender(messageSender)
						}, DataSourceConfigurationSetter.ForDesktop(),
						new CurrentHttpContext(),
						() => messageBroker),
					messageBroker)
				{
					MessageBrokerDisabled = messageBrokerDisabled
				};

			initializer.Start(new StateManager(), appSettings, passwordPolicyService);

			return true;
		}
	}
}