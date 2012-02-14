using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Reflection;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Events;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Messaging.Composites
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
        private readonly MessageBrokerImplementation _messageBroker;
        private readonly IDictionary<EventHandler<EventMessageArgs>, IList<IEventFilter>> _filters = new Dictionary<EventHandler<EventMessageArgs>, IList<IEventFilter>>();
        private IBrokerService _brokerService;

        public MessageRegistrationManager(MessageBrokerImplementation messageBrokerImplementation)
        {
            _messageBroker = messageBrokerImplementation;
        }

        public IBrokerService BrokerService
        {
            get { return _brokerService; }
            set { _brokerService = value; }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IDictionary<EventHandler<EventMessageArgs>, IList<IEventFilter>> Filters
        {
            get { return _filters; }
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

        /// <summary>
        /// Registers the event subscription.
        /// </summary>
        /// <param name="eventMessageHandler">The event message handler.</param>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 05/08/2009
        /// </remarks>
        public void RegisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler, Guid parentObjectId, Type parentObjectType, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate)
        {
            if (!domainObjectType.IsInterface)
                throw new NotInterfaceTypeException();
            lock (_lockFilters)
            {
                IEventFilter filter = RegisterFilter(parentObjectId, parentObjectType, domainObjectId, domainObjectType, startDate, endDate);
                IList<IEventFilter> eventFilters;
                if (_filters.TryGetValue(eventMessageHandler,out eventFilters))
                {
                    eventFilters.Add(filter);
                }
                else
                {
                    var list = new List<IEventFilter>{filter};
                    _filters.Add(eventMessageHandler, list);
                }
            }
        }

        /// <summary>
        /// Unregisters the event subscription.
        /// </summary>
        /// <param name="eventMessageHandler">The event message handler.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/04/2009
        /// </remarks>
        public void UnregisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
        {
            lock (_lockFilters)
            {
                IList<IEventFilter> filters;
                if (_filters.TryGetValue(eventMessageHandler, out filters))
                {
                    if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
                    {
                        foreach (IEventFilter filter in filters)
                            InternalUnregisterFilter(filter.FilterId);
                    }
                    _filters.Remove(eventMessageHandler);
                }
            }
        }

        /// <summary>
        /// Unregisters the filter.
        /// </summary>
        /// <param name="filterId">The filter id.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 05/08/2009
        /// </remarks>
        public void UnregisterFilter(Guid filterId)
        {
            lock (_lockFilters)
            {
                InternalUnregisterFilter(filterId);
            }
        }

        /// <summary>
        /// Internal unregistration of filters.
        /// </summary>
        /// <param name="filterId">The filter id.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 05/08/2009
        /// </remarks>
        private void InternalUnregisterFilter(Guid filterId)
        {

            try
            {
                _messageBroker.ServiceGuard(_brokerService);
                _brokerService.UnregisterFilter(filterId);
            }
            catch (SocketException exception)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exception.ToString());
            }
        }


        /// <summary>
        /// Checks the filters.
        /// </summary>
        /// <param name="eventMessageArgs">The event message args.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void CheckFilters(EventMessageArgs eventMessageArgs)
        {
            IList<EventHandler<EventMessageArgs>> removeList = new List<EventHandler<EventMessageArgs>>();
            try
            {
                lock (_lockFilters)
                {
                    foreach (var key in _filters)
                    {
                        IList<IEventFilter> filters = key.Value;
                        foreach (IEventFilter filter in filters)
                        {
                            Type eventMessageType = eventMessageArgs.Message.InterfaceType;

                            CheckFilter(key.Key,
                                        filter,
                                        eventMessageArgs,
                                        filter.DomainObjectTypeCache,
                                        eventMessageType,
                                        removeList);

                        }
                    }
                }
            }
            catch (Exception exc)
            {
                _messageBroker.InternalLog(exc);
            }

            RemoveDeadDelegates(removeList);

        }

        /// <summary>
        /// Checks the filter.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="eventMessageArgs">The event message args.</param>
        /// <param name="filterType">Type of the filter.</param>
        /// <param name="eventMessageType">Type of the event message.</param>
        /// <param name="removeList">The remove list.</param>
        private void CheckFilter(EventHandler<EventMessageArgs> key,
                                 IEventFilter filter,
                                 EventMessageArgs eventMessageArgs,
                                 Type filterType,
                                 Type eventMessageType,
                                 ICollection<EventHandler<EventMessageArgs>> removeList)
        {
            if ((filter.DomainObjectId == eventMessageArgs.Message.DomainObjectId && filter.DomainObjectType != null &&
                 filterType.IsAssignableFrom(eventMessageType)) ||
                (filter.DomainObjectId == Guid.Empty && filter.DomainObjectType != null &&
                 filterType.IsAssignableFrom(eventMessageType)))
            {
                InvokeDelegate(key, filter, eventMessageArgs, removeList);
            }
        }


        /// <summary>
        /// Registers the filter.
        /// </summary>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 05/08/2009
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public IEventFilter RegisterFilter(Guid parentObjectId, Type parentObjectType, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate)
        {
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                IEventFilter filter;
                try
                { 
                    _messageBroker.ServiceGuard(_brokerService);
                    filter = _brokerService.RegisterFilter(_messageBroker.SubscriberId, domainObjectId, domainObjectType.AssemblyQualifiedName, startDate, endDate, Environment.UserName);
                }
                catch (Exception exception)
                {
                    BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), string.Format(System.Globalization.CultureInfo.InvariantCulture, "An error occured while trying register subscription with broker: {0}", exception.Message));
                    filter = new EventFilter(Guid.Empty, Guid.Empty, parentObjectId, parentObjectType.AssemblyQualifiedName, domainObjectId, domainObjectType.AssemblyQualifiedName, startDate, endDate, Environment.UserName, DateTime.Now);
                }
                return filter;
            }
            else
            {
                IEventFilter filter = new EventFilter(Guid.Empty, Guid.Empty, parentObjectId, parentObjectType.AssemblyQualifiedName, domainObjectId, domainObjectType.AssemblyQualifiedName, startDate, endDate, Environment.UserName, DateTime.Now);
                return filter;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void ReinitializeFilters()
        {
            foreach (KeyValuePair<EventHandler<EventMessageArgs>, IList<IEventFilter>> valuePair in _filters)
            {
                foreach (IEventFilter eventFilter in valuePair.Value)
                {
                    try
                    {
                        IEventFilter filter = RegisterFilter(
                            eventFilter.ReferenceObjectId,
                            eventFilter.ReferenceObjectTypeCache,
                            eventFilter.DomainObjectId,
                            eventFilter.DomainObjectTypeCache,
                            eventFilter.EventStartDate,
                            eventFilter.EventEndDate);
                        eventFilter.FilterId = filter.FilterId;
                    }
                    catch (Exception exception)
                    {
                        BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exception.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Unregisters the filters.
        /// </summary>
        public void UnregisterFilters()
        {
            lock (_lockFilters)
            {
                if (Filters != null)
                {
                    foreach (IList<IEventFilter> filters in Filters.Values)
                    {
                        foreach (IEventFilter filter in filters)
                        {
                            UnregisterFilter(filter.FilterId);
                        }
                    }
                    Filters.Clear();
                }
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="eventMessageArgs">The event message args.</param>
        /// <param name="removeList">The remove list.</param>
        private void InvokeDelegate(EventHandler<EventMessageArgs> key, IEventFilter filter, EventMessageArgs eventMessageArgs, ICollection<EventHandler<EventMessageArgs>> removeList)
        {
            if (filter.EventStartDate != Consts.MinDate || filter.EventEndDate != Consts.MaxDate)
            {
                if (filter.EventStartDate <= eventMessageArgs.Message.EventStartDate &&
                    filter.EventEndDate >= eventMessageArgs.Message.EventEndDate)
                    InvokeRegisteredDelegate(key, eventMessageArgs, removeList);
            }
            else
            {
                InvokeRegisteredDelegate(key, eventMessageArgs, removeList);
            }
        }

        /// <summary>
        /// Invokes the registered delegate.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="eventMessageArgs">The event message args.</param>
        /// <param name="removeList">The remove list.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void InvokeRegisteredDelegate(EventHandler<EventMessageArgs> key, EventMessageArgs eventMessageArgs, ICollection<EventHandler<EventMessageArgs>> removeList)
        {
            foreach (Delegate del in key.GetInvocationList())
            {
                try
                {
                    // arguments are: object sender and EventMessageArgs e ...
                    del.Method.Invoke(del.Target, new object[] { this, eventMessageArgs });
                }
                catch (TargetInvocationException exc)
                {
                    if (exc.InnerException != null)
                        BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.ToString());
                }
                catch (Exception deadClientEx)
                {
                    BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), "DeadClientException: " + deadClientEx);
                    _messageBroker.InternalLog(deadClientEx);
                    Delegate.Remove(key, del);
                    removeList.Add(key);
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void RemoveDeadDelegates(IEnumerable<EventHandler<EventMessageArgs>> removeList)
        {
            try
            {
                foreach (EventHandler<EventMessageArgs> key in removeList)
                    UnregisterEventSubscription(key);
            }
            catch (Exception exc)
            {
                _messageBroker.InternalLog(exc);
            }
        }


    }
}