using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Coders;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging.Core;
using Teleopti.Messaging.Coders;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.Events;

namespace Teleopti.Messaging.Tests
{
    [TestFixture]
    public class EncoderDecoderTest
    {
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private string ipAddress;

        [SetUp]
        public void SetUp()
        {
            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            ipAddress = SocketUtility.IsIpAddress(ipEntry).ToString();
        }

        // ReSharper disable MemberCanBeMadeStatic
        private void ReceiveHeartbeat()
        {
            string[] args = new string[2];
            args[0] = "9083";
            args[1] = "tcp";
            int port = Int32.Parse(args[0]);   // Receiving Port
            UdpClient client = new UdpClient(port); // UDP socket for receiving
            IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, port);
            byte[] packet = new byte[Consts.MaxWireLength];
            packet = client.Receive(ref remoteIPEndPoint);
            EventHeartbeatDecoder decoder = new EventHeartbeatDecoder();
            IEventHeartbeat heartbeat = decoder.Decode(packet);
            Assert.IsNotEmpty(heartbeat.ToString());
            client.Close();
        }
        // ReSharper restore MemberCanBeMadeStatic

        public void ReceiveEventLogEntryUdp()
        {
            string[] args = new string[2];
            args[0] = "8090";
            args[1] = "tcp";
            int port = Int32.Parse(args[0]);   // Receiving Port
            UdpClient client = new UdpClient(port); // UDP socket for receiving
            IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, port);
            byte[] packet = new byte[Consts.MaxWireLength];
            packet = client.Receive(ref remoteIPEndPoint);
            LogEntryDecoder decoder = new LogEntryDecoder();
            ILogEntry msg = decoder.Decode(packet);
            Assert.IsNotEmpty(msg.ToString());
            client.Close();
        }

        public void ReceiveEventLogEntryTcp()
        {
            string[] args = new string[2];
            args[0] = "8091";
            args[1] = "tcp";
            int port = Int32.Parse(args[0]);   // Receiving Port
            // Create a TCPListener to accept client connections
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            TcpClient client = listener.AcceptTcpClient();   // Get client connection

            _resetEvent.Set();

            // Receive text-encoded quote
            LogEntryDecoder decoder = new LogEntryDecoder();
            ILogEntry msg = decoder.Decode(client.GetStream());
            Assert.IsNotEmpty(msg.ToString());

            client.Close();
            listener.Stop();

        }

        public void ReceiveEventUserUdp()
        {
            string[] args = new string[2];
            args[0] = "8092";
            args[1] = "tcp";
            int port = Int32.Parse(args[0]);   // Receiving Port
            UdpClient client = new UdpClient(port); // UDP socket for receiving
            IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, port);
            byte[] packet = new byte[Consts.MaxWireLength];
            packet = client.Receive(ref remoteIPEndPoint);
            IEventUserDecoder decoder = new EventUserDecoder();
            IEventUser msg = decoder.Decode(packet);
            Assert.IsNotEmpty(msg.ToString());
            client.Close();
        }

        public void ReceiveEventUserTcp()
        {
            string[] args = new string[2];
            args[0] = "8093";
            args[1] = "tcp";
            int port = Int32.Parse(args[0]);   // Receiving Port
            // Create a TCPListener to accept client connections
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            TcpClient client = listener.AcceptTcpClient();   // Get client connection

            _resetEvent.Set();

            // Receive text-encoded quote
            IEventUserDecoder decoder = new EventUserDecoder();
            IEventUser msg = decoder.Decode(client.GetStream());
            Assert.IsNotEmpty(msg.ToString());
            client.Close();
            listener.Stop();

        }

        public void ReceiveEventSubscriberUdp()
        {
            string[] args = new string[2];
            args[0] = "8094";
            args[1] = "tcp";
            int port = Int32.Parse(args[0]);   // Receiving Port
            UdpClient client = new UdpClient(port); // UDP socket for receiving
            IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, port);
            byte[] packet = new byte[Consts.MaxWireLength];
            packet = client.Receive(ref remoteIPEndPoint);
            IEventSubscriberDecoder decoder = new EventSubscriberDecoder();
            IEventSubscriber msg = decoder.Decode(packet);
            Assert.IsNotEmpty(msg.ToString());
            client.Close();
        }

        public void ReceiveEventSubscriberTcp()
        {
            string[] args = new string[2];
            args[0] = "9001";
            args[1] = "tcp";
            int port = Int32.Parse(args[0]);   // Receiving Port
            // Create a TCPListener to accept client connections
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            TcpClient client = listener.AcceptTcpClient();   // Get client connection
            _resetEvent.Set();
            // Receive text-encoded quote
            IEventSubscriberDecoder decoder = new EventSubscriberDecoder();
            IEventSubscriber msg = decoder.Decode(client.GetStream());
            Assert.IsNotEmpty(msg.ToString());
            client.Close();
            listener.Stop();
        }

        public void ReceiveEventFilterUdp()
        {
            string[] args = new string[2];
            args[0] = "8096";
            args[1] = "tcp";
            int port = Int32.Parse(args[0]);   // Receiving Port
            UdpClient client = new UdpClient(port); // UDP socket for receiving
            IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, port);
            byte[] packet = new byte[Consts.MaxWireLength];
            packet = client.Receive(ref remoteIPEndPoint);
            IEventFilterDecoder decoder = new EventFilterDecoder();
            IEventFilter msg = decoder.Decode(packet);
            Assert.IsNotEmpty(msg.ToString());
            client.Close();
        }

        public void ReceiveEventFilterTcp()
        {
            string[] args = new string[2];
            args[0] = "8097";
            args[1] = "tcp";
            int port = Int32.Parse(args[0]);   // Receiving Port
            // Create a TCPListener to accept client connections
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            TcpClient client = listener.AcceptTcpClient();   // Get client connection

            _resetEvent.Set();

            // Receive text-encoded quote
            IEventFilterDecoder decoder = new EventFilterDecoder();
            IEventFilter msg = decoder.Decode(client.GetStream());
            Assert.IsNotEmpty(msg.ToString());

            client.Close();
            listener.Stop();

        }

        public void ReceiveEventMessageUdp()
        {
            string[] args = new string[2];
            args[0] = "8098";
            args[1] = "tcp";
            int port = Int32.Parse(args[0]);   // Receiving Port
            UdpClient client = new UdpClient(port); // UDP socket for receiving
            IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, port);
            byte[] packet = new byte[Consts.MaxWireLength];
            packet = client.Receive(ref remoteIPEndPoint);
            IEventMessageDecoder decoder = new EventMessageDecoder();
            IEventMessage msg = decoder.Decode(packet);
            Assert.IsNotEmpty(msg.ToString());

            client.Close();
        }

        public void ReceiveEventMessageTcp()
        {
            string[] args = new string[2];
            args[0] = "8099";
            args[1] = "tcp";
            int port = Int32.Parse(args[0]);   // Receiving Port
            // Create a TCPListener to accept client connections
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            TcpClient client = listener.AcceptTcpClient();   // Get client connection

            _resetEvent.Set();

            // Receive text-encoded quote
            IEventMessageDecoder decoder = new EventMessageDecoder();
            IEventMessage msg = decoder.Decode(client.GetStream());
            IEventMessage embeddedMessage = null;
            if (msg.DomainObject.Length > 1)
            {
                embeddedMessage = decoder.Decode(msg.DomainObject);
            }
            Assert.IsNotEmpty(msg.ToString());
            if (embeddedMessage != null)
            {
                Assert.IsNotEmpty(embeddedMessage.ToString());
            }
            client.Close();
            listener.Stop();

        }

        [Test]
        public void FramerUtilityNextTokenTests()
        {
            EventMessage message = new EventMessage(Guid.Empty, DateTime.Now, DateTime.Now, 1, 333, Guid.Empty, 444, false, Guid.NewGuid(), typeof(Object).AssemblyQualifiedName, Guid.NewGuid(), typeof(Object).AssemblyQualifiedName, DomainUpdateType.Insert, "ankarlp", DateTime.Now);
            // Binary
            EventMessageEncoder encoder = new EventMessageEncoder();
            byte[] bytes = encoder.Encode(message);
            FramerUtility utility = new FramerUtility();
            byte[] delimiter = Encoding.ASCII.GetBytes(new char[] { Consts.Separator });
            MemoryStream stream = new MemoryStream(bytes);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] token = utility.NextToken(stream, delimiter);
            string s = Encoding.ASCII.GetString(token);
            Assert.AreEqual("00000000-0000-0000-0000-000000000000", s);
        }

        [Test]
        public void FramerUtilityEndsWithTests()
        {
            FramerUtility utility = new FramerUtility();
            byte[] delimiter = Encoding.ASCII.GetBytes(new char[]{Consts.Separator});
            bool test = utility.EndsWith(Encoding.ASCII.GetBytes("ankarlp\n"), delimiter);
            Assert.AreEqual(true, test);
        }

        [Test]
        public void SendAndReceiveHeartbeatTest()
        {
            ThreadStart start = ReceiveHeartbeat;
            Thread t = new Thread(start);
            t.IsBackground = true;
            t.Name = "Tcp Receiving Thread";
            t.Start();
            _resetEvent.WaitOne(1000, false);
            string[] args = new string[3];
            args[0] = ipAddress;
            args[1] = "9083";
            args[2] = "tcp";
            String server = args[0];             // Destination address
            int servPort = Int32.Parse(args[1]); // Destination port
            // Create socket that is connected to server on specified port
            UdpClient client = new UdpClient();
            IEventHeartbeat heartbeat = new EventHeartbeat(Guid.Empty, Guid.Empty, Process.GetCurrentProcess().Id, "ankarlp", DateTime.Now);
            // Send text-encoded quote
            IEventHeartbeatEncoder coder = new EventHeartbeatEncoder();
            byte[] fullMessage = coder.Encode(heartbeat);
            client.Send(fullMessage, fullMessage.Length, server, servPort);
            t.Join(1000);
            client.Close();
        }


        [Test]
        public void SendAndReceiveEventMessageTcp()
        {
            ThreadStart start = ReceiveEventMessageTcp;
            Thread t = new Thread(start);
            t.IsBackground = true;
            t.Name = "Tcp Receiving Thread";
            t.Start();
            _resetEvent.WaitOne(1000, false);
            string[] args = new string[3];
            args[0] = ipAddress;
            args[1] = "8099";
            args[2] = "tcp";
            String server = args[0];             // Destination address
            int servPort = Int32.Parse(args[1]); // Destination port
            // Create socket that is connected to server on specified port
            TcpClient client = new TcpClient(server, servPort);
            NetworkStream netStream = client.GetStream();
            IEventMessage eventMessage = new EventMessage(Guid.Empty, DateTime.Now, DateTime.Now, 1, Process.GetCurrentProcess().Id, Guid.Empty, 0, false, Guid.NewGuid(), typeof(Object).AssemblyQualifiedName, Guid.NewGuid(), typeof(Object).AssemblyQualifiedName, DomainUpdateType.Insert, "ankarlp", DateTime.Now);
            // Send text-encoded quote
            IEventMessageEncoder coder = new EventMessageEncoder();
            byte[] codedMessage = coder.Encode(eventMessage);
            eventMessage.DomainObject = codedMessage;
            byte[] fullMessage = coder.Encode(eventMessage);
            netStream.Write(fullMessage, 0, fullMessage.Length);
            netStream.Close(1000);
            client.Close();
            t.Join(3000);
        }

        [Test]
        public void SendAndReceiveEventMessageUdp()
        {
            ThreadStart start = ReceiveEventMessageUdp;
            Thread t = new Thread(start) { IsBackground = true, Name = "UDP Receiving Thread" };
            t.IsBackground = true;
            t.Start();
            _resetEvent.WaitOne(1000, false);
            string[] args = new string[3];
            args[0] = ipAddress;
            args[1] = "8098";
            args[2] = "udp";
            String server = args[0];             // Server name or IP address
            int destPort = Int32.Parse(args[1]); // Destination port
            IEventMessage eventMessage = new EventMessage(Guid.NewGuid(), DateTime.Now, DateTime.Now, 1, Process.GetCurrentProcess().Id, Guid.Empty, 0, false, Guid.NewGuid(), typeof(Object).AssemblyQualifiedName, Guid.NewGuid(), typeof(Object).AssemblyQualifiedName, DomainUpdateType.Insert, "ankarlp", DateTime.Now);
            UdpClient client = new UdpClient(); // UDP socket for sending
            IEventMessageEncoder encoder = new EventMessageEncoder();
            byte[] codedMessage = encoder.Encode(eventMessage);
            Assert.AreNotEqual(0, codedMessage.Length);
            client.Send(codedMessage, codedMessage.Length, server, destPort);
            t.Join(1000);
            client.Close();
        }

        [Test]
        public void SendAndReceiveEventFilterTcp()
        {
            ThreadStart start = ReceiveEventFilterTcp;
            Thread t = new Thread(start);
            t.IsBackground = true;
            t.Name = "Tcp Receiving Thread";
            t.Start();
            _resetEvent.WaitOne(1000, false);
            string[] args = new string[3];
            args[0] = ipAddress;
            args[1] = "8097";
            args[2] = "tcp";
            String server = args[0];             // Destination address
            int servPort = Int32.Parse(args[1]); // Destination port
            // Create socket that is connected to server on specified port
            TcpClient client = new TcpClient(server, servPort);
            NetworkStream netStream = client.GetStream();
            IEventFilter eventFilter = new EventFilter(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), typeof(Object).AssemblyQualifiedName, Guid.NewGuid(), typeof(Object).AssemblyQualifiedName, DateTime.Now, DateTime.Now.AddDays(1), "ankarlp", DateTime.Now);
            // Send text-encoded quote
            IEventFilterEncoder coder = new EventFilterEncoder();
            byte[] codedMessage = coder.Encode(eventFilter);
            netStream.Write(codedMessage, 0, codedMessage.Length);
            netStream.Close(1000);
            client.Close();
        }

        [Test]
        public void SendAndReceiveEventFilterUdp()
        {
            ThreadStart start = ReceiveEventFilterUdp;
            Thread t = new Thread(start) { IsBackground = true, Name = "UDP Receiving Thread" };
            t.IsBackground = true;
            t.Start();
            _resetEvent.WaitOne(1000, false);
            string[] args = new string[3];
            args[0] = ipAddress;
            args[1] = "8096";
            args[2] = "udp";
            String server = args[0];             // Server name or IP address
            int destPort = Int32.Parse(args[1]); // Destination port
            IEventFilter eventFilter = new EventFilter(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), typeof(Object).AssemblyQualifiedName, Guid.NewGuid(), typeof(Object).AssemblyQualifiedName, DateTime.Now, DateTime.Now.AddDays(1), "ankarlp", DateTime.Now);
            UdpClient client = new UdpClient(); // UDP socket for sending
            IEventFilterEncoder encoder = new EventFilterEncoder();
            byte[] codedMessage = encoder.Encode(eventFilter);
            client.Send(codedMessage, codedMessage.Length, server, destPort);
            t.Join(1000);
            client.Close();
        }

        [Test]
        public void SendAndReceiveEventUserTcp()
        {
            ThreadStart start = ReceiveEventUserTcp;
            Thread t = new Thread(start) { IsBackground = true, Name = "Tcp Receiving Thread" };
            t.IsBackground = true;
            t.Start();
            _resetEvent.WaitOne(1000, false);
            string[] args = new string[3];
            args[0] = ipAddress;
            args[1] = "8093";
            args[2] = "tcp";
            String server = args[0];             // Destination address
            int servPort = Int32.Parse(args[1]); // Destination port
            // Create socket that is connected to server on specified port
            TcpClient client = new TcpClient(server, servPort);
            NetworkStream netStream = client.GetStream();
            IEventUser eventUser = new EventUser(1, "TOPTINET", "ankarlp", "ankarlp", DateTime.Now);
            // Send text-encoded quote
            IEventUserEncoder coder = new EventUserEncoder();
            byte[] codedMessage = coder.Encode(eventUser);
            netStream.Write(codedMessage, 0, codedMessage.Length);
            netStream.Close(1000);
            client.Close();
        }

        [Test]
        public void SendAndReceiveEventUserUdp()
        {
            ThreadStart start = ReceiveEventUserUdp;
            Thread t = new Thread(start) { IsBackground = true, Name = "UDP Receiving Thread" };
            t.IsBackground = true;
            t.Start();
            _resetEvent.WaitOne(1000, false);
            string[] args = new string[3];
            args[0] = ipAddress;
            args[1] = "8092";
            args[2] = "udp";
            String server = args[0];             // Server name or IP address
            int destPort = Int32.Parse(args[1]); // Destination port
            IEventUser eventUser = new EventUser(1, "TOPTINET", "ankarlp", "ankarlp", DateTime.Now);
            UdpClient client = new UdpClient(); // UDP socket for sending
            IEventUserEncoder encoder = new EventUserEncoder();
            byte[] codedMessage = encoder.Encode(eventUser);
            client.Send(codedMessage, codedMessage.Length, server, destPort);
            t.Join(1000);
            client.Close();
        }

        [Test]
        public void SendAndReceiveEventLogEntryTcp()
        {
            ThreadStart start = ReceiveEventLogEntryTcp;
            Thread t = new Thread(start) { IsBackground = true, Name = "Tcp Receiving Thread" };
            t.IsBackground = true;
            t.Start();
            _resetEvent.WaitOne(1000, false);
            string[] args = new string[3];
            args[0] = ipAddress;
            args[1] = "8091";
            args[2] = "tcp";
            String server = args[0];             // Destination address
            int servPort = Int32.Parse(args[1]); // Destination port
            // Create socket that is connected to server on specified port
            TcpClient client = new TcpClient(server, servPort);
            NetworkStream netStream = client.GetStream();
            ILogEntry eventLogEntry = new LogEntry(Guid.NewGuid(), Process.GetCurrentProcess().Id, "Test description", String.Empty, String.Empty, String.Empty, "ankarlp", DateTime.Now);
            // Send text-encoded quote
            IEventLogEntryEncoder coder = new LogEntryEncoder();
            byte[] codedMessage = coder.Encode(eventLogEntry);
            netStream.Write(codedMessage, 0, codedMessage.Length);
            netStream.Close(1000);
            client.Close();
        }

        [Test]
        public void SendAndReceiveEventLogEntryUdp()
        {
            ThreadStart start = ReceiveEventLogEntryUdp;
            Thread t = new Thread(start) { IsBackground = true, Name = "UDP Receiving Thread" };
            t.IsBackground = true;
            t.Start();
            _resetEvent.WaitOne(1000, false);
            string[] args = new string[3];
            args[0] = ipAddress;
            args[1] = "8090";
            args[2] = "udp";
            String server = args[0];             // Server name or IP address
            int destPort = Int32.Parse(args[1]); // Destination port
            ILogEntry eventLogEntry = new LogEntry(Guid.NewGuid(), Process.GetCurrentProcess().Id, "Test description", String.Empty, String.Empty, String.Empty, "ankarlp", DateTime.Now);
            UdpClient client = new UdpClient(); // UDP socket for sending
            IEventLogEntryEncoder encoder = new LogEntryEncoder();
            byte[] codedMessage = encoder.Encode(eventLogEntry);
            client.Send(codedMessage, codedMessage.Length, server, destPort);
            t.Join(1000);
            client.Close();
        }

        [Test]
        public void SendAndReceiveEventSubscriberTcp()
        {
            ThreadStart start = ReceiveEventSubscriberTcp;
            Thread t = new Thread(start) { IsBackground = true, Name = "Tcp Receiving Thread" };
            t.IsBackground = true;
            t.Start();
            _resetEvent.WaitOne(1000, false);
            string[] args = new string[3];
            args[0] = ipAddress;
            args[1] = "9001";
            args[2] = "tcp";
            String server = args[0];             // Destination address
            int servPort = Int32.Parse(args[1]); // Destination port
            // Create socket that is connected to server on specified port
            TcpClient client = new TcpClient(server, servPort);
            NetworkStream netStream = client.GetStream();
            IEventSubscriber eventSubscriber = new EventSubscriber(Guid.NewGuid(), 1, Process.GetCurrentProcess().Id, "172.22.1.31", 9090, "ankarlp", DateTime.Now);
            // Send text-encoded quote
            IEventSubscriberEncoder coder = new EventSubscriberEncoder();
            byte[] codedMessage = coder.Encode(eventSubscriber);
            netStream.Write(codedMessage, 0, codedMessage.Length);
            netStream.Close(1000);
            client.Close();
        }

        [Test]
        public void SendAndReceiveEventSubscriberUdp()
        {
            ThreadStart start = ReceiveEventSubscriberUdp;
            Thread t = new Thread(start);
            t.IsBackground = true;
            t.Name = "UDP Receiving Thread";
            t.Start();
            _resetEvent.WaitOne(1000, false);
            string[] args = new string[3];
            args[0] = ipAddress;
            args[1] = "8094";
            args[2] = "udp";
            String server = args[0];             // Server name or IP address
            int destPort = Int32.Parse(args[1]); // Destination port
            IEventSubscriber eventSubscriber = new EventSubscriber(Guid.NewGuid(), 1, Process.GetCurrentProcess().Id, "172.22.1.31", 9090, "ankarlp", DateTime.Now);
            UdpClient client = new UdpClient(); // UDP socket for sending
            IEventSubscriberEncoder encoder = new EventSubscriberEncoder();
            byte[] codedMessage = encoder.Encode(eventSubscriber);
            client.Send(codedMessage, codedMessage.Length, server, destPort);
            t.Join(1000);
            client.Close();
        }

        [Test, ExpectedException(typeof(IOException))]
        public void EventUserEncoderTests()
        {
            Assert.Throws(typeof(IOException), delegate
            {
                IEventUser eventUser = new EventUser(1, "TOPTI\nET", "ankarlp", "ankarlp", DateTime.Now);
                IEventUserEncoder encoder = new EventUserEncoder();
                encoder.Encode(eventUser);
            });
        }

        // **********

        [Test, ExpectedException(typeof(IOException))]
        public void EventMessageEncoderChangedByTests()
        {
            Assert.Throws(typeof(IOException), delegate
            {
                IEventMessage eventMessage = new EventMessage(Guid.NewGuid(), DateTime.Now, DateTime.Now, 1, Process.GetCurrentProcess().Id, Guid.Empty, 0, false, Guid.NewGuid(), typeof(Object).AssemblyQualifiedName, Guid.NewGuid(), typeof(Object).AssemblyQualifiedName, DomainUpdateType.Insert, "a\nkarlp", DateTime.Now);
                IEventMessageEncoder encoder = new EventMessageEncoder();
                encoder.Encode(eventMessage);
            });
        }

        [Test, ExpectedException(typeof(IOException))]
        public void EventUserEncoderUserNameTests()
        {
            Assert.Throws(typeof(IOException), delegate
            {
                IEventUser eventUser = new EventUser(1, "TOPTIET", "a\nkarlp", "ankarlp", DateTime.Now);
                IEventUserEncoder encoder = new EventUserEncoder();
                encoder.Encode(eventUser);
            });
        }

        [Test, ExpectedException(typeof(IOException))]
        public void EventUserEncoderUserNameChangedByNewLineTests()
        {
            Assert.Throws(typeof(IOException), delegate
            {
                IEventUser eventUser = new EventUser(1, "TOPTIET", "ankarlp", "a\nkarlp", DateTime.Now);
                IEventUserEncoder encoder = new EventUserEncoder();
                encoder.Encode(eventUser);
            });
        }

        [Test, ExpectedException(typeof(IOException))]
        public void EventLogEntryEncoderChangedByTests()
        {
            Assert.Throws(typeof(IOException), delegate
            {
                ILogEntry eventLogEntry = new LogEntry(Guid.NewGuid(), Process.GetCurrentProcess().Id, string.Empty, String.Empty, String.Empty, String.Empty, "a\nkarlp", DateTime.Now);
                IEventLogEntryEncoder encoder = new LogEntryEncoder();
                encoder.Encode(eventLogEntry);
            });
        }

        [TearDown]
        public void TearDown()
        {
        }

    }
}
