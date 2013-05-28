using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class MainShiftActivityLayerTest
    {
        private MainShiftActivityLayer _target;

        [SetUp]
        public void Setup()
        {
            _target = new MainShiftActivityLayer(ActivityFactory.CreateActivity("test"),new DateTimePeriod(2010,1,1,2010,1,1));
        }

        [Test]
        public void HasNonpublicConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void ShouldNotAcceptPeriodWithSeconds()
        {
            _target = new MainShiftActivityLayer(_target.Payload, _target.Period.MovePeriod(TimeSpan.FromSeconds(4)));
        }

    }
}