using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Logging.Core;
using Teleopti.Messaging.Events;

namespace Teleopti.Messaging.Tests
{

    [TestFixture]
    public class EventsSerialisationTest
    {
        private string ipAddress;

        [SetUp]
        public void SetUp()
        {
            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            IPAddress[] address = ipEntry.AddressList;
            ipAddress = address[address.Length - 1].ToString();
        }

        [Test]
        public void EventMessageSerialisationAndDeserialisationTests()
        {
            EventMessage message = new EventMessage();
            message.EventId = Guid.Empty;
            message.EventStartDate = DateTime.Now;
            message.EventEndDate = DateTime.Now;
            message.UserId = 1;
            message.ProcessId = 1;
            message.PackageSize = 0;
            message.IsHeartbeat = false;
            message.ReferenceObjectId = Guid.NewGuid();
            message.ReferenceObjectType = typeof(Object).AssemblyQualifiedName;
            message.DomainObjectId = Guid.NewGuid();
            message.DomainObjectType = typeof(Object).AssemblyQualifiedName;
            message.DomainObject = new byte[0];
            message.ChangedBy = "ankarlp";
            message.ChangedDateTime = DateTime.Now;

//            MemoryStream memoryStream = new MemoryStream();
//            XmlSerializer xmlSerialiser = new XmlSerializer(typeof(EventMessage));
//            xmlSerialiser.Serialize(memoryStream, message);
//            XmlTextWriter xmlWriter = new XmlTextWriter("EventMessageSerialisationAndDeserialisationTests.xml", Encoding.ASCII);
//            xmlWriter.WriteRaw(Encoding.ASCII.GetChars(memoryStream.GetBuffer()), 0, memoryStream.GetBuffer().Length);
//            xmlWriter.Flush();
//            xmlWriter.Close();
//            Stream stream = new FileStream("EventMessageSerialisationAndDeserialisationTests.xml", FileMode.Open, FileAccess.Read);
//            XmlTextReader xmlReader = new XmlTextReader(stream);
//            EventMessage deserializeEventMessage = (EventMessage) xmlSerialiser.Deserialize(xmlReader);
//            Assert.AreEqual(message.EventId, deserializeEventMessage.EventId);
//            Assert.AreEqual(message.UserId, deserializeEventMessage.UserId);
//            Assert.AreEqual(message.DomainObjectId, deserializeEventMessage.DomainObjectId);
//            Assert.AreEqual(message.DomainObjectType, deserializeEventMessage.DomainObjectType);
//            Assert.AreEqual(message.ChangedBy, deserializeEventMessage.ChangedBy);

            // Binary
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream binaryMemoryStream = new MemoryStream();
            binaryFormatter.Serialize(binaryMemoryStream, message);
            binaryMemoryStream.Position = 0;
            EventMessage deserializedBinaryEventMessage = (EventMessage)binaryFormatter.Deserialize(binaryMemoryStream);

            Assert.AreEqual(message.EventId, deserializedBinaryEventMessage.EventId);
            Assert.AreEqual(message.UserId, deserializedBinaryEventMessage.UserId);
            Assert.AreEqual(message.DomainObjectId, deserializedBinaryEventMessage.DomainObjectId);
            Assert.AreEqual(message.DomainObjectType, deserializedBinaryEventMessage.DomainObjectType);
            Assert.AreEqual(message.ChangedBy, deserializedBinaryEventMessage.ChangedBy);

        }

        [Test]
        public void EventMessageSerialisationWithPayloadTests()
        {

            EventMessage message = new EventMessage
                                       {
                                           EventId = Guid.Empty,
                                           EventStartDate = DateTime.Now,
                                           EventEndDate = DateTime.Now,
                                           UserId = 1,
                                           ProcessId = 1,
                                           PackageSize = 0,
                                           IsHeartbeat = false,
                                           ReferenceObjectId = Guid.NewGuid(),
                                           ReferenceObjectType = typeof(Object).AssemblyQualifiedName,
                                           DomainObjectId = Guid.NewGuid(),
                                           DomainObjectType = typeof(Object).AssemblyQualifiedName,
                                           DomainObject = new byte[0],
                                           ChangedBy = "ankarlp",
                                           ChangedDateTime = DateTime.Now
                                       };
            // Binary
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream binaryMemoryStream = new MemoryStream();
            MemoryStream binaryMemoryWithPayloadStream = new MemoryStream();
            binaryFormatter.Serialize(binaryMemoryStream, message);
            binaryMemoryStream.Position = 0;
            message.DomainObject = binaryMemoryStream.GetBuffer();
            binaryFormatter.Serialize(binaryMemoryWithPayloadStream, message);
            binaryMemoryWithPayloadStream.Position = 0;
            EventMessage deserializedBinaryEventMessage = (EventMessage)binaryFormatter.Deserialize(binaryMemoryWithPayloadStream);

//            Console.WriteLine("EventId: " + deserializedBinaryEventMessage.EventId);
//            Console.WriteLine("UserId: " + deserializedBinaryEventMessage.UserId);
//            Console.WriteLine("DomainObjectId: " + deserializedBinaryEventMessage.DomainObjectId);
//            Console.WriteLine("DomainObjectVersion: " + deserializedBinaryEventMessage.DomainObjectVersion);
//            Console.WriteLine("DomainObjectType: " + deserializedBinaryEventMessage.DomainObjectType);

            object deserialize = binaryFormatter.Deserialize(new MemoryStream(deserializedBinaryEventMessage.DomainObject));

//            Console.WriteLine("ChangedBy: " + deserializedBinaryEventMessage.ChangedBy);
//            Console.WriteLine("ChangedDateTime: " + deserializedBinaryEventMessage.ChangedDateTime);


        }


        [Test]
        public void EventFilterSerialisationAndDeserialisationTests()
        {
            EventFilter filter = new EventFilter();
            filter.FilterId = Guid.Empty;
            filter.SubscriberId = Guid.Empty;
            filter.ReferenceObjectId = Guid.NewGuid();
            filter.ReferenceObjectType = typeof(Object).AssemblyQualifiedName;
            filter.DomainObjectId = Guid.NewGuid();
            filter.DomainObjectType = typeof(Object).AssemblyQualifiedName;
            filter.ChangedBy = "ankarlp";
            filter.ChangedDateTime = DateTime.Now;
//            MemoryStream memoryStream = new MemoryStream();
//            XmlSerializer xmlSerialiser = new XmlSerializer(typeof(EventFilter));
//            xmlSerialiser.Serialize(memoryStream, filter);
//            XmlTextWriter xmlWriter = new XmlTextWriter("EventFilterSerialisationAndDeserialisationTests.xml", Encoding.ASCII);
//            xmlWriter.WriteRaw(Encoding.ASCII.GetChars(memoryStream.GetBuffer()), 0, memoryStream.GetBuffer().Length);
//            xmlWriter.Flush();
//            xmlWriter.Close();
//            Stream stream = new FileStream("EventFilterSerialisationAndDeserialisationTests.xml", FileMode.Open, FileAccess.Read);
//            XmlTextReader xmlReader = new XmlTextReader(stream);
//            EventFilter deserializeEventFilter = (EventFilter)xmlSerialiser.Deserialize(xmlReader);
//
////            Console.WriteLine("FilterId: " + deserializeEventFilter.FilterId);
////            Console.WriteLine("SubscriberId: " + deserializeEventFilter.SubscriberId);
////            Console.WriteLine("DomainObjectId: " + deserializeEventFilter.DomainObjectId);
////            Console.WriteLine("DomainObjectType: " + deserializeEventFilter.DomainObjectType);
////            Console.WriteLine("ChangedBy: " + deserializeEventFilter.ChangedBy);

            // Binary
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream binaryMemoryStream = new MemoryStream();
            binaryFormatter.Serialize(binaryMemoryStream, filter);
            binaryMemoryStream.Position = 0;
            EventFilter deserializedBinaryEventFilter = (EventFilter)binaryFormatter.Deserialize(binaryMemoryStream);

//            Console.WriteLine("FilterId: " + deserializedBinaryEventFilter.FilterId);
//            Console.WriteLine("SubscriberId: " + deserializedBinaryEventFilter.SubscriberId);
//            Console.WriteLine("DomainObjectId: " + deserializedBinaryEventFilter.DomainObjectId);
//            Console.WriteLine("DomainObjectType: " + deserializedBinaryEventFilter.DomainObjectType);
//            Console.WriteLine("ChangedBy: " + deserializedBinaryEventFilter.ChangedBy);

        }


        [Test]
        public void EventUserSerialisationAndDeserialisationTests()
        {
            EventUser user = new EventUser();
            user.UserId = 1;
            user.Domain = "TOPTINET";
            user.UserName = "ankarlp";
            user.ChangedBy = "ankarlp";
            user.ChangedDateTime = DateTime.Now;
//            MemoryStream memoryStream = new MemoryStream();
////            Console.WriteLine("Serialising Event User to xml.");
//            XmlSerializer xmlSerialiser = new XmlSerializer(typeof(EventUser));
//            xmlSerialiser.Serialize(memoryStream, user);
////            Console.WriteLine("Serialising Event User to xml Succeded!");
////            Console.WriteLine("Derialising Event User from xml.");
//            XmlTextWriter xmlWriter = new XmlTextWriter("EventUserSerialisationAndDeserialisationTests.xml", Encoding.ASCII);
//            xmlWriter.WriteRaw(Encoding.ASCII.GetChars(memoryStream.GetBuffer()), 0, memoryStream.GetBuffer().Length);
//            xmlWriter.Flush();
//            xmlWriter.Close();
//            Stream stream = new FileStream("EventUserSerialisationAndDeserialisationTests.xml", FileMode.Open, FileAccess.Read);
//            XmlTextReader xmlReader = new XmlTextReader(stream);
//            EventUser deserializeEventUser = (EventUser)xmlSerialiser.Deserialize(xmlReader);
//            Console.WriteLine("UserId: " + deserializeEventUser.UserId);
//            Console.WriteLine("Domain: " + deserializeEventUser.Domain);
//            Console.WriteLine("UserName: " + deserializeEventUser.UserName);
//            Console.WriteLine("ChangedBy: " + deserializeEventUser.ChangedBy);
//            Console.WriteLine("ChangedDateTime: " + deserializeEventUser.ChangedDateTime);
//            Console.WriteLine("Deserialising Event User from xml Succeded!");
            // Binary
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream binaryMemoryStream = new MemoryStream();
            //Console.WriteLine("Serialising Event User to binary format.");
            binaryFormatter.Serialize(binaryMemoryStream, user);
//            Console.WriteLine("Serialising Event User to binary format Succeded!");
            binaryMemoryStream.Position = 0;
            EventUser deserializedBinaryEventMessage = (EventUser)binaryFormatter.Deserialize(binaryMemoryStream);
//            Console.WriteLine("UserId: " + deserializedBinaryEventMessage.UserId);
//            Console.WriteLine("Domain: " + deserializedBinaryEventMessage.Domain);
//            Console.WriteLine("UserName: " + deserializedBinaryEventMessage.UserName);
//            Console.WriteLine("ChangedBy: " + deserializedBinaryEventMessage.ChangedBy);
//            Console.WriteLine("ChangedDateTime: " + deserializedBinaryEventMessage.ChangedDateTime);
//            Console.WriteLine("Deserialising Event User from binary format Succeded!");
        }

        [Test]
        public void EventSubscriberSerialisationAndDeserialisationTests()
        {
            EventSubscriber subscriber = new EventSubscriber();
            subscriber.SubscriberId = Guid.Empty;
            subscriber.UserId = 1;
            subscriber.ProcessId = Process.GetCurrentProcess().Id;
            subscriber.IPAddress = "0.0.0.0";
            subscriber.Port = 0;
            subscriber.ChangedBy = "ankarlp";
            subscriber.ChangedDateTime = DateTime.Now;
//            MemoryStream memoryStream = new MemoryStream();
//            XmlSerializer xmlSerialiser = new XmlSerializer(typeof(EventSubscriber));
//            xmlSerialiser.Serialize(memoryStream, subscriber);
//            XmlTextWriter xmlWriter = new XmlTextWriter("EventSubscriberSerialisationAndDeserialisationTests.xml", Encoding.ASCII);
//            xmlWriter.WriteRaw(Encoding.ASCII.GetChars(memoryStream.GetBuffer()), 0, memoryStream.GetBuffer().Length);
//            xmlWriter.Flush();
//            xmlWriter.Close();
//            Stream stream = new FileStream("EventSubscriberSerialisationAndDeserialisationTests.xml", FileMode.Open, FileAccess.Read);
//            XmlTextReader xmlReader = new XmlTextReader(stream);
//            EventSubscriber deserializeEventSubscriber = (EventSubscriber)xmlSerialiser.Deserialize(xmlReader);
            // Binary
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream binaryMemoryStream = new MemoryStream();
            binaryFormatter.Serialize(binaryMemoryStream, subscriber);
            binaryMemoryStream.Position = 0;
            EventSubscriber deserializedBinaryEventSubscriber = (EventSubscriber)binaryFormatter.Deserialize(binaryMemoryStream);
        }


        [Test]
        public void EventLogEntrySerialisationAndDeserialisationTests()
        {
            ILogEntry logEntry = new LogEntry();
            logEntry.LogId = Guid.Empty;
            logEntry.ProcessId = Process.GetCurrentProcess().Id;
            logEntry.Description = "Test description";
            logEntry.Exception = string.Empty;
            logEntry.Message = string.Empty;
            logEntry.StackTrace = string.Empty;
            logEntry.ChangedBy = "ankarlp";
            logEntry.ChangedDateTime = DateTime.Now;
//            MemoryStream memoryStream = new MemoryStream();
//            XmlSerializer xmlSerialiser = new XmlSerializer(typeof(LogEntry));
//            xmlSerialiser.Serialize(memoryStream, logEntry);
//            XmlTextWriter xmlWriter = new XmlTextWriter("EventLogEntrySerialisationAndDeserialisationTests.xml", Encoding.ASCII);
//            xmlWriter.WriteRaw(Encoding.ASCII.GetChars(memoryStream.GetBuffer()), 0, memoryStream.GetBuffer().Length);
//            xmlWriter.Flush();
//            xmlWriter.Close();
//            Stream stream = new FileStream("EventLogEntrySerialisationAndDeserialisationTests.xml", FileMode.Open, FileAccess.Read);
//            XmlTextReader xmlReader = new XmlTextReader(stream);
//            EventLogEntry deserializeEventLogEntry = (EventLogEntry)xmlSerialiser.Deserialize(xmlReader);
            // Binary
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream binaryMemoryStream = new MemoryStream();
            binaryFormatter.Serialize(binaryMemoryStream, logEntry);
            binaryMemoryStream.Position = 0;
            LogEntry deserializedBinaryEventLogEntry = (LogEntry)binaryFormatter.Deserialize(binaryMemoryStream);

        }

        [TearDown]
        public void TearDown()
        {
        }

    }

}
