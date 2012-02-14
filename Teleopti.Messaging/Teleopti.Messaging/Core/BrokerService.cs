using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Core
{
    public class BrokerService : BrokerServiceBase, IBrokerService
    {

        public BrokerService(IPublisher publisher) : base(publisher)
        {
        }

        public Int32 RegisterUser(string domain, string userName)
        {
            BrokerProcessor processor = GetProcessor();
            IEventUser user = processor.CreateUser(domain, userName);
            return processor.RegisterUser(user);
        }

        public Guid RegisterSubscriber(Int32 userId, string userName, Int32 processId, string ipAddress, int port)
        {
            BrokerProcessor processor = GetProcessor();
            IEventSubscriber subscriber = processor.CreateSubscription(userId, processId, userName, ipAddress, port);
            lock (_lockObject)
            {
                EventSubscriptions.Add(subscriber);    
            }
            return processor.RegisterSubscription(subscriber);
        }

        public void UnregisterSubscriber(Guid subscriberId)
        {
            BrokerProcessor processor = GetProcessor();
            lock (_lockObject)
            {
                IEventSubscriber subscriber = GetSubscriber(subscriberId);
                if(subscriber != null)
                    EventSubscriptions.Remove(subscriber);
            }
            processor.UnregisterSubscription(subscriberId);
        }

        public IEventFilter RegisterFilter(Guid subscriberId, Guid domainObjectId, string domainObjectType, DateTime startDate, DateTime endDate, string userName)
        {
            BrokerProcessor processor = GetProcessor();
            IEventFilter filter = processor.CreateFilter(subscriberId, domainObjectId, domainObjectType, startDate, endDate, userName);
            return processor.RegisterFilter(filter);
        }

        public void UnregisterFilter(Guid filterId)
        {
            BrokerProcessor processor = GetProcessor();
            processor.UnregisterFilter(filterId);
        }

        public void SendEventMessage(DateTime eventStartDate, DateTime eventEndDate, Int32 userId, Int32 processId, Guid moduleId, Int32 packageSize, bool isHeartbeat, Guid domainObjectId, string domainObjectType, DomainUpdateType updateType, string userName)
        {
            BrokerProcessor processor = GetProcessor();
            IEventMessage eventMessage = processor.CreateEventMessage(eventStartDate, eventEndDate, userId, processId, moduleId, packageSize, isHeartbeat, domainObjectId, domainObjectType, updateType, userName);
            CustomThreadPool.QueueUserWorkItem(SendAsync, eventMessage);
        }

        public void SendEventMessage(DateTime eventStartDate, DateTime eventEndDate, Int32 userId, Int32 processId, Guid moduleId, Int32 packageSize, bool isHeartbeat, Guid domainObjectId, string domainObjectType, DomainUpdateType updateType, byte[] domainObject, string userName)
        {
            BrokerProcessor processor = GetProcessor();
            IEventMessage eventMessage = processor.CreateEventMessage(eventStartDate, eventEndDate, userId, processId, moduleId, packageSize, isHeartbeat, domainObjectId, domainObjectType, updateType, domainObject, userName);
            CustomThreadPool.QueueUserWorkItem(SendAsync, eventMessage);
        }

        public void SendEventMessages(IEventMessage[] eventMessages)
        {
            CustomThreadPool.QueueUserWorkItem(SendAsyncList, eventMessages);
        }

        public void Log(Int32 processId, string description, string exception, string message, string stackTrace, string userName)
        {
            BrokerProcessor processor = GetProcessor();
            ILogEntry eventLogEntry = processor.CreateEventLogEntry(processId, description, exception, message, stackTrace, userName);
            CustomThreadPool.QueueUserWorkItem(LogAsync, eventLogEntry);
        }

        public IConfigurationInfo[] RetrieveConfigurations(string configurationType)
        {
            IBrokerProcessor processor = new BrokerProcessor(ConnectionString);
            return processor.ReadConfigurationInfo(configurationType);
        }

        public void SendReceipt(IEventReceipt receipt)
        {
            ReceiptThreadPool.QueueUserWorkItem(AcceptReceipt, receipt);
        }

        public void SendHeartbeat(IEventHeartbeat beat)
        {
            HeartbeatThreadPool.QueueUserWorkItem(AcceptHeartbeat, beat);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public string ServicePath
        {
            get { return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Remove(0, 6); }
        }

        public IMessageInfo[] RetrieveAddresses()
        {
            return GetAddresses();
        }

        private IMessageInfo[] GetAddresses()
        {
            IBrokerProcessor processor = new BrokerProcessor(ConnectionString);
            return processor.ReadAddressInfo();
        }

        public void UpdateConfigurations(IList<IConfigurationInfo> configurations)
        {
            IBrokerProcessor processor = new BrokerProcessor(ConnectionString);
            processor.UpdateConfigurations(configurations);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void DeleteConfiguration(IConfigurationInfo configurationInfo)
        {
            try
            {
                IBrokerProcessor processor = GetProcessor();
                processor.DeleteAddedRecord("msg.sp_Configuration_Delete", "@ConfigurationId", configurationInfo.ConfigurationId);
            }
            catch (Exception exc)
            {
                Log(Process.GetCurrentProcess().Id, "DeleteConfiguration(IConfigurationInfo configurationInfo)", exc.GetType().Name, exc.Message, exc.StackTrace, Environment.UserName);
            }
        }


        public void UpdateAddresses(IList<IMessageInfo> multicastAddressInfos)
        {
            IBrokerProcessor processor = new BrokerProcessor(ConnectionString);
            processor.UpdateAddresses(multicastAddressInfos);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void DeleteAddresses(IMessageInfo multicastAddressInfo)
        {
            try
            {
                IBrokerProcessor processor = GetProcessor();
                processor.DeleteAddedRecord("msg.sp_Address_Delete", "@MessageBrokerId", multicastAddressInfo.AddressId);
            }
            catch (Exception exc)
            {
                Log(Process.GetCurrentProcess().Id, "DeleteAddresses(IMulticastAddressInfo multicastAddressInfo)", exc.GetType().Name, exc.Message, exc.StackTrace, Environment.UserName);
            }
        }

        public IEventHeartbeat[] RetrieveHeartbeats()
        {
            IBrokerProcessor processor = new BrokerProcessor(ConnectionString);
            return processor.ReadHeartbeats();            
        }

        public ILogbookEntry[] RetrieveLogbookEntries()
        {
            IBrokerProcessor processor = new BrokerProcessor(ConnectionString);
            return processor.ReadLogbookEntries();                        
        }

        public IEventUser[] RetrieveEventUsers()
        {
            IBrokerProcessor processor = new BrokerProcessor(ConnectionString);
            return processor.ReadEventUsers();            
        }

        public IEventReceipt[] RetrieveEventReceipt()
        {
            IBrokerProcessor processor = new BrokerProcessor(ConnectionString);
            return processor.ReadEventReceipt();            
        }

        public IEventSubscriber[] RetrieveSubscribers()
        {
            IBrokerProcessor processor = new BrokerProcessor(ConnectionString);
            return processor.ReadSubscribers();            
        }

        public IEventFilter[] RetrieveFilters()
        {
            IBrokerProcessor processor = new BrokerProcessor(ConnectionString);
            return processor.ReadFilters();            
        }

        public IList<ISocketInfo> RetrieveSocketInformation()
        {
            IBrokerProcessor processor = new BrokerProcessor(ConnectionString);
            return processor.GetSocketInformation();
        }

        #region IDisposable Override Implementation

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(true);
            if (isDisposing)
            {

            }
        }

        #endregion

    }

}
