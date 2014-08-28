using System;
using System.Collections.Generic;
using System.ServiceModel;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using log4net;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.ClientProxies;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class SdkConfigurationReader : IConfigurationReader
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SdkConfigurationReader));
        private static readonly object LockObject = new object();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "NHibernate"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void ReadConfiguration(MessageSenderCreator creator)
        {
            lock (LockObject)
            {
                if (StateHolderReader.IsInitialized)
                {
                    Logger.Info("StateHolder already initialized. This step is skipped.");
                    return;
                }

                //This should probably go into a host of some kind instead
                ICollection<string> encryptedNHibConfigs;
                IDictionary<string, string> encryptedAppSettings;
                using (var proxy = new Proxy())
                {
                    try
                    {
                        encryptedNHibConfigs = proxy.GetHibernateConfigurationInternal();
                        encryptedAppSettings = proxy.GetAppSettingsInternal();

                    }
                    catch (CommunicationException exception)
                    {
                        Logger.Error("Configuration could not be retrieved from server.", exception);
                        encryptedAppSettings = new Dictionary<string, string>(0);
                        encryptedNHibConfigs = new List<string>(0);
                    }
                }
                if (encryptedNHibConfigs.Count == 0)
                    throw new DataSourceException(
                        "No NHibernate configurations received. Verify that the SDK is up and running.");

            	
                encryptedAppSettings.DecryptDictionary(EncryptionConstants.Image1, EncryptionConstants.Image2);
				MessageBrokerContainer.Configure(null, MessageFilterManager.Instance);
            	var application =
            		new InitializeApplication(
            			new DataSourcesFactory(new EnversConfiguration(), creator.Create(), DataSourceConfigurationSetter.ForServiceBus()),
						MessageBrokerContainer.CompositeClient());
                application.Start(new BasicState(), encryptedAppSettings,
                                  encryptedNHibConfigs.DecryptList(EncryptionConstants.Image1,
                                                                   EncryptionConstants.Image2), null);

                Logger.Info("Initialized application");
            }
        }
    }

	public class InternalServiceBusSender : IServiceBusSender
	{
		private readonly Func<IServiceBus> _serviceBus;
        
		public InternalServiceBusSender(Func<IServiceBus> serviceBus)
		{
		    _serviceBus = serviceBus;
		}

	    public void Dispose()
		{
		}

		public void Send(object message)
		{
			_serviceBus().Send(message);
		}

		public bool EnsureBus()
		{
			return true;
		}
	}

	public class MessageSenderCreator
	{
		private readonly IServiceBusSender _serviceBusSender;

		public MessageSenderCreator(IServiceBusSender serviceBusSender)
		{
			_serviceBusSender = serviceBusSender;
		}

		public IList<IMessageSender> Create()
		{
			var sender = _serviceBusSender;
			var eventPublisher = new ServiceBusEventPublisher(sender, new EventContextPopulator(new CurrentIdentity(), new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make())));
			return new List<IMessageSender>
				{
					new ScheduleMessageSender(eventPublisher, new ClearEvents()),
					new EventsMessageSender(new SyncEventsPublisher(eventPublisher)),
					new MeetingMessageSender(eventPublisher),
					new GroupPageChangedMessageSender(eventPublisher),
					new TeamOrSiteChangedMessageSender(eventPublisher),
					new PersonChangedMessageSender(eventPublisher),
					new PersonPeriodChangedMessageSender(eventPublisher)
				};
		}
	}

	public class ConfigurationReaderFactory
	{
		public IConfigurationReader Reader()
		{
			var specification = new ConfigFromWebServiceSpecification();
			if (specification.IsRunningWithSdk())
			{
				return new SdkConfigurationReader();
			}

			var wrapper = new ConfigurationManagerWrapper();
			var xmlPath = wrapper.AppSettings["ConfigPath"];
			if (string.IsNullOrWhiteSpace(xmlPath))
			{
				xmlPath = AppDomain.CurrentDomain.BaseDirectory;
			}
			
			return new FileConfigurationReader(xmlPath);
		}
	}

	public class ConfigFromWebServiceSpecification
	{
		public bool IsRunningWithSdk()
		{
			var wrapper = new ConfigurationManagerWrapper();
			return !wrapper.AppSettings.ContainsKey("ConfigPath");
		}
	}

	public class FileConfigurationReader : IConfigurationReader
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(FileConfigurationReader));
		private readonly string _xmlFilePath;
		private static readonly object LockObject = new object();

		public FileConfigurationReader(string xmlFilePath)
		{
			_xmlFilePath = xmlFilePath;
		}

		public void ReadConfiguration(MessageSenderCreator creator)
		{
			lock (LockObject)
			{
				if (StateHolderReader.IsInitialized)
				{
					Logger.Info("StateHolder already initialized. This step is skipped.");
					return;
				}

				MessageBrokerContainer.Configure(null, MessageFilterManager.Instance);
				var application =
					new InitializeApplication(
						new DataSourcesFactory(new EnversConfiguration(), creator.Create(),
						                       DataSourceConfigurationSetter.ForServiceBus()),
						MessageBrokerContainer.CompositeClient());
				application.Start(new BasicState(), _xmlFilePath, null, new ConfigurationManagerWrapper(), true);

				Logger.Info("Initialized application");
			}
		}
	}

    public interface IConfigurationReader
    {
		void ReadConfiguration(MessageSenderCreator creator);
    }
}
