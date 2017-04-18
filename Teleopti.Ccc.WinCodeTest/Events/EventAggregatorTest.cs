using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events;

namespace Teleopti.Ccc.WinCodeTest.Events
{
    [TestFixture]
    public class EventAggregatorTest
    {
        private EventSubscriber _subscriber;
        private EventPublisher _publisher;
        private IEventAggregator _eventAggregator;

        [SetUp]
        public void Setup()
        {
            _eventAggregator = new EventAggregator();
            _publisher = new EventPublisher();
            _subscriber = new EventSubscriber(_eventAggregator);
        }
        [Test]
        public void VerifyCanPublishAndSubscribeEventFromToDifferentClasses()
        {


            Assert.IsFalse(_subscriber.StringEventRecieved);
            _publisher.MethodThatTriggersEvent("topic",_eventAggregator);
            Assert.IsTrue(_subscriber.StringEventRecieved);
            Assert.AreEqual("topic",_subscriber.EventTopic);
            Assert.AreEqual(_publisher.EventValue,_subscriber.EventValue);
        }

        internal class EventSubscriber
        {
            internal bool StringEventRecieved;
            internal string EventValue;
            internal string EventTopic;

            internal EventSubscriber(IEventAggregator aggregator)
            {
                aggregator.GetEvent<GenericEvent<string>>().Subscribe(s =>
                {
                   StringEventRecieved = true;
                   EventValue = s.Value;
                   EventTopic = s.Topic; 
                });
            }

          
        }

        internal class EventPublisher
        {
            internal string EventValue = "eventValue";

            internal void MethodThatTriggersEvent(string topic,IEventAggregator eventAggregator)
            {
                EventValue.PublishEvent(topic, eventAggregator);

            }
        }
   
    }
}
