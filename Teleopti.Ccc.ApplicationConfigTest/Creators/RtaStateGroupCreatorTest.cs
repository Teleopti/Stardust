using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Creators
{
    [TestFixture]
    [Category("LongRunning")]
    public class RtaStateGroupCreatorTest
    {
        private RtaStateGroupCreator _target;

        [SetUp]
        public void Setup()
        {
            _target = new RtaStateGroupCreator("Test.xml");
        }
        
        [Test]
        public void VerifyCanCreateRta()
        {
            IList<IRtaStateGroup> rtaStateGroups = _target.RtaGroupCollection;

            Assert.AreEqual(2,rtaStateGroups.Count);

            Assert.AreEqual("Symposium", rtaStateGroups[0].Name);
            Assert.AreEqual(3, rtaStateGroups[0].StateCollection.Count);
            
            Assert.AreEqual("Avaya", rtaStateGroups[1].Name);
            Assert.AreEqual(3, rtaStateGroups[1].StateCollection.Count);
        }
    }
}
