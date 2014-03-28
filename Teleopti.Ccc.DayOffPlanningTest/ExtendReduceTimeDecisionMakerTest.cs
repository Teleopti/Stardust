using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Secret;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SecretTest
{
    [TestFixture]
    public class ExtendReduceTimeDecisionMakerTest
    {
        private IExtendReduceTimeDecisionMaker _target;
        private MockRepository _mocks;
        private IScheduleMatrixLockableBitArrayConverter _scheduleMatrixLockableBitArrayConverter;
        private IScheduleResultDataExtractor _scheduleResultDataExtractor;
        private IScheduleMatrixPro _matrix;
        private ILockableBitArray _lockableBitArray;
        private IList<double?> _data;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDayPro _scheduleDayPro3;
        private IScheduleDayPro _scheduleDayPro4;
        private IScheduleDayPro _scheduleDayPro5;
        private IList<IScheduleDayPro> _days;
            
        [SetUp]
        public void Setup()
        {
            _target = new ExtendReduceTimeDecisionMaker();
            _mocks = new MockRepository();
            _scheduleMatrixLockableBitArrayConverter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
            _scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _lockableBitArray = new LockableBitArray(5, false, false, null);
            _data = new List<double?>{-1, 8, 1, -8, 5};
            _scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro3 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro4 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro5 = _mocks.StrictMock<IScheduleDayPro>();
            _days = new List<IScheduleDayPro> { _scheduleDayPro1, _scheduleDayPro2, _scheduleDayPro3, _scheduleDayPro4, _scheduleDayPro5 };
        }

        [Test]
        public void ShouldFindCorrectDays()
        {
            using(_mocks.Record())
            {
               commonMocks();
            }

            ExtendReduceTimeDecisionMakerResult result;

            using(_mocks.Playback())
            {
                result = _target.Execute(_scheduleMatrixLockableBitArrayConverter, _scheduleResultDataExtractor);
            }

            Assert.AreEqual(new DateOnly(2011, 1, 4), result.DayToLengthen.Value);
            Assert.AreEqual(new DateOnly(2011, 1, 2), result.DayToShorten.Value);
        }

        [Test]
        public void ShouldSkipDaysOff()
        {
            _lockableBitArray.Set(1, true);
            using (_mocks.Record())
            {
                commonMocks();
            }

            ExtendReduceTimeDecisionMakerResult result;

            using (_mocks.Playback())
            {
                result = _target.Execute(_scheduleMatrixLockableBitArrayConverter, _scheduleResultDataExtractor);
            }

            Assert.AreEqual(new DateOnly(2011, 1, 4), result.DayToLengthen.Value);
            Assert.AreEqual(new DateOnly(2011, 1, 5), result.DayToShorten.Value);
        }

        [Test]
        public void ShouldNotReturnDateToShortenIfNoOverstaff()
        {
            _data = new List<double?> { -1, -9, -1, -8, -5 };
            using (_mocks.Record())
            {
                commonMocks();
            }

            ExtendReduceTimeDecisionMakerResult result;

            using (_mocks.Playback())
            {
                result = _target.Execute(_scheduleMatrixLockableBitArrayConverter, _scheduleResultDataExtractor);
            }

            Assert.AreEqual(new DateOnly(2011, 1, 2), result.DayToLengthen.Value);
            Assert.IsFalse(result.DayToShorten.HasValue);
        }

        [Test]
        public void ShouldNotReturnDateToLengthenIfNoUnderStaff()
        {
            _data = new List<double?> { 1, 8, 1, 9, 5 };
            using (_mocks.Record())
            {
                commonMocks();
            }

            ExtendReduceTimeDecisionMakerResult result;

            using (_mocks.Playback())
            {
                result = _target.Execute(_scheduleMatrixLockableBitArrayConverter, _scheduleResultDataExtractor);
            }

            Assert.IsFalse(result.DayToLengthen.HasValue);
            Assert.AreEqual(new DateOnly(2011, 1, 4), result.DayToShorten.Value);
        }

        [Test]
        public void ShouldNotReturnDatesIfAllLocked()
        {
            _lockableBitArray.Lock(0, true);
            _lockableBitArray.Lock(1, true);
            _lockableBitArray.Lock(2, true);
            _lockableBitArray.Lock(3, true);
            _lockableBitArray.Lock(4, true);

            using (_mocks.Record())
            {
                commonMocks();
            }

            ExtendReduceTimeDecisionMakerResult result;

            using (_mocks.Playback())
            {
                result = _target.Execute(_scheduleMatrixLockableBitArrayConverter, _scheduleResultDataExtractor);
            }

            Assert.IsFalse(result.DayToLengthen.HasValue);
            Assert.IsFalse(result.DayToLengthen.HasValue);
        }

        private void commonMocks()
        {
            Expect.Call(_scheduleMatrixLockableBitArrayConverter.SourceMatrix).Return(_matrix);
            Expect.Call(_scheduleMatrixLockableBitArrayConverter.Convert(false, false)).Return(_lockableBitArray);
            Expect.Call(_scheduleResultDataExtractor.Values()).Return(_data).Repeat.AtLeastOnce();
            Expect.Call(_matrix.FullWeeksPeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(_days)).Repeat.Any();
            Expect.Call(_scheduleDayPro1.Day).Return(new DateOnly(2011, 1, 1)).Repeat.Any();
            Expect.Call(_scheduleDayPro2.Day).Return(new DateOnly(2011, 1, 2)).Repeat.Any();
            Expect.Call(_scheduleDayPro3.Day).Return(new DateOnly(2011, 1, 3)).Repeat.Any();
            Expect.Call(_scheduleDayPro4.Day).Return(new DateOnly(2011, 1, 4)).Repeat.Any();
            Expect.Call(_scheduleDayPro5.Day).Return(new DateOnly(2011, 1, 5)).Repeat.Any();
        }
    }
}