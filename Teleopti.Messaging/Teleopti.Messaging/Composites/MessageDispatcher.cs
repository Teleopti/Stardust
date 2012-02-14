﻿using System;
using System.Diagnostics;
using System.Net.Sockets;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Events;

namespace Teleopti.Messaging.Composites
{

    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly IBrokerService _brokerService;
        private readonly MessageBrokerImplementation _messageBroker;
        private readonly IMessageFilterManager _messageFilterManager;

        public MessageDispatcher(MessageBrokerImplementation messageBroker, IBrokerService brokerService, IMessageFilterManager messageFilterManager)
        {
            _messageBroker = messageBroker;
            _brokerService = brokerService;
            _messageFilterManager = messageFilterManager;
        }

        public void SendEventMessage(DateTime eventStartDate,
                                     DateTime eventEndDate,
                                     Guid moduleId,
                                     Guid parentObjectId,
                                     Type parentObjectType,
                                     Guid domainObjectId,
                                     Type domainObjectType,
                                     DomainUpdateType updateType)
        {
        	var processId = Process.GetCurrentProcess().Id;
            SendEventMessageInterProcess(eventStartDate,
                                         eventEndDate,
                                         _messageBroker.UserId,
                                         processId,
                                         moduleId,
                                         0,
                                         false,
                                         parentObjectId,
                                         (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(parentObjectType) : parentObjectType.AssemblyQualifiedName),
                                         domainObjectId,
                                         (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName),
                                         updateType,
                                         new byte[0],
                                         Environment.UserName);
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _brokerService.SendEventMessage(eventStartDate,
                                                eventEndDate,
                                                _messageBroker.UserId,
                                                processId,
                                                moduleId,
                                                0,
                                                false,
                                                domainObjectId,
                                                (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName),
                                                updateType,
                                                Environment.UserName);
            }
        }

        public void SendEventMessage(DateTime eventStartDate,
                                     DateTime eventEndDate,
                                     Guid moduleId,
                                     Guid parentObjectId,
                                     Type parentObjectType,
                                     Guid domainObjectId,
                                     Type domainObjectType,
                                     DomainUpdateType updateType,
                                     byte[] domainObject)
        {
        	var processId = Process.GetCurrentProcess().Id;
            SendEventMessageInterProcess(eventStartDate,
                                         eventEndDate,
                                         _messageBroker.UserId,
                                         processId,
                                         moduleId,
                                         0,
                                         false,
                                         parentObjectId,
                                         (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(parentObjectType) : parentObjectType.AssemblyQualifiedName),
                                         domainObjectId,
                                         (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName),
                                         updateType,
                                         domainObject,
                                         Environment.UserName);
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _brokerService.SendEventMessage(eventStartDate,
                                                eventEndDate,
                                                _messageBroker.UserId,
                                                processId,
                                                moduleId,
                                                0,
                                                false,
                                                domainObjectId,
                                                (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName),
                                                updateType,
                                                domainObject,
                                                Environment.UserName);
            }
        }

        public void SendEventMessages(IEventMessage[] eventMessages)
        {
        	var processId = Process.GetCurrentProcess().Id;
            foreach (IEventMessage eventMessage in eventMessages)
            {
                SendEventMessageInterProcess(eventMessage.EventStartDate,
                                             eventMessage.EventEndDate,
                                             _messageBroker.UserId,
                                             processId,
                                             eventMessage.ModuleId,
                                             eventMessage.PackageSize,
                                             eventMessage.IsHeartbeat,
                                             eventMessage.ReferenceObjectId,
                                             eventMessage.ReferenceObjectType,
                                             eventMessage.DomainObjectId,
                                             eventMessage.DomainObjectType,
                                             eventMessage.DomainUpdateType,
                                             eventMessage.DomainObject,
                                             eventMessage.ChangedBy);
            }
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                try
                {
                    _brokerService.SendEventMessages(eventMessages);
                }
                catch (SocketException exception)
                {
                    BaseLogger.Instance.WriteLine(EventLogEntryType.Error,GetType(),string.Format(System.Globalization.CultureInfo.InvariantCulture, "An error occured while trying to notify broker: {0}",exception.Message));
                }
            }
        }

        public void SendEventMessage(Guid parentObjectId, Type parentObjectType, Guid domainObjectId, Type domainObjectType)
        {
			var processId = Process.GetCurrentProcess().Id;
            SendEventMessageInterProcess(Consts.MinDate,
                                         Consts.MaxDate,
                                         _messageBroker.UserId,
                                         processId,
                                         Guid.Empty,
                                         0,
                                         false,
                                         parentObjectId,
                                         (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(parentObjectType) : parentObjectType.AssemblyQualifiedName),
                                         domainObjectId,
                                         (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName),
                                         DomainUpdateType.NotApplicable,
                                         new byte[0],
                                         Environment.UserName);
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _brokerService.SendEventMessage(DateTime.Now,
                                                DateTime.Now,
                                                _messageBroker.UserId,
                                                processId,
                                                Guid.Empty,
                                                0,
                                                false,
                                                domainObjectId,
                                                (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName),
                                                DomainUpdateType.NotApplicable,
                                                Environment.UserName);
            }
        }

        public void SendEventMessage(Guid parentObjectId, Type parentObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
        {
			var processId = Process.GetCurrentProcess().Id;
            SendEventMessageInterProcess(Consts.MinDate,
                                         Consts.MaxDate,
                                         _messageBroker.UserId,
                                         processId,
                                         Guid.Empty,
                                         0,
                                         false,
                                         parentObjectId,
                                         (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(parentObjectType) : parentObjectType.AssemblyQualifiedName),
                                         domainObjectId,
                                         (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName),
                                         updateType,
                                         new byte[0],
                                         Environment.UserName);
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _brokerService.SendEventMessage(DateTime.Now,
                                                DateTime.Now,
                                                _messageBroker.UserId,
                                                processId,
                                                Guid.Empty,
                                                0,
                                                false,
                                                domainObjectId,
                                                (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName),
                                                updateType,
                                                Environment.UserName);
            }
        }

        public void SendEventMessage(Guid moduleId, Guid parentObjectId, Type parentObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType)
        {
			var processId = Process.GetCurrentProcess().Id;
            SendEventMessageInterProcess(Consts.MinDate,
                                         Consts.MaxDate,
                                         _messageBroker.UserId,
                                         processId,
                                         moduleId,
                                         0,
                                         false,
                                         parentObjectId,
                                         (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(parentObjectType) : parentObjectType.AssemblyQualifiedName),
                                         domainObjectId,
                                         (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName),
                                         updateType,
                                         new byte[0],
                                         Environment.UserName);
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _brokerService.SendEventMessage(DateTime.Now,
                                                DateTime.Now,
                                                _messageBroker.UserId,
                                                processId,
                                                moduleId,
                                                0,
                                                false,
                                                domainObjectId,
                                                (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName),
                                                updateType,
                                                Environment.UserName);
            }
        }

        public void SendEventMessage(Guid moduleId, Guid parentObjectId, Type parentObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
        {
			var processId = Process.GetCurrentProcess().Id;
            SendEventMessageInterProcess(Consts.MinDate,
                                         Consts.MaxDate,
                                         _messageBroker.UserId,
                                         processId,
                                         moduleId,
                                         0,
                                         false,
                                         parentObjectId,
                                         (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(parentObjectType) : parentObjectType.AssemblyQualifiedName),
                                         domainObjectId,
                                         (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName),
                                         updateType,
                                         domainObject,
                                         Environment.UserName);
            if (!String.IsNullOrEmpty(_messageBroker.ConnectionString) && _messageBroker.Initialized == 0)
            {
                _brokerService.SendEventMessage(Consts.MinDate,
                                                Consts.MaxDate,
                                                _messageBroker.UserId,
                                                processId,
                                                moduleId,
                                                0,
                                                false,
                                                domainObjectId,
                                                (_messageBroker.IsTypeFilterApplied ? _messageFilterManager.LookupType(domainObjectType) : domainObjectType.AssemblyQualifiedName),
                                                updateType,
                                                domainObject,
                                                Environment.UserName);
            }
        }

        private void SendEventMessageInterProcess(DateTime startDate, DateTime endDate, int userId, int processId, Guid moduleId, int packageSize, bool isHeartbeat, Guid parentObjectId, string parentObjectType, Guid domainObjectId, string domainObjectType, DomainUpdateType updateType, byte[] domainObject, string userName)
        {
            IEventMessage message = new EventMessage(Guid.Empty, startDate, endDate, userId, processId, moduleId, packageSize, isHeartbeat, parentObjectId, parentObjectType, domainObjectId, domainObjectType, updateType, domainObject, userName, DateTime.Now);
            _messageBroker.CustomThreadPool.QueueUserWorkItem(SendInterProcess, message);
        }

        private void SendInterProcess(object state)
        {
            IEventMessage message = (IEventMessage)state;
            message.IsInterprocess = true;
            _messageBroker.OnEventMessage(this, new EventMessageArgs(message));
        }

    }
}