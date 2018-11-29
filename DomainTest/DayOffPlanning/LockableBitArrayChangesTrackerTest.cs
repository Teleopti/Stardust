using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class LockableBitArrayChangesTrackerTest
    {
        private LockableBitArrayChangesTracker _target;
        private MockRepository _mocks;
        private IScheduleMatrixPro _matrix;
        private ILockableBitArray _bitArrayOriginal;
        private ILockableBitArray _bitArrayWorking;
        IScheduleDayPro _scheduleDay1;
        IScheduleDayPro _scheduleDay2;
        IScheduleDayPro _scheduleDay3;
        IScheduleDayPro _scheduleDay4;
        IScheduleDayPro _scheduleDay5;
        IScheduleDayPro _scheduleDay6;
        IScheduleDayPro _scheduleDay7;
        IScheduleDayPro _scheduleDay8;
        IScheduleDayPro _scheduleDay9;
        IScheduleDayPro _scheduleDay10;
        IScheduleDayPro _scheduleDay11;
        IScheduleDayPro _scheduleDay12;
        IScheduleDayPro _scheduleDay13;
        IScheduleDayPro _scheduleDay14;
        private IScheduleDayPro[] _outerWeekList;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay2 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay3 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay4 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay5 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay6 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay7 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay8 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay9 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay10 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay11 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay12 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay13 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay14 = _mocks.StrictMock<IScheduleDayPro>();

            _outerWeekList = new []
                            {
                                _scheduleDay1,
                                _scheduleDay2,
                                _scheduleDay3,
                                _scheduleDay4,
                                _scheduleDay5,
                                _scheduleDay6,
                                _scheduleDay7,
                                _scheduleDay8,
                                _scheduleDay9,
                                _scheduleDay10,
                                _scheduleDay11,
                                _scheduleDay12,
                                _scheduleDay13,
                                _scheduleDay14
                            };


            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
        }

        [Test]
        public void VerifyDayOffChanges()
        {
            const bool considerWeekBefore = false;
            _target = new LockableBitArrayChangesTracker();
            _bitArrayOriginal = new LockableBitArray(7, considerWeekBefore, false, null);
            _bitArrayOriginal.Set(2, true);
            _bitArrayWorking = new LockableBitArray(7, considerWeekBefore, false, null);
            _bitArrayWorking.Set(3, true);
            using (_mocks.Record())
            {
                Expect.Call(_matrix.OuterWeeksPeriodDays).Return(_outerWeekList)
                    .Repeat.Any();
                Expect.Call(_scheduleDay10.Day).Return(new DateOnly(2010, 1, 3)).Repeat.Once();
                Expect.Call(_scheduleDay11.Day).Return(new DateOnly(2010, 1, 4)).Repeat.Once();
               
            }

            using (_mocks.Playback())
            {
                IList<DateOnly> result = _target.DayOffChanges(_bitArrayOriginal, _bitArrayWorking, _matrix, considerWeekBefore);
                Assert.AreEqual(2, result.Count);
            }
        }

        [Test]
        public void VerifyDayOffChangesConsiderWeekBefore()
        {
            const bool considerWeekBefore = true;
            _target = new LockableBitArrayChangesTracker();
            _bitArrayOriginal = new LockableBitArray(14, considerWeekBefore, false, null);
            _bitArrayOriginal.Set(2, true);
            _bitArrayWorking = new LockableBitArray(14, considerWeekBefore, false, null);
            _bitArrayWorking.Set(3, true);
            using (_mocks.Record())
            {
                Expect.Call(_matrix.OuterWeeksPeriodDays).Return(_outerWeekList)
                    .Repeat.Any();
                Expect.Call(_scheduleDay3.Day).Return(new DateOnly(2010, 1, 3)).Repeat.Once();
                Expect.Call(_scheduleDay4.Day).Return(new DateOnly(2010, 1, 4)).Repeat.Once();

            }

            using (_mocks.Playback())
            {
                IList<DateOnly> result = _target.DayOffChanges(_bitArrayOriginal, _bitArrayWorking, _matrix, considerWeekBefore);
                Assert.AreEqual(2, result.Count);
            }
        }

    }
}
