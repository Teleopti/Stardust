using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.DomainTest.Scheduling.ScheduleTagging
{
    [TestFixture]
    public class NullScheduleTagTest
    {
        private NullScheduleTag _target;

        [SetUp]
        public void Setup()
        {
            _target = NullScheduleTag.Instance;
        }

        [Test]
        public void DescriptionSetterShouldNotWork()
        {
            _target.Description = "bblloovv";
            Assert.AreNotEqual("bblloovv", _target.Description);
        }

        [Test]
        public void DescriptionShouldReturnString()
        {
            Assert.IsNotEmpty(_target.Description);
        }

        [Test]
        public void EqualsShouldReturnTrueIfComparedWithNullScheduleTag()
        {
            var other = new ScheduleTag();
            Assert.IsFalse(_target.Equals(other));
            Assert.IsTrue(_target.Equals(NullScheduleTag.Instance));
        }

        [Test]
        public void SetIdShouldNotWork()
        {
            Assert.IsNull(_target.Id);
            _target.SetId(Guid.NewGuid());
            Assert.IsNull(_target.Id);
            _target.ClearId();
            Assert.IsNull(_target.Id);
        }

        [Test]
        public void VerifyChangedInfo()
        {
            Assert.IsNull(_target.CreatedBy);
            Assert.IsNull(_target.CreatedOn);
            Assert.IsNull(_target.UpdatedBy);
            Assert.IsNull(_target.UpdatedOn);
        }

        [Test]
        public void SetDeletedShouldDoNothing()
        {
            Assert.IsFalse(_target.IsDeleted);
            _target.SetDeleted();
            Assert.IsFalse(_target.IsDeleted);
        }
    }
}