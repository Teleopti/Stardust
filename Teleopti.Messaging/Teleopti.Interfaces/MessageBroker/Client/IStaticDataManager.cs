using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Client
{
    /// <summary>
    /// The Static Data Manager
    /// </summary>
    public interface IStaticDataManager
    {
        /// <summary>
        /// Updates the configurations.
        /// </summary>
        /// <param name="configurations">The configurations.</param>
        void UpdateConfigurations(IList<IConfigurationInfo> configurations);
        /// <summary>
        /// Deletes the configuration item.
        /// </summary>
        /// <param name="configurationInfo">The configuration info.</param>
        void DeleteConfigurationItem(IConfigurationInfo configurationInfo);
        /// <summary>
        /// Updates the addresses.
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        void UpdateAddresses(IList<IMessageInformation> addresses);
        /// <summary>
        /// Deletes the address item.
        /// </summary>
        /// <param name="addressInfo">The address info.</param>
        void DeleteAddressItem(IMessageInformation addressInfo);
        /// <summary>
        /// Retrieves the heartbeats.
        /// </summary>
        /// <returns></returns>
        IEventHeartbeat[] RetrieveHeartbeats();
        /// <summary>
        /// Retrieves the logbook entries.
        /// </summary>
        /// <returns></returns>
        ILogbookEntry[] RetrieveLogbookEntries();
        /// <summary>
        /// Retrieves the event users.
        /// </summary>
        /// <returns></returns>
        IEventUser[] RetrieveEventUsers();
        /// <summary>
        /// Retrieves the event receipt.
        /// </summary>
        /// <returns></returns>
        IEventReceipt[] RetrieveEventReceipt();
        /// <summary>
        /// Retrieves the subscribers.
        /// </summary>
        /// <returns></returns>
        IEventSubscriber[] RetrieveSubscribers();
        /// <summary>
        /// Retrieves the filters.
        /// </summary>
        /// <returns></returns>
        IEventFilter[] RetrieveFilters();
        /// <summary>
        /// Retrieves the configurations.
        /// </summary>
        /// <returns></returns>
        IList<IConfigurationInfo> RetrieveConfigurations();
        /// <summary>
        /// Retrieves the addresses.
        /// </summary>
        /// <returns></returns>
        IList<IMessageInformation> RetrieveAddresses();
    }
}