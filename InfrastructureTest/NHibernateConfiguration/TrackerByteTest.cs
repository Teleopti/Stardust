using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
    [TestFixture]
    [Category("BucketB")]
    public class TrackerByteTest
    {
        private ITracker _target;
       

        [SetUp]
        public void Setup()
        {
            _target = Tracker.CreateDayTracker();
        }

        [Test]
        public void VerifyReplaceReturnsInstance()
        {
            Assert.AreEqual(Tracker.CreateDayTracker(), new TrackerByte().Replace(_target, this, this));   
        }

        [Test]
        public void TestAssemble()
        {
            Assert.AreEqual(Tracker.CreateDayTracker(), new TrackerByte().Assemble(_target, this));   
        }

        [Test]
        public void TestDisassemble()
        {
            Assert.AreEqual(Tracker.CreateDayTracker(), new TrackerByte().Disassemble(_target));   
        }

        [Test]
        public void TestGetHashCode()
        {
            Assert.AreEqual(Tracker.CreateDayTracker().GetHashCode(), new TrackerByte().GetHashCode(_target.GetHashCode()));
        }
    }
}