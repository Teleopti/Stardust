using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Messaging.Client
{
    /// <summary>
    /// The Message Registration Manager.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 11/04/2009
    /// </remarks>
    public class MessageRegistrationManager : IMessageRegistrationManager
    {
        private static readonly object _lockFilters = new object();
        private IBrokerService _brokerService;
        private MessageBrokerImplementation _messageBroker;

        public MessageRegistrationManager(MessageBrokerImplementation messageBroker)
        {
            _messageBroker = messageBroker;
        }

        public IBrokerService BrokerService
        {
            get { return _brokerService; }
            set { _brokerService = value; }
        }

        public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Type parentObjectType, Type domainObjectType)
        {
            RegisterEventSubscription(eventMessageHandler, Guid.Empty, parentObjectType, Guid.Empty, domainObjectType, Consts.MinDate, Consts.MaxDate);
        }

        public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Guid parentObjectId, Type parentObjectType, Guid domainObjectId, Type domainObjectType)
        {
            RegisterEventSubscription(eventMessageHandler, parentObjectId, parentObjectType, domainObjectId, domainObjectType, Consts.MinDate, Consts.MaxDate);
        }

        public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Type parentObjectType, Type domainObjectType, DateTime startDate, DateTime endDate)
        {
            RegisterEventSubscription(eventMessageHandler, Guid.Empty, parentObjectType, Guid.Empty, domainObjectType, startDate, endDate);
        }

        public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Guid parentObjectId, Type parentObjectType, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate)
        {
            if (!domainObjectType.IsInterface)
                throw new NotInterfaceTypeException();
            lock (_lockFilters)
            {
                IEventFilter filter = RegisterFilter(parentObjectId, parentObjectType, domainObjectId, domainObjectType, startDate, endDate);
                if (_messageBroker.Filters.ContainsKey(eventMessageHandler))
                {
                    _messageBroker.Filters[eventMessageHandler].Add(filter);
                }
                else
                {
                    List<IEventFilter> list = new List<IEventFilter>();
                    list.Add(filter);
                    _messageBroker.Filters.Add(eventMessageHandler, list);
                }
            }
        }

        public void UnregisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
        {
            lock (_lockFilters)
            {
                if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
                {
                    IList<IEventFilter> filters;
                    if (_messageBroker.Filters.TryGetValue(eventMessageHandler, out filters))
                    {
                        foreach (IEventFilter filter in filters)
                            UnregisterFilter(filter.FilterId);
                        _messageBroker.Filters.Remove(eventMessageHandler);
                    }
                }
            }
        }

        public IEventFilter RegisterFilter(Guid parentObjectId, Type parentObjectType, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate)
        {
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _messageBroker.ServiceGuard(_brokerService);
                IEventFilter filter = _brokerService.RegisterFilter(_messageBroker.SubscriberId, domainObjectId, domainObjectType.AssemblyQualifiedName, startDate, endDate, Environment.UserName);
                return filter;
            }
            else
            {
                IEventFilter filter = new EventFilter(Guid.Empty, Guid.Empty, parentObjectId, parentObjectType.AssemblyQualifiedName, domainObjectId, domainObjectType.AssemblyQualifiedName, startDate, endDate, Environment.UserName, DateTime.Now);
                return filter;
            }
        }

        public void UnregisterFilter(Guid filterId)
        {
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _messageBroker.ServiceGuard(_brokerService);
                _brokerService.UnregisterFilter(filterId);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void ReintializeFilters(IDictionary<EventHandler<EventMessageArgs>, IList<IEventFilter>> filters)
        {
            foreach (KeyValuePair<EventHandler<EventMessageArgs>, IList<IEventFilter>> valuePair in filters)
            {
                foreach (IEventFilter eventFilter in valuePair.Value)
                {
                    try
                    {
                        IEventFilter filter = RegisterFilter(
                                                 eventFilter.ReferenceObjectId,
                                                 Type.GetType(eventFilter.ReferenceObjectType),
                                                 eventFilter.DomainObjectId,
                                                 Type.GetType(eventFilter.DomainObjectType),
                                                 eventFilter.EventStartDate,
                                                 eventFilter.EventEndDate);
                        eventFilter.FilterId = filter.FilterId;
                    }
                    catch (Exception exception)
                    {
                        Logger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exception.ToString());
                    }
                }
            }
        }

    }
}
