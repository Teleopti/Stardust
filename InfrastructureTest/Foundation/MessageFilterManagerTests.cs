using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Interfaces.Domain;

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
            Assert.IsTrue(filterManager.FilterDictionary.Keys.Contains(typeof(IJobResultProgress)));
            Assert.AreEqual(1, filterManager.FilterDictionary[typeof(IJobResultProgress)].Count);
            Assert.AreEqual(1, filterManager.FilterDictionary[typeof(IScheduleChangedInDefaultScenario)].Count);
            Assert.AreEqual(1, filterManager.FilterDictionary[typeof(IScheduleChangedEvent)].Count);
            Assert.AreEqual(1, filterManager.FilterDictionary[typeof(Scenario)].Count);
        }
    }
}