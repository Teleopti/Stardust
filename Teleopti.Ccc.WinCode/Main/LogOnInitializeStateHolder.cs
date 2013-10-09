using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Xml.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Sdk.ClientProxies;
using Teleopti.Ccc.WinCode.Common.ServiceBus;
using Teleopti.Ccc.WinCode.Services;
using Teleopti.Messaging.SignalR;
using log4net;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.WinCode.Main
{
    public static class LogonInitializeStateHolder
	{
		public static string ErrorMessage = string.Empty;
		public static string WarningMessage = string.Empty;
        private static readonly ILog Logger = LogManager.GetLogger(typeof (LogonInitializeStateHolder));

		public static bool GetConfigFromFileSystem(string nhibConfPath, bool messageBrokerDisabled)
		{
			new InitializeApplication(
				new DataSourcesFactory(new EnversConfiguration(), new List<IMessageSender>(),
				                       DataSourceConfigurationSetter.ForDesktop()),
				new SignalBroker(MessageFilterManager.Instance))
				{
					MessageBrokerDisabled = messageBrokerDisabled
				}.Start(new StateManager(), nhibConfPath, new LoadPasswordPolicyService(nhibConfPath),
				        new ConfigurationManagerWrapper(), true);
            return true;
        }

		public static bool GetConfigFromWebService(string endpointName)
        {
            ICollection<string> encryptedNHibConfigs;
            IDictionary<string, string> encryptedAppSettings;
            string passwordPolicyString;
			using (var proxy = Proxy.GetProxy(endpointName))
			{
				using (PerformanceOutput.ForOperation("Getting config from web service"))
				{
					try
					{
						encryptedNHibConfigs = proxy.GetHibernateConfigurationInternal();
						encryptedAppSettings = proxy.GetAppSettingsInternal();
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
			if (encryptedNHibConfigs.Count == 0)
                throw new DataSourceException("No NHibernate configurations received");

            var passwordPolicyDocument = XDocument.Parse(passwordPolicyString);
            var passwordPolicyService = new LoadPasswordPolicyService(passwordPolicyDocument);

            encryptedAppSettings.DecryptDictionary(Interfaces.Infrastructure.EncryptionConstants.Image1, Interfaces.Infrastructure.EncryptionConstants.Image2);
            encryptedAppSettings.Add("Sdk", endpointNames);

            bool messageBrokerDisabled = false;
            string messageBrokerDisabledString;
            if (!encryptedAppSettings.TryGetValue("MessageBroker",out messageBrokerDisabledString) ||
                string.IsNullOrEmpty(messageBrokerDisabledString))
            {
                messageBrokerDisabled = true;
            }
        	
			var sendToServiceBus = new ServiceBusSender(new CurrentIdentity());
        	var initializeApplication =
        		new InitializeApplication(
        			new DataSourcesFactory(new EnversConfiguration(),
												  new List<IMessageSender>
												      {
														  new EventsMessageSender(new SyncEventsPublisher(new ServiceBusEventPublisher(sendToServiceBus))),
												          new ScheduleMessageSender(sendToServiceBus), 
                                                          new MeetingMessageSender(sendToServiceBus),
                                                          new GroupPageChangedMessageSender(sendToServiceBus),
                                                          new TeamOrSiteChangedMessageSender(sendToServiceBus),
                                                          new PersonChangedMessageSender(sendToServiceBus),
                                                          new PersonPeriodChangedMessageSender(sendToServiceBus)
												      }, DataSourceConfigurationSetter.ForDesktop()),
        			new SignalBroker(MessageFilterManager.Instance))
        			{
        				MessageBrokerDisabled = messageBrokerDisabled
        			};

            initializeApplication.Start(new StateManager(), encryptedAppSettings,
                                              encryptedNHibConfigs.DecryptList(
                                                  Interfaces.Infrastructure.EncryptionConstants.Image1,
                                                  Interfaces.Infrastructure.EncryptionConstants.Image2),
                                              passwordPolicyService);

            if (initializeApplication.UnavailableDataSources.Any())
            {
                WarningMessage = UserTexts.Resources.ErrorOccuredWhenAccessingTheDataSource;
                foreach (var unavailableDataSource in initializeApplication.UnavailableDataSources)
                {
                    WarningMessage = string.Concat(WarningMessage, Environment.NewLine, unavailableDataSource);
                }
            }

            return true;
        }
    }
}