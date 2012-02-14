using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class UpdatedAfterSpecificationTest
    {
        private UpdatedAfterSpecification _target;
        private MockRepository _mocks;
        private DateTime _dateTime;
        private TimeSpan _diff;

        [SetUp]
        public void Setup()
        {
            _diff = TimeSpan.FromMinutes(1);
            _mocks = new MockRepository();
            _dateTime = new DateTime(2001,1,1,1,1,1,0);
            _target = new UpdatedAfterSpecification(_dateTime);
        }

        [Test]
        public void VerifySatisfiedIfUpdatedAfterCertainDateTime()
        {
            IChangeInfo before = _mocks.StrictMock<IChangeInfo>();
            IChangeInfo after = _mocks.StrictMock<IChangeInfo>();

            using(_mocks.Record())
            {
                Expect.Call(before.UpdatedOn).Return(_dateTime.Subtract(_diff));
                Expect.Call(after.UpdatedOn).Return(_dateTime.Add(_diff));
            }
            using(_mocks.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(before));
                Assert.IsTrue(_target.IsSatisfiedBy(after));
            }
        }

        [Test]
        public void VerifyAlwaysSatisfiedIfUpdatedOnIsNull()
        {
            IChangeInfo changeInfo = _mocks.StrictMock<IChangeInfo>();
           
            using (_mocks.Record())
            {
                Expect.Call(changeInfo.UpdatedOn).Return(null);
            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(changeInfo));
            }
        }

        [Test]
        public void VerifyDateTimeKindMustBeEqual()
        {
            DateTime dateTimeWithinButwithWrongDateTimeKind = new DateTime(_dateTime.Ticks,DateTimeKind.Utc).Add(_diff);
            Assert.AreNotEqual(dateTimeWithinButwithWrongDateTimeKind.Kind, _dateTime.Kind);

            IChangeInfo changeInfo = _mocks.StrictMock<IChangeInfo>();

            using (_mocks.Record())
            {
                Expect.Call(changeInfo.UpdatedOn).Return(dateTimeWithinButwithWrongDateTimeKind);
            }
            using (_mocks.Playback())
            {
                Assert.Throws<ArgumentException>(()=>_target.IsSatisfiedBy(changeInfo));
            }
        }

    }
}
