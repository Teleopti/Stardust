using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class ExtendReduceDaysOffDecisionMakerTest
    {
        private ExtendReduceDaysOffDecisionMaker _target;
        private MockRepository _mocks;
        private IScheduleMatrixLockableBitArrayConverterEx _bitArrayConverter;
        private IScheduleResultDataExtractor _scheduleResultDataExtractor;
        private IDayOffLegalStateValidator _validator;
        private IList<IDayOffLegalStateValidator> _validators;
        private IScheduleMatrixPro _matrix;
        private ILockableBitArray _bitArray;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDayPro _scheduleDayPro3;
        private IScheduleDayPro _scheduleDayPro4;
        private IScheduleDayPro _scheduleDayPro5;
        private IScheduleDayPro _scheduleDayPro6;
        private IScheduleDayPro _scheduleDayPro7;

        [SetUp]
        public void Setup()
        {
			_mocks = new MockRepository();
			_bitArrayConverter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverterEx>();
            _target = new ExtendReduceDaysOffDecisionMaker(_bitArrayConverter);
            _scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            _validator = _mocks.StrictMock<IDayOffLegalStateValidator>();
            _validators = new List<IDayOffLegalStateValidator> { _validator };
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _bitArray = new LockableBitArray(7, false, false);
            _scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro3 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro4 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro5 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro6 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro7 = _mocks.StrictMock<IScheduleDayPro>();
        }

        [Test]
        public void ShouldFindBestDayToLengthenAndShorten()
        {
            _bitArray.Set(1, true);
            _bitArray.Set(6, true);
            using (_mocks.Record())
            {
                commonMocks();
                Expect.Call(_validator.IsValid(_bitArray.ToLongBitArray(), 6)).Return(true).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_scheduleResultDataExtractor.Values()).Return(standardList());
                Expect.Call(_matrix.FullWeeksPeriodDays).Return(fullWeekPeriodDays())
               .Repeat.AtLeastOnce();
            }

            ExtendReduceTimeDecisionMakerResult result;

            using (_mocks.Playback())
            {
                result = _target.Execute(_matrix, _scheduleResultDataExtractor, _validators);
            }

            Assert.AreEqual(new DateOnly(2012, 2, 2), result.DayToLengthen);
            Assert.AreEqual(new DateOnly(2012, 2, 3), result.DayToShorten);
        }

        [Test]
        public void ShouldSkipLockedDays()
        {
            _bitArray.Set(1, true);
            _bitArray.Set(3, true);
            _bitArray.Lock(1, true);
            _bitArray.Lock(2, true);
            using (_mocks.Record())
            {
                commonMocks();
                Expect.Call(_validator.IsValid(_bitArray.ToLongBitArray(), 6)).Return(true).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_scheduleResultDataExtractor.Values()).Return(standardList());
                Expect.Call(_matrix.FullWeeksPeriodDays).Return(fullWeekPeriodDays())
               .Repeat.AtLeastOnce();
            }

            ExtendReduceTimeDecisionMakerResult result;

            using (_mocks.Playback())
            {
                result = _target.Execute(_matrix, _scheduleResultDataExtractor, _validators);
            }

            Assert.AreEqual(new DateOnly(2012, 2, 4), result.DayToLengthen);
            Assert.AreEqual(new DateOnly(2012, 2, 6), result.DayToShorten);
        }

        [Test]
        public void ShouldNotReturnDayToLengthenIfNoUnderStaff()
        {
            _bitArray.SetAll(true);
            _bitArray.Set(0, false);

            using (_mocks.Record())
            {
                commonMocks();
                Expect.Call(_validator.IsValid(_bitArray.ToLongBitArray(), 6)).Return(true).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_scheduleResultDataExtractor.Values()).Return(new List<double?> {1, 32, 32, 1, 10, 10, 0});
                Expect.Call(_matrix.FullWeeksPeriodDays).Return(fullWeekPeriodDays())
               .Repeat.AtLeastOnce();
            }

            ExtendReduceTimeDecisionMakerResult result;

            using (_mocks.Playback())
            {
                result = _target.Execute(_matrix, _scheduleResultDataExtractor, _validators);
            }

            Assert.IsNull(result.DayToLengthen);
            Assert.AreEqual(new DateOnly(2012, 2, 1), result.DayToShorten);
        }

        [Test]
        public void ShouldNotReturnDayToShortenIfNoOverstaff()
        {
            _bitArray.Set(0, true);

            using (_mocks.Record())
            {
                commonMocks();
                Expect.Call(_validator.IsValid(_bitArray.ToLongBitArray(), 6)).Return(true).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_scheduleResultDataExtractor.Values()).Return(new List<double?> { -1, -32, -32, -1, -10, -10, 0 });
                Expect.Call(_matrix.FullWeeksPeriodDays).Return(fullWeekPeriodDays())
               .Repeat.AtLeastOnce();
            }

            ExtendReduceTimeDecisionMakerResult result;

            using (_mocks.Playback())
            {
                result = _target.Execute(_matrix, _scheduleResultDataExtractor, _validators);
            }

            Assert.AreEqual(new DateOnly(2012, 2, 1), result.DayToLengthen);
            Assert.IsNull(result.DayToShorten);
        }

        [Test] 
        public void ShouldNotReturnIfNoValidStateCanBeFound()
        {
            _bitArray.Set(1, true);
            _bitArray.Set(6, true);
            using (_mocks.Record())
            {
                commonMocks();
                Expect.Call(_validator.IsValid(_bitArray.ToLongBitArray(), 6)).Return(false).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_scheduleResultDataExtractor.Values()).Return(standardList());
            }

            ExtendReduceTimeDecisionMakerResult result;

            using (_mocks.Playback())
            {
                result = _target.Execute(_matrix, _scheduleResultDataExtractor, _validators);
            }

            Assert.IsNull(result.DayToLengthen);
            Assert.IsNull(result.DayToShorten);
        }

        private void commonMocks()
        {
            Expect.Call(_bitArrayConverter.Convert(_matrix, false, false)).Return(_bitArray).Repeat.Any();
            Expect.Call(_scheduleDayPro1.Day).Return(new DateOnly(2012, 2, 1)).Repeat.Any();
            Expect.Call(_scheduleDayPro2.Day).Return(new DateOnly(2012, 2, 2)).Repeat.Any();
            Expect.Call(_scheduleDayPro3.Day).Return(new DateOnly(2012, 2, 3)).Repeat.Any();
            Expect.Call(_scheduleDayPro4.Day).Return(new DateOnly(2012, 2, 4)).Repeat.Any();
            Expect.Call(_scheduleDayPro5.Day).Return(new DateOnly(2012, 2, 5)).Repeat.Any();
            Expect.Call(_scheduleDayPro6.Day).Return(new DateOnly(2012, 2, 6)).Repeat.Any();
            Expect.Call(_scheduleDayPro7.Day).Return(new DateOnly(2012, 2, 7)).Repeat.Any();
        }

        private static IList<double?> standardList()
        {
            return new List<double?>{ 1, -32, 32, -1, -10, 10, 0};
        }

        private IScheduleDayPro[] fullWeekPeriodDays()
        {
            return new []
                       {
                           _scheduleDayPro1,
                           _scheduleDayPro2,
                           _scheduleDayPro3,
                           _scheduleDayPro4,
                           _scheduleDayPro5,
                           _scheduleDayPro6,
                           _scheduleDayPro7
                       };
        }
    }
}