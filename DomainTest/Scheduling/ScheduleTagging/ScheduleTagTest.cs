using System;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
            Assert.IsFalse(((IDeleteTag)_target).IsDeleted);
            ((IDeleteTag)_target).SetDeleted();
            Assert.IsTrue(((IDeleteTag)_target).IsDeleted);
            Assert.IsNullOrEmpty(_target.Description);
            _target.Description = "012345678912345";
            Assert.AreEqual("012345678912345", _target.Description);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowIfDescriptionLongerThan15()
        {
            _target.Description = "0123456789123456";
        }
    }
}