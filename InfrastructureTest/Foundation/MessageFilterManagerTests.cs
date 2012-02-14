using NUnit.Framework;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Composites;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    [Category("LongRunning")]
    public class MessageFilterManagerTests
    {
        [Test]
        public void MessageFilterManagerTest()
        {
            MessageFilterManager filterManager = MessageFilterManager.Instance;
            Assert.IsNotNull(filterManager.FilterDictionary);
            Assert.IsTrue(filterManager.FilterDictionary.Keys.Contains(typeof(IStatisticTask)));
            Assert.AreEqual(1,filterManager.FilterDictionary[typeof(IStatisticTask)].Count);
            Assert.IsTrue(filterManager.FilterDictionary.Keys.Contains(typeof(IExternalAgentState)));
            Assert.AreEqual(1, filterManager.FilterDictionary[typeof(IExternalAgentState)].Count);
            Assert.IsTrue(filterManager.FilterDictionary.Keys.Contains(typeof(IJobResultProgress)));
            Assert.AreEqual(1, filterManager.FilterDictionary[typeof(IJobResultProgress)].Count);
            Assert.AreEqual(1, filterManager.FilterDictionary[typeof(IEventHeartbeat)].Count);
        }
    }
}