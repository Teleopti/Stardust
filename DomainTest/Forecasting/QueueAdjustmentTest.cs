using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class QueueAdjustmentTest
    {
        private QueueAdjustment _target;

        [SetUp]
        public void Setup()
        {
            _target = new QueueAdjustment();
        }

        [Test]
        public void VerifyStruct()
        {
            Assert.AreEqual(new QueueAdjustment(),_target);
            Assert.IsTrue(new QueueAdjustment()==_target);
            Assert.IsFalse(new QueueAdjustment() != _target);
            Assert.IsFalse(new QueueAdjustment().Equals(null));
        }

        [Test]
        public void VerifyHashCode()
        {
            var dictionary = new Dictionary<QueueAdjustment, int>();
            dictionary.Add(_target,5);
            Assert.AreEqual(5,dictionary[_target]);
        }

        [Test]
        public void VerifyOfferedTasks()
        {
            var offeredTasks = new Percent(0.9);
            _target.OfferedTasks = offeredTasks;
            Assert.AreEqual(offeredTasks,_target.OfferedTasks);
        }

        [Test]
        public void VerifyOverflowedIn()
        {
            var overflowIn = new Percent(0.2);
            _target.OverflowIn = overflowIn;
            Assert.AreEqual(overflowIn, _target.OverflowIn);
        }

        [Test]
        public void VerifyOverflowOut()
        {
            var overflowOut = new Percent(0.3);
            _target.OverflowOut = overflowOut;
            Assert.AreEqual(overflowOut,_target.OverflowOut);
        }

        [Test]
        public void VerifyAbandonedShort()
        {
            var abandondedShort = new Percent(0.1);
            _target.AbandonedShort = abandondedShort;
            Assert.AreEqual(abandondedShort,_target.AbandonedShort);
        }
    }
}
