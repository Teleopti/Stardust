using System;
using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client;

namespace Teleopti.Messaging.Composites
{
    public class StaticDataManager : IStaticDataManager
    {
        private readonly MessageBrokerImplementation _messageBroker;

        public StaticDataManager(MessageBrokerImplementation messageBroker)
        {
            _messageBroker = messageBroker;
        }

        public void UpdateConfigurations(IList<IConfigurationInfo> configurations)
        {
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _messageBroker.ServiceGuard(_messageBroker.BrokerService);
                _messageBroker.BrokerService.UpdateConfigurations(configurations);
            }
        }

        public void DeleteConfigurationItem(IConfigurationInfo configurationInfo)
        {
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _messageBroker.ServiceGuard(_messageBroker.BrokerService);
                _messageBroker.BrokerService.DeleteConfiguration(configurationInfo);
            }
        }

        public void UpdateAddresses(IList<IMessageInformation> addresses)
        {
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _messageBroker.ServiceGuard(_messageBroker.BrokerService);
                _messageBroker.BrokerService.UpdateAddresses(addresses);
            }
        }

        public void DeleteAddressItem(IMessageInformation addressInfo)
        {
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _messageBroker.ServiceGuard(_messageBroker.BrokerService);
                _messageBroker.BrokerService.DeleteAddresses(addressInfo);
            }
        }

        public IEventHeartbeat[] RetrieveHeartbeats()
        {
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _messageBroker.ServiceGuard(_messageBroker.BrokerService);
                return _messageBroker.BrokerService.RetrieveHeartbeats();
            }
            return new IEventHeartbeat[0];
        }

        public ILogbookEntry[] RetrieveLogbookEntries()
        {
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _messageBroker.ServiceGuard(_messageBroker.BrokerService);
                return _messageBroker.BrokerService.RetrieveLogbookEntries();
            }
            return new ILogbookEntry[0];
        }

        public IEventUser[] RetrieveEventUsers()
        {
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _messageBroker.ServiceGuard(_messageBroker.BrokerService);
                return _messageBroker.BrokerService.RetrieveEventUsers();
            }
            return new IEventUser[0];
        }

        public IEventReceipt[] RetrieveEventReceipt()
        {
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _messageBroker.ServiceGuard(_messageBroker.BrokerService);
                return _messageBroker.BrokerService.RetrieveEventReceipt();
            }
            return new IEventReceipt[0];
        }

        public IEventSubscriber[] RetrieveSubscribers()
        {
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _messageBroker.ServiceGuard(_messageBroker.BrokerService);
                return _messageBroker.BrokerService.RetrieveSubscribers();
            }
            return new IEventSubscriber[0];
        }

        public IEventFilter[] RetrieveFilters()
        {
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _messageBroker.ServiceGuard(_messageBroker.BrokerService);
                return _messageBroker.BrokerService.RetrieveFilters();
            }
            return new IEventFilter[0];
        }

        public IList<IConfigurationInfo> RetrieveConfigurations()
        {
            return _messageBroker.CreateConfigurations();
        }

        public IList<IMessageInformation> RetrieveAddresses()
        {
            return _messageBroker.CreateAddresses();
        }

    }
}