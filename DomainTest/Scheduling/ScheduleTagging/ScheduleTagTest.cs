using System;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.ScheduleTagging
{
    [TestFixture]
    public class ScheduleTagTest
    {
        private IScheduleTag _target;

        [SetUp]
        public void Setup()
        {
            _target = new Domain.Scheduling.ScheduleTagging.ScheduleTag();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsFalse(_target.IsDeleted);
            _target.SetDeleted();
            Assert.IsTrue(_target.IsDeleted);
			Assert.That(_target.Description, Is.Null.Or.Empty);
			_target.Description = "012345678912345";
            Assert.AreEqual("012345678912345", _target.Description);
        }

        [Test]
        public void ShouldThrowIfDescriptionLongerThan15()
        {
			Assert.Throws<ArgumentException>(() => _target.Description = "0123456789123456");
        }
    }
}