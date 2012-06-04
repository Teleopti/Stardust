using System;
using System.Collections.Generic;
using System.ServiceModel;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using log4net;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.ClientProxies;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Composites;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class SdkConfigurationReader : ISdkConfigurationReader
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BusBootStrapper));
        private static readonly object LockObject = new object();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "NHibernate"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void ReadConfiguration(Func<IServiceBus> serviceBus)
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
                using (Proxy proxy = new Proxy())
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

            	var notify = new BusDenormalizeNotification(serviceBus);
            	var saveToDenormalizationQueue = new SaveToDenormalizationQueue();
                encryptedAppSettings.DecryptDictionary(EncryptionConstants.Image1, EncryptionConstants.Image2);
            	var application =
            		new InitializeApplication(
            			new DataSourcesFactory(new EnversConfiguration(),
            			                       new List<IDenormalizer>
            			                       	{
            			                       		new ScheduleDenormalizer(notify, saveToDenormalizationQueue),
            			                       		new MeetingDenormalizer(notify, saveToDenormalizationQueue)
            			                       	},
															new DataSourceConfigurationSetter(false, true, "thread_static")),
            			MessageBrokerImplementation.GetInstance(MessageFilterManager.Instance.FilterDictionary));
                application.Start(new BasicState(), encryptedAppSettings,
                                  encryptedNHibConfigs.DecryptList(EncryptionConstants.Image1,
                                                                   EncryptionConstants.Image2), null);

                Logger.Info("Initialized application");
            }
        }
    }

    public interface ISdkConfigurationReader
    {
        void ReadConfiguration(Func<IServiceBus> serviceBus);
    }

	public class BusDenormalizeNotification : ISendDenormalizeNotification
	{
		private readonly Func<IServiceBus> _serviceBus;

		public BusDenormalizeNotification(Func<IServiceBus> serviceBus)
		{
			_serviceBus = serviceBus;
		}

		public void Notify()
		{
			var identity = (TeleoptiIdentity)TeleoptiPrincipal.Current.Identity;
			_serviceBus.Invoke().Send(new ProcessDenormalizeQueue
			                          	{
			                 		BusinessUnitId = identity.BusinessUnit.Id.GetValueOrDefault(Guid.Empty),
			                 		Datasource = identity.DataSource.Application.Name,
			                 		Timestamp = DateTime.UtcNow
			                 	});
		}
	}
}
