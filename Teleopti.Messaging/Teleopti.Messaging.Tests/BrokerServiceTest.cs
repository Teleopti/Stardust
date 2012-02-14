using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;
using Teleopti.Messaging.Server;

namespace Teleopti.Messaging.Tests
{
    [TestFixture]
    public class BrokerServiceTest
    {
        private MockRepository _mocks;
        private IBrokerService _brokerService;

        [SetUp]
        private void SetUp()
        {
            _mocks = new MockRepository();
            _brokerService = _mocks.CreateMock<IBrokerService>();       
        }

        [Test]
        public void TestDuplicatedSubscribersForPort()
        {

            IEventHeartbeat heartbeat = new EventHeartbeat();
            heartbeat.HeartbeatId = new Guid("5C1CF20F-7D04-4696-BF14-9D9C23F5CF2E");
            heartbeat.SubscriberId = new Guid("5C1CF20F-7D04-4696-BF14-9D9C23F5CF2E");
            heartbeat.ProcessId = 5948;
            heartbeat.ChangedBy = "ankarlp";
            heartbeat.ChangedDateTime = DateTime.Now;

            IEventHeartbeat heartbeat2 = new EventHeartbeat();
            heartbeat2.HeartbeatId = new Guid("9E0E75B4-D988-4507-B0F0-F81C07751ACF");
            heartbeat2.SubscriberId = new Guid("9E0E75B4-D988-4507-B0F0-F81C07751ACF");
            heartbeat2.ProcessId = 5948;
            heartbeat2.ChangedBy = "buster";
            heartbeat2.ChangedDateTime = DateTime.Now;

            BrokerService broker = new BrokerService();
            broker.EventSubscriptions = new List<IEventSubscriber>();
            broker.EventSubscriptions.Add(new EventSubscriber(new Guid("5C1CF20F-7D04-4696-BF14-9D9C23F5CF2E"), 1, 234, "172.22.1.31", 9090, "ankarlp", DateTime.Now));
            broker.EventSubscriptions.Add(new EventSubscriber(Guid.NewGuid(), 1, 432, "172.22.1.31", 9090, "ankarlp", DateTime.Now));
            broker.EventSubscriptions.Add(new EventSubscriber(new Guid("9E0E75B4-D988-4507-B0F0-F81C07751ACF"), 1, 234, "172.22.1.31", 9091, "buster", DateTime.Now));
            IDictionary<Guid, IEventHeartbeat> dictionary = new Dictionary<Guid, IEventHeartbeat>();
            dictionary.Add(heartbeat.SubscriberId, heartbeat);
            dictionary.Add(heartbeat2.SubscriberId, heartbeat2);
            RunInstanceMethod(typeof(BrokerService), "RemoveStaleSubscribers", broker, new object[] { dictionary });
            RunInstanceMethod(typeof(BrokerService), "RemoveStaleSubscribers", broker, new object[] { dictionary });
            RunInstanceMethod(typeof(BrokerService), "RemoveStaleSubscribers", broker, new object[] { dictionary });
            RunInstanceMethod(typeof(BrokerService), "RemoveStaleSubscribers", broker, new object[] { dictionary });
            Assert.AreEqual(broker.EventSubscriptions.Count, 2);
        }

        public static object RunInstanceMethod(Type t, string strMethod, object objInstance, object[] aobjParams)
        {
            BindingFlags eFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return RunMethod(t, strMethod, objInstance, aobjParams, eFlags);
        } 

        private static object RunMethod(Type t, string strMethod, object objInstance, object[] aobjParams, BindingFlags eFlags)
        {
            MethodInfo m = t.GetMethod(strMethod, eFlags);
            if (m == null)
            {
                throw new ArgumentException("There is no method '" + strMethod + "' for type '" + t + "'.");
            }
            object objRet = m.Invoke(objInstance, aobjParams);
            return objRet;
        } 

        [Test]
        public void TestMessageBroker()
        {

            // *** Du skall här testa Message Broker ***
            // 3. Testa att Skicka in ett filter
            // 4. Från backgrunds tråd skicka ett meddelande.
            // 5. hmmm hur skall detta fungera ???

//            IMessageBroker broker = MessageBroker.GetInstance(_brokerService);
//            using (_mocks.Record())
//            {
//                //this must happend
//                //  Expect.Call(_brokerService.RegisterFilter(_subscriberId, domainObjectId, domainObjectType, startDate, endDate, Environment.UserName);)
//                //    .Return(6);
//            }
//              using (mocks.Playback())
//              {
//                  StringAssert.AreEqualIgnoringCase("6", obj.DoSomething(2, 4));
//              }
        }

        [TearDown]
        private void TearDown()
        {

        }

    }
}
