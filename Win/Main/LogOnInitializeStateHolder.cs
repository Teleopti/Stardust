﻿#region Imports

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Deployment.Application;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Xml.Linq;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Win.Forecasting.Forms.ImportForecast;
using Teleopti.Ccc.Win.Payroll.Forms.PayrollExportPages;
using Teleopti.Messaging.SignalR;
using log4net;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.ClientProxies;
using Teleopti.Ccc.Win.Services;
using Teleopti.Messaging.Composites;

#endregion

namespace Teleopti.Ccc.Win.Main
{
    public static class LogOnInitializeStateHolder
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (LogOnInitializeStateHolder));

        /// <summary>
        /// Initializes the state holder.
        /// </summary>
        /// <remarks>
        /// You can get the application settings from two sources, either locally from the application config file, or fetch them over the web service.
        /// To decide the source of the settings, make sure that the "GetConfigFromWebService" entry is "false" in in the appsettings section 
        /// in the app.config file.
        /// </remarks>
        public static bool InitializeStateHolder()
        {
            ErrorMessage = string.Empty;
            WarningMessage = string.Empty;
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["GetConfigFromWebService"],
                                  CultureInfo.InvariantCulture))
            {
                return GetConfigFromWebService();
            }
            
            return GetConfigFromFileSystem();
        }

        private static bool GetConfigFromFileSystem()
        {
            string nhibConfPath;

            if (ApplicationDeployment.IsNetworkDeployed)
                nhibConfPath = ApplicationDeployment.CurrentDeployment.DataDirectory;
            else
                nhibConfPath = ConfigurationManager.AppSettings["nhibConfPath"];

            if (nhibConfPath == null)
                nhibConfPath = Directory.GetCurrentDirectory();

        	bool messageBrokerDisabled = string.IsNullOrEmpty(ConfigurationManager.AppSettings["MessageBroker"]);

        	new InitializeApplication(new DataSourcesFactory(new EnversConfiguration(), new List<IMessageSender>(), DataSourceConfigurationSetter.ForDesktop()),
				new SignalBroker(MessageFilterManager.Instance.FilterDictionary))
				{
					MessageBrokerDisabled = messageBrokerDisabled
				}.Start(new StateManager(), nhibConfPath, new LoadPasswordPolicyService(nhibConfPath), new ConfigurationManagerWrapper());
            return true;
        }

        public static string ErrorMessage = string.Empty;
        public static string WarningMessage = string.Empty;

        private static bool GetConfigFromWebService()
        {
            ICollection<string> encryptedNHibConfigs;
            IDictionary<string, string> encryptedAppSettings;
            string passwordPolicyString;
            using (Proxy proxy = new Proxy())
            {
                using(PerformanceOutput.ForOperation("Getting config from web service"))
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
                        Logger.Error("Configuration could not be retrieved from server.",exception);
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

            bool messageBrokerDisabled = false;
            string messageBrokerDisabledString;
            if (!encryptedAppSettings.TryGetValue("MessageBroker",out messageBrokerDisabledString) ||
                string.IsNullOrEmpty(messageBrokerDisabledString))
            {
                messageBrokerDisabled = true;
            }
        	var sendDenormalizeNotification = new SendDenormalizeNotificationToSdk(new SendCommandToSdk(new SdkAuthentication()));
        	var saveToDenormalizationQueue = new SaveToDenormalizationQueue();
        	var initializeApplication =
        		new InitializeApplication(
        			new DataSourcesFactory(new EnversConfiguration(),
												  new List<IMessageSender>
												      {
												          new ScheduleMessageSender(sendDenormalizeNotification, saveToDenormalizationQueue), 
                                                          new MeetingMessageSender(sendDenormalizeNotification, saveToDenormalizationQueue),
                                                          new GroupPageChangedMessageSender(sendDenormalizeNotification, saveToDenormalizationQueue ),
                                                          new PersonChangedMessageSender(sendDenormalizeNotification, saveToDenormalizationQueue ),
                                                          new PersonPeriodChangedMessageSender(sendDenormalizeNotification, saveToDenormalizationQueue )
												      }, DataSourceConfigurationSetter.ForDesktop()),
        			new SignalBroker(MessageFilterManager.Instance.FilterDictionary))
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