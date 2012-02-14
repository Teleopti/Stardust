using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using NUnit.Framework;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging.Core;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.DataAccessLayer;
using Teleopti.Messaging.Events;

namespace Teleopti.Messaging.Tests
{
    [TestFixture]
    public class StoredProcedureTest
    {
        private string _connectionString;

        [SetUp]
        public void SetUp()
        {
            _connectionString = @"Data Source=teleopti457;User Id=TeleoptiDemoUser;Password=TeleoptiDemoPwd;Initial Catalog=TeleoptiAnalytics_Demo";
        }

        private void DeleteAddedRecord(string commandText, string name, Guid id)
        {
            ConnectionFactory.GetInstance(_connectionString).GetOpenConnection();
            SqlCommand cmd = new SqlCommand
            {
                CommandText = commandText,
                CommandType = CommandType.StoredProcedure,
                Connection = ConnectionFactory.GetInstance(_connectionString).GetOpenConnection()
            };
            SqlParameter parameter = new SqlParameter(name, SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Input,
                Value = id
            };
            cmd.Parameters.Add(parameter);
            cmd.ExecuteNonQuery();
        }

        private void DeleteAddedRecord(string commandText, string name, long id)
        {
            ConnectionFactory.GetInstance(_connectionString).GetOpenConnection();
            SqlCommand cmd = new SqlCommand
                                 {
                                     CommandText = commandText,
                                     CommandType = CommandType.StoredProcedure,
                                     Connection = ConnectionFactory.GetInstance(_connectionString).GetOpenConnection()
                                 };
            SqlParameter parameter = new SqlParameter(name, SqlDbType.Int)
                                         {
                                             Direction = ParameterDirection.Input,
                                             Value = id
                                         };
            cmd.Parameters.Add(parameter);
            cmd.ExecuteNonQuery();
        }

        [Test]
        public void EmptyTests()
        {
            Console.WriteLine(Guid.Empty);
        }

        [Test]
        public void StoreAddressInfoTests()
        {
            ConnectionFactory.GetInstance(_connectionString).GetOpenConnection();
            AddressInserter inserter = new AddressInserter(_connectionString);
            IMessageInformation addressInfo = new MessageInformation(Guid.Empty, 0, "0.0.0.0", 9091);
            inserter.Execute(addressInfo);
            AddressReader reader = new AddressReader(_connectionString);
            IAddressInformation fromDB = reader.Execute(addressInfo.AddressId);
            Assert.AreEqual(addressInfo.Port, fromDB.Port);
            DeleteAddedRecord("msg.sp_Address_Delete", "@AddressId", addressInfo.AddressId);
        }

        [Test]
        public void StoreReceiptTests()
        {
            ConnectionFactory.GetInstance(_connectionString).GetOpenConnection();
            EventMessageInserter eventInserter = new EventMessageInserter(_connectionString);
            EventMessage message = new EventMessage(Guid.Empty, DateTime.Now, DateTime.Now, 1, 333, Guid.Empty, 444, false, Guid.NewGuid(), typeof(Object).AssemblyQualifiedName, Guid.NewGuid(), typeof(Object).AssemblyQualifiedName, DomainUpdateType.Insert, new byte[0], "ankarlp", DateTime.Now);
            eventInserter.Execute(message);

            ReceiptInserter inserter = new ReceiptInserter(_connectionString);
            IEventReceipt receipt = new EventReceipt(Guid.Empty, message.EventId, Process.GetCurrentProcess().Id, Environment.UserName, DateTime.Now);
            inserter.Execute(receipt);
            ReceiptReader reader = new ReceiptReader(_connectionString);
            IEventReceipt fromDB = reader.Execute(receipt.ReceiptId);

            Assert.AreEqual(receipt.ProcessId, fromDB.ProcessId);
            Assert.AreEqual(receipt.ChangedBy, fromDB.ChangedBy);

            DeleteAddedRecord("msg.sp_Receipt_Delete", "@ReceiptId", receipt.ReceiptId);
            DeleteAddedRecord("msg.sp_Event_Delete", "@EventId", message.EventId);
        }

        [Test]
        public void StoreHeartbeatTests()
        {
            ConnectionFactory.GetInstance(_connectionString).GetOpenConnection();
            HeartbeatInserter inserter = new HeartbeatInserter(_connectionString);
            IEventHeartbeat beat = new EventHeartbeat(Guid.Empty, Guid.Empty, Process.GetCurrentProcess().Id, Environment.UserName, DateTime.Now);
            inserter.Execute(beat);
            //HeartbeatReader reader = new HeartbeatReader(_connectionString);
            //IEventHeartbeat fromDB = reader.Execute(beat.HeartbeatId);

            //Assert.AreEqual(beat.ProcessId, fromDB.ProcessId);
            //Assert.AreEqual(beat.ChangedBy, fromDB.ChangedBy);

            //DeleteAddedRecord("msg.sp_Heartbeat_Delete", "@HeartbeatId", beat.HeartbeatId);
        }

        [Test]
        public void StoreEventUserTests()
        {
            ConnectionFactory.GetInstance(_connectionString).GetOpenConnection();
            EventUserInserter inserter = new EventUserInserter(_connectionString);
            EventUser user = new EventUser();
            user.UserId = 0;
            user.Domain = "TOPTINET";
            user.UserName = "urban";
            user.ChangedBy = "pudel";
            user.ChangedDateTime = DateTime.Now;
                                 
            inserter.Execute(user);
            EventUserReader reader = new EventUserReader(_connectionString);
            IEventUser userFromDB = reader.Execute(user.UserId);

            Assert.AreNotEqual(0, userFromDB.UserId);
            Assert.AreEqual(user.Domain, userFromDB.Domain);
            Assert.AreEqual(user.UserName, userFromDB.UserName);
            Assert.AreEqual(user.ChangedBy, userFromDB.ChangedBy);

            DeleteAddedRecord("msg.sp_Users_Delete", "@UserId", user.UserId);

        }


        [Test]
        public void StoreEventUserCollectionTests()
        {
            ConnectionFactory.GetInstance(_connectionString).GetOpenConnection();
            EventUserInserter inserter = new EventUserInserter(_connectionString);
            EventUser user = new EventUser();
            user.UserId = 0;
            user.Domain = "TOPTINET";
            user.UserName = "ankarlp";
            user.ChangedBy = "ankarlp";
            user.ChangedDateTime = DateTime.Now;
            ICollection<IEventUser> users = new List<IEventUser>();
            users.Add(user);
            inserter.Execute(users);
            EventUserReader reader = new EventUserReader(_connectionString);
            IEventUser userFromDB = reader.Execute(user.UserId);

            Assert.AreEqual(user.UserId, userFromDB.UserId);
            Assert.AreEqual(user.Domain, userFromDB.Domain);
            Assert.AreEqual(user.UserName, userFromDB.UserName);
            Assert.AreEqual(user.ChangedBy, userFromDB.ChangedBy);

        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void StoreEventUserIsNullTests()
        {
            Assert.Throws(typeof(ArgumentNullException), delegate
                                                              {
                                                                  ConnectionFactory.GetInstance(_connectionString).GetOpenConnection();
                                                                  EventUserInserter inserter = new EventUserInserter(_connectionString);
                                                                  inserter.Execute((IEventUser)null);
                                                              });

        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void StoreEventUserCollectionIsNullTests()
        {
            Assert.Throws(typeof(ArgumentNullException), delegate
                                                  {
                                                      ConnectionFactory.GetInstance(_connectionString).GetOpenConnection();
                                                      EventUserInserter inserter = new EventUserInserter(_connectionString);
                                                      inserter.Execute((ICollection<IEventUser>)null);
                                                  });
        }

        [Test]
        public void StoreEventSubscriberTests()
        {
            ConnectionFactory.GetInstance(_connectionString).GetOpenConnection();
            EventSubscriberInserter inserter = new EventSubscriberInserter(_connectionString);
            EventSubscriber subscriber = new EventSubscriber();
            subscriber.SubscriberId = Guid.Empty;
            subscriber.UserId = 1;
            subscriber.ProcessId = Process.GetCurrentProcess().Id;
            subscriber.IPAddress = "0.0.0.0";
            subscriber.Port = 0;
            subscriber.ChangedBy = "ankarlp";
            subscriber.ChangedDateTime = DateTime.Now;
            inserter.Execute(subscriber);

            EventSubscriberReader reader = new EventSubscriberReader(_connectionString);
            IEventSubscriber subscriberFromDB = reader.Execute(subscriber.SubscriberId);

            Assert.AreEqual(subscriber.UserId, subscriberFromDB.UserId);
            Assert.AreEqual(subscriber.ChangedBy, subscriberFromDB.ChangedBy);
            DeleteAddedRecord("msg.sp_Subscriber_Delete", "@SubscriberId", subscriberFromDB.SubscriberId);
        }

        [Test]
        public void StoreEventSubscriberCollectionTests()
        {
            ConnectionFactory.GetInstance(_connectionString).GetOpenConnection();
            EventSubscriberInserter inserter = new EventSubscriberInserter(_connectionString);
            EventSubscriber subscriber = new EventSubscriber();
            subscriber.SubscriberId = Guid.Empty;
            subscriber.UserId = 1;
            subscriber.ProcessId = Process.GetCurrentProcess().Id;
            subscriber.IPAddress = "0.0.0.0";
            subscriber.Port = 0;
            subscriber.ChangedBy = "ankarlp";
            subscriber.ChangedDateTime = DateTime.Now;
            ICollection<IEventSubscriber> subscribers = new Collection<IEventSubscriber> {subscriber};
            inserter.Execute(subscribers);
            EventSubscriberReader reader = new EventSubscriberReader(_connectionString);
            IEventSubscriber subscriberFromDB = reader.Execute(subscriber.SubscriberId);

            Assert.AreEqual(subscriber.SubscriberId, subscriber.SubscriberId);
            Assert.AreEqual(subscriber.UserId, subscriberFromDB.UserId);
            Assert.AreEqual(subscriber.ProcessId, subscriberFromDB.ProcessId);
            Assert.AreEqual(subscriber.ChangedBy, subscriberFromDB.ChangedBy);

            DeleteAddedRecord("msg.sp_Subscriber_Delete", "@SubscriberId", subscriberFromDB.SubscriberId);

        }

        [Test]
        public void StoreEventFilterTests()
        {
            ConnectionFactory.GetInstance(_connectionString).GetOpenConnection();
            EventSubscriberInserter subscriberInserter = new EventSubscriberInserter(_connectionString);
            EventSubscriber subscriber = new EventSubscriber();
            subscriber.SubscriberId = Guid.Empty;
            subscriber.UserId = 1;
            subscriber.ProcessId = Process.GetCurrentProcess().Id;
            subscriber.IPAddress = "0.0.0.0";
            subscriber.Port = 0;
            subscriber.ChangedBy = "ankarlp";
            subscriber.ChangedDateTime = DateTime.Now;
            subscriberInserter.Execute(subscriber);
            EventFilterInserter inserter = new EventFilterInserter(_connectionString);
            EventFilter filter = new EventFilter
                                     {
                                         FilterId = Guid.Empty,
                                         SubscriberId = subscriber.SubscriberId,
                                         ReferenceObjectId = Guid.NewGuid(),
                                         ReferenceObjectType = typeof(Object).AssemblyQualifiedName,
                                         DomainObjectId = Guid.NewGuid(),
                                         DomainObjectType = typeof(Object).AssemblyQualifiedName,
                                         EventStartDate = DateTime.Now,
                                         EventEndDate = DateTime.Now.AddDays(1),
                                         ChangedBy = "ankarlp",
                                         ChangedDateTime = DateTime.Now
                                     };
            inserter.Execute(filter);
            EventFilterReader reader = new EventFilterReader(_connectionString);
            IEventFilter filterFromDB = reader.Execute(filter.FilterId);
            Assert.AreEqual(filter.SubscriberId, filterFromDB.SubscriberId);
            Assert.AreEqual(filter.DomainObjectId, filterFromDB.DomainObjectId);
            Assert.AreEqual(filter.DomainObjectType, filterFromDB.DomainObjectType);
            Assert.AreEqual(filter.ChangedBy, filterFromDB.ChangedBy);
            DeleteAddedRecord("msg.sp_Filter_Delete", "@FilterId", filterFromDB.FilterId);
            DeleteAddedRecord("msg.sp_Subscriber_Delete", "@SubscriberId", subscriber.SubscriberId);

        }

        [Test]
        public void StoreEventFilterCollectionTests()
        {
            ConnectionFactory.GetInstance(_connectionString).GetOpenConnection();
            EventSubscriberInserter subscriberInserter = new EventSubscriberInserter(_connectionString);
            EventSubscriber subscriber = new EventSubscriber();
            subscriber.SubscriberId = Guid.Empty;
            subscriber.UserId = 1;
            subscriber.ProcessId = Process.GetCurrentProcess().Id;
            subscriber.IPAddress = "0.0.0.0";
            subscriber.Port = 0;
            subscriber.ChangedBy = "ankarlp";
            subscriber.ChangedDateTime = DateTime.Now;
            subscriberInserter.Execute(subscriber);
            EventFilterInserter inserter = new EventFilterInserter(_connectionString);
            EventFilter filter = new EventFilter
                                     {
                                         FilterId = Guid.Empty,
                                         SubscriberId = subscriber.SubscriberId,
                                         ReferenceObjectId = Guid.NewGuid(),
                                         ReferenceObjectType = typeof(Object).AssemblyQualifiedName,
                                         DomainObjectId = Guid.NewGuid(),
                                         DomainObjectType = typeof(Object).AssemblyQualifiedName,
                                         EventStartDate = DateTime.Now,
                                         EventEndDate = DateTime.Now.AddDays(1),
                                         ChangedBy = "ankarlp",
                                         ChangedDateTime = DateTime.Now
                                     };
            IList<IEventFilter> filters = new List<IEventFilter> {filter};
            inserter.Execute(filters);
            EventFilterReader reader = new EventFilterReader(_connectionString);
            IEventFilter filterFromDB = reader.Execute(filter.FilterId);

            Assert.AreEqual(filter.SubscriberId, filterFromDB.SubscriberId);
            Assert.AreEqual(filter.DomainObjectType, filterFromDB.DomainObjectType);
            Assert.AreEqual(filter.ChangedBy, filterFromDB.ChangedBy);
            DeleteAddedRecord("msg.sp_Filter_Delete", "@FilterId", filterFromDB.FilterId);
            DeleteAddedRecord("msg.sp_Subscriber_Delete", "@SubscriberId", subscriber.SubscriberId);
        }

        [Test]
        public void StoreEventMessageTests()
        {
            ConnectionFactory.GetInstance(_connectionString).GetOpenConnection();
            EventMessageInserter inserter = new EventMessageInserter(_connectionString);
            EventMessage message = new EventMessage(Guid.Empty, DateTime.Now, DateTime.Now, 1, 333, Guid.Empty, 444, false, Guid.NewGuid(), typeof(Object).AssemblyQualifiedName, Guid.NewGuid(), typeof(Object).AssemblyQualifiedName, DomainUpdateType.Insert, new byte[0], "ankarlp", DateTime.Now);

            inserter.Execute(message);
            EventMessageReader reader = new EventMessageReader(_connectionString);
            IEventMessage retreivedMessage = reader.Execute(message.EventId);
            Assert.AreNotEqual(0, retreivedMessage.EventId);
            Assert.AreEqual(message.UserId, retreivedMessage.UserId);
            Assert.AreEqual(message.DomainObjectId, retreivedMessage.DomainObjectId);
            Assert.AreEqual(message.DomainObjectType, retreivedMessage.DomainObjectType);
            Assert.AreEqual(message.ChangedBy, retreivedMessage.ChangedBy);
            DeleteAddedRecord("msg.sp_Event_Delete", "@EventId", retreivedMessage.EventId);
        }

        [Test]
        public void StoreEventMessageCollectionTests()
        {
            ConnectionFactory.GetInstance(_connectionString).GetOpenConnection();
            EventMessageInserter inserter = new EventMessageInserter(_connectionString);
            EventMessage message =
                new EventMessage(Guid.Empty, DateTime.Now, DateTime.Now, 1, 333, Guid.Empty, 444, false, Guid.NewGuid(), typeof(Object).AssemblyQualifiedName, Guid.NewGuid(), typeof(Object).AssemblyQualifiedName,
                                 DomainUpdateType.Insert, new byte[0], "ankarlp", DateTime.Now);

            IList<IEventMessage> eventMessages = new List<IEventMessage> {message};
            inserter.Execute(eventMessages);

            EventMessageReader reader = new EventMessageReader(_connectionString);
            IEventMessage messages = reader.Execute(message.EventId);
            Assert.AreEqual(message.UserId, messages.UserId);
            Assert.AreEqual(message.ChangedBy, messages.ChangedBy);

            DeleteAddedRecord("msg.sp_Event_Delete", "@EventId", message.EventId);
        }


        [Test]
        public void StoreEventLogEntryTests()
        {
            ConnectionFactory.GetInstance(_connectionString).GetOpenConnection();
            LogEntryInserter inserter = new LogEntryInserter(_connectionString);
            LogEntry logEntry = new LogEntry();

            logEntry.LogId = Guid.Empty;
            logEntry.ProcessId = Process.GetCurrentProcess().Id;
            logEntry.Description = "Test description";
            logEntry.Exception = string.Empty;
            logEntry.Message = string.Empty;
            logEntry.StackTrace = string.Empty;
            logEntry.ChangedBy = "ankarlp";
            logEntry.ChangedDateTime = DateTime.Now;
                                         
            inserter.Execute(logEntry);

            LogEntryReader reader = new LogEntryReader(_connectionString);
            ILogEntry eventLogEntry = reader.Execute(logEntry.LogId);

            Assert.AreNotEqual(0, eventLogEntry.LogId);
            Assert.AreEqual(logEntry.ProcessId, eventLogEntry.ProcessId);
            Assert.AreEqual(logEntry.Exception, eventLogEntry.Exception);
            Assert.AreEqual(logEntry.Message, eventLogEntry.Message);
            Assert.AreEqual(logEntry.StackTrace, eventLogEntry.StackTrace);
            Assert.AreEqual(logEntry.ChangedBy, eventLogEntry.ChangedBy);

            DeleteAddedRecord("msg.sp_Log_Delete","@LogId", eventLogEntry.LogId);
        }


        [Test]
        public void StoreEventLogEntryCollectionTests()
        {
            ConnectionFactory.GetInstance(_connectionString).GetOpenConnection();
            LogEntryInserter inserter = new LogEntryInserter(_connectionString);
            LogEntry logEntry = new LogEntry
            {
                LogId = Guid.Empty,
                ProcessId = Process.GetCurrentProcess().Id,
                Description = "Hej",
                Exception = string.Empty,
                Message = string.Empty,
                StackTrace = string.Empty,
                ChangedBy = "ankarlp",
                ChangedDateTime = DateTime.Now
            };
            IList<ILogEntry> logEntries = new List<ILogEntry> {logEntry};
            inserter.Execute(logEntries);
            LogEntryReader reader = new LogEntryReader(_connectionString);
            ILogEntry eventLogEntry = reader.Execute(logEntries[0].LogId);

            Assert.AreEqual(logEntry.Description, eventLogEntry.Description);
            Assert.AreEqual(logEntry.Exception, eventLogEntry.Exception);
            Assert.AreEqual(logEntry.Message, eventLogEntry.Message);
            Assert.AreEqual(logEntry.StackTrace, eventLogEntry.StackTrace);
            Assert.AreEqual(logEntry.ChangedBy, eventLogEntry.ChangedBy);
        }

        [TearDown]
        public void TearDown()
        {

        }

    }
}
