using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class ExtendReduceTimeDecisionMakerTest
    {
        private ExtendReduceTimeDecisionMaker _target;
        private MockRepository _mocks;
        private IScheduleResultDataExtractor _scheduleResultDataExtractor;
        private IScheduleMatrixPro _matrix;
        private ILockableBitArray _lockableBitArray;
        private IList<double?> _data;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDayPro _scheduleDayPro3;
        private IScheduleDayPro _scheduleDayPro4;
        private IScheduleDayPro _scheduleDayPro5;
        private IScheduleDayPro[] _days;
	    private IScheduleMatrixLockableBitArrayConverterEx _bitArrayConverter;

	    [SetUp]
        public void Setup()
	    {
			_mocks = new MockRepository();
			_bitArrayConverter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverterEx>();
            _target = new ExtendReduceTimeDecisionMaker(_bitArrayConverter);
            _scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _lockableBitArray = new LockableBitArray(5, false, false, null);
            _data = new List<double?>{-1, 8, 1, -8, 5};
            _scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro3 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro4 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro5 = _mocks.StrictMock<IScheduleDayPro>();
            _days = new [] { _scheduleDayPro1, _scheduleDayPro2, _scheduleDayPro3, _scheduleDayPro4, _scheduleDayPro5 };
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
                result = _target.Execute(_matrix, _scheduleResultDataExtractor);
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
                result = _target.Execute(_matrix, _scheduleResultDataExtractor);
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
                result = _target.Execute(_matrix, _scheduleResultDataExtractor);
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
                result = _target.Execute(_matrix, _scheduleResultDataExtractor);
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
                result = _target.Execute(_matrix, _scheduleResultDataExtractor);
            }

            Assert.IsFalse(result.DayToLengthen.HasValue);
            Assert.IsFalse(result.DayToLengthen.HasValue);
        }

        private void commonMocks()
        {
            Expect.Call(_bitArrayConverter.Convert(_matrix, false, false)).Return(_lockableBitArray);
            Expect.Call(_scheduleResultDataExtractor.Values()).Return(_data).Repeat.AtLeastOnce();
            Expect.Call(_matrix.FullWeeksPeriodDays).Return(_days).Repeat.Any();
            Expect.Call(_scheduleDayPro1.Day).Return(new DateOnly(2011, 1, 1)).Repeat.Any();
            Expect.Call(_scheduleDayPro2.Day).Return(new DateOnly(2011, 1, 2)).Repeat.Any();
            Expect.Call(_scheduleDayPro3.Day).Return(new DateOnly(2011, 1, 3)).Repeat.Any();
            Expect.Call(_scheduleDayPro4.Day).Return(new DateOnly(2011, 1, 4)).Repeat.Any();
            Expect.Call(_scheduleDayPro5.Day).Return(new DateOnly(2011, 1, 5)).Repeat.Any();
        }
    }
}