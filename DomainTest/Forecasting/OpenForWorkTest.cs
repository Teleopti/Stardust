using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class OpenForWorkTest
    {
        private OpenForWork _target;
        
        [SetUp]
        public void Setup()
        {
            _target = new OpenForWork(true, true);
        }

        [Test]
        public void ShouldSetValue()
        {
            Assert.That(_target.IsOpen, Is.True);
            Assert.That(_target.IsOpenForIncomingWork, Is.True);
        }

        [Test]
        public void ShouldCompareTwoInstances()
        {
            var openForWork = new OpenForWork();
            Assert.That(_target.Equals(openForWork), Is.False);

            var openForWork1 = new OpenForWork(true, true);
            Assert.That(_target.Equals(openForWork1), Is.True);
        }

        [Test]
        public void ShouldDetectNullValue()
        {
            Assert.That(_target.Equals(null), Is.False);
        }
    }
}
