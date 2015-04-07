using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Xml.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Sdk.ClientProxies;
using Teleopti.Ccc.WinCode.Common.ServiceBus;
using Teleopti.Ccc.WinCode.Services;
using log4net;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.WinCode.Main
{
	public static class LogonInitializeStateHolder
	{
		public static string ErrorMessage = string.Empty;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(LogonInitializeStateHolder));

		public static bool InitWithoutDataSource(ILogonModel model, IMessageBrokerComposite messageBroker, SharedSettings settings)
		{
			string passwordPolicyString;
			using (var proxy = Proxy.GetProxy(model.SelectedSdk))
			{
				using (PerformanceOutput.ForOperation("Getting config from web service"))
				{
					try
					{
						passwordPolicyString = proxy.GetPasswordPolicy();
					}
					catch (TimeoutException timeoutException)
					{
						Logger.Error("Configuration could not be retrieved from due to a timeout.", timeoutException);
						ErrorMessage = timeoutException.Message;
						return false;
					}
					catch (CommunicationException exception)
					{
						Logger.Error("Configuration could not be retrieved from server.", exception);
						ErrorMessage = exception.Message;
						return false;
					}
				}
			}

			var passwordPolicyDocument = XDocument.Parse(passwordPolicyString);
			var passwordPolicyService = new LoadPasswordPolicyService(passwordPolicyDocument);

			var appsett = ConfigurationManager.AppSettings;
			
			var appSettings = appsett.Keys.Cast<string>().ToDictionary(key => key, key => appsett[key]);
			appSettings.Add("Sdk", model.SelectedSdk);

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
							new TeamOrSiteChangedMessageSender(messageSender),
							new PersonChangedMessageSender(messageSender),
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

		public static bool GetConfigFromFileSystem(string nhibConfPath, bool messageBrokerDisabled, IMessageBrokerComposite messageBroker)
		{
			new InitializeApplication(
				new DataSourcesFactory(new EnversConfiguration(), new List<IMessageSender>(),
										DataSourceConfigurationSetter.ForDesktop(), new CurrentHttpContext(), () => messageBroker),
				messageBroker
				)
			{
				MessageBrokerDisabled = messageBrokerDisabled
			}.Start(new StateManager(), nhibConfPath, new LoadPasswordPolicyService(nhibConfPath),
						  new ConfigurationManagerWrapper(), true);
			return true;
		}
	}
}