using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    [Category("BucketB")]
    public class MessageFilterManagerTests
    {
        [Test]
        public void MessageFilterManagerTest()
        {
            MessageFilterManager filterManager = MessageFilterManager.Instance;
            Assert.IsTrue(filterManager.HasType(typeof(IStatisticTask)));
            Assert.AreEqual(filterManager.LookupType(typeof(PublicNote)),typeof(IPersistableScheduleData));
            Assert.AreEqual(filterManager.LookupTypeToSend(typeof(PublicNote)), typeof(IPublicNote).AssemblyQualifiedName);
        }
    }
}