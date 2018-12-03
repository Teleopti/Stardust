using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class GridlockRemoverForDeleteTest
    {
        private IGridlockRemoverForDelete _target;
        private MockRepository _mocks;
        private IGridlockManager _gridlockManager;
        private IScheduleDay _day1;
        private IScheduleDay _day2;
        private IPerson _person;
        private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod1;
        private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod2;
        private DateTimePeriod _period1;
        private DateTimePeriod _period2;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _gridlockManager = new GridlockManager();
            _target = new GridlockRemoverForDelete(_gridlockManager);
            _day1 = _mocks.StrictMock<IScheduleDay>();
            _day2 = _mocks.StrictMock<IScheduleDay>();
            _person = PersonFactory.CreatePerson();
            _dateOnlyAsDateTimePeriod1 = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            _dateOnlyAsDateTimePeriod2 = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            _period1 = new DateTimePeriod(2011, 1, 1, 2011, 1, 1);
            _period2 = new DateTimePeriod(2011, 1, 2, 2011, 1, 2);
        }

        [Test]
        public void ShouldNotRemoveIfOnlyWriteProtectLockAndIAmAllowedToWorkWithWriteProtected()
        {
            IList<IScheduleDay> source = new List<IScheduleDay>{ _day1, _day2};
            using(_mocks.Record())
            {
                commonMocks();
            }

            IList<IScheduleDay> ret;
            using(_mocks.Playback())
            {
				using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
				{
					_gridlockManager.AddLock(_day1, LockType.WriteProtected);
					ret = _target.RemoveLocked(source);
				}
			}
            Assert.AreEqual(2, ret.Count);
        }

        [Test]
        public void ShouldRemoveIfNormalLockAndWriteProtectLockAndIAmAllowedToWorkWithWriteProtected()
        {
            IList<IScheduleDay> source = new List<IScheduleDay> { _day1, _day2 };
            using (_mocks.Record())
            {
                commonMocks();
            }

            IList<IScheduleDay> ret;
            using (_mocks.Playback())
            {
                _gridlockManager.AddLock(_day1, LockType.WriteProtected);
                _gridlockManager.AddLock(_day1, LockType.Normal);
                ret = _target.RemoveLocked(source);
            }
            Assert.AreEqual(1, ret.Count);
            Assert.AreSame(_day2, ret[0]);
        }

        [Test]
        public void ShouldNotRemoveIfOnlyOutsidePeriodLock()
        {
            IList<IScheduleDay> source = new List<IScheduleDay> { _day1, _day2 };
            using (_mocks.Record())
            {
                commonMocks();
            }

            IList<IScheduleDay> ret;
            using (_mocks.Playback())
            {
                _gridlockManager.AddLock(_day1, LockType.OutsidePersonPeriod);
                ret = _target.RemoveLocked(source);
            }
            Assert.AreEqual(2, ret.Count);
        }

        [Test]
        public void ShouldRemoveIfOutsidePeriodLockAndNormal()
        {
            IList<IScheduleDay> source = new List<IScheduleDay> { _day1, _day2 };
            using (_mocks.Record())
            {
                commonMocks();
            }

            IList<IScheduleDay> ret;
            using (_mocks.Playback())
            {
                _gridlockManager.AddLock(_day1, LockType.OutsidePersonPeriod);
                _gridlockManager.AddLock(_day1, LockType.Normal);
                ret = _target.RemoveLocked(source);
            }
            Assert.AreEqual(1, ret.Count);
            Assert.AreSame(_day2, ret[0]);
        }

        private void commonMocks()
        {
            Expect.Call(_day1.Person).Return(_person).Repeat.Any();
            Expect.Call(_day2.Person).Return(_person).Repeat.Any();
            Expect.Call(_day1.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod1).Repeat.Any();
            Expect.Call(_day2.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod2).Repeat.Any();
            Expect.Call(_dateOnlyAsDateTimePeriod1.DateOnly).Return(new DateOnly(2011, 1, 1)).Repeat.Any();
            Expect.Call(_dateOnlyAsDateTimePeriod2.DateOnly).Return(new DateOnly(2011, 1, 2)).Repeat.Any();
            Expect.Call(_day1.Period).Return(_period1).Repeat.Any();
            Expect.Call(_day2.Period).Return(_period2).Repeat.Any();
            Expect.Call(_day1.TimeZone).Return(TimeZoneInfoFactory.UtcTimeZoneInfo()).Repeat.Any();
            Expect.Call(_day2.TimeZone).Return(TimeZoneInfoFactory.UtcTimeZoneInfo()).Repeat.Any();
            Expect.Call(_dateOnlyAsDateTimePeriod1.Period()).Return(_period1).Repeat.Any();
            Expect.Call(_dateOnlyAsDateTimePeriod2.Period()).Return(_period2).Repeat.Any();
        }
    }
}