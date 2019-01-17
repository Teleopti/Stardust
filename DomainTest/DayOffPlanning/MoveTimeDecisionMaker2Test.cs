using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class MoveTimeDecisionMaker2Test
    {
        private MoveTimeDecisionMaker2 _target;
        private MockRepository _mocks;

        private IScheduleMatrixPro _scheduleMatrix;
        private IScheduleMatrixLockableBitArrayConverterEx _matrixConverter;
        private IScheduleResultDataExtractor _dataExtractor;
        private IList<double?> _values;
        private IScheduleDayPro _scheduleDayPro0201Mon;
        private IScheduleDayPro _scheduleDayPro0202Tue;
        private IScheduleDayPro _scheduleDayPro0203Wed;
        private IScheduleDayPro _scheduleDayPro0204Thu;
        private IScheduleDayPro _scheduleDayPro0205Fri;
        private IScheduleDayPro _scheduleDayPro0206Sat;
        private IScheduleDayPro _scheduleDayPro0207Sun;
        private IScheduleDayPro _scheduleDayPro0208Mon;
        private IScheduleDayPro _scheduleDayPro0209Tue;
        private IScheduleDayPro _scheduleDayPro0210Wed;
        private IScheduleDayPro _scheduleDayPro0211Thu;
        private IScheduleDayPro _scheduleDayPro0212Fri;
        private IScheduleDayPro _scheduleDayPro0213Sat;
        private IScheduleDayPro _scheduleDayPro0214Sun;
        private IScheduleDayPro _scheduleDayPro0215Mon;
        private IScheduleDayPro _scheduleDayPro0216Tue;
        private IScheduleDayPro _scheduleDayPro0217Wed;
        private IScheduleDayPro _scheduleDayPro0218Thu;
        private IScheduleDayPro _scheduleDayPro0219Fri;
        private IScheduleDayPro _scheduleDayPro0220Sat;
        private IScheduleDayPro _scheduleDayPro0221Sun;

        private IScheduleDay _schedulePartDayOff;
        private IScheduleDay _schedulePartShortMainShift;
        private IScheduleDay _schedulePartLongMainShift;

        [SetUp]
        public void Setup()
        {
			_mocks = new MockRepository();
			_matrixConverter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverterEx>();
            _target = new MoveTimeDecisionMaker2(_matrixConverter);

            _scheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _dataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            _values = new List<double?> { -20, -10, 10, 30 };

            _scheduleDayPro0201Mon = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0202Tue = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0203Wed = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0204Thu = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0205Fri = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0206Sat = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0207Sun = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0208Mon = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0209Tue = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0210Wed = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0211Thu = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0212Fri = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0213Sat = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0214Sun = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0215Mon = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0216Tue = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0217Wed = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0218Thu = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0219Fri = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0220Sat = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0221Sun = _mocks.StrictMock<IScheduleDayPro>();

            _schedulePartDayOff = _mocks.StrictMock<IScheduleDay>();
            _schedulePartShortMainShift = _mocks.StrictMock<IScheduleDay>();
            _schedulePartLongMainShift = _mocks.StrictMock<IScheduleDay>();

        }

        #region Simple tests

        [Test]
        public void SimpleNotLockedTest()
        {

            ILockableBitArray bitArray = createBitArray();

            using (_mocks.Record())
            {
                simpleMatrixExpectations();

                Expect.Call(_matrixConverter.Convert(_scheduleMatrix, false, false)).Return(bitArray).Repeat.Any();
                Expect.Call(_dataExtractor.Values()).Return(_values).Repeat.Any();
            }

            // day counterparts are > febr 9, 10, 11, 12
            IList<DateOnly> result = _target.Execute(_scheduleMatrix, _dataExtractor);
            Assert.AreEqual(2, result.Count);

            DateOnly mostUnderStaffingDay = new DateOnly(2010, 02, 9);
            DateOnly mostOverStaffingDay = new DateOnly(2010, 02, 12);

            Assert.AreEqual(mostUnderStaffingDay, result[0]);
            Assert.AreEqual(mostOverStaffingDay, result[1]);
        }

        [Test]
        public void VerifyDecisionMakerNeverGiveMeTwoSameDates()
        {
            var bitArray = createBitArray();
            var values = new List<double?> { -20, -20, -20, -20 };
            using (_mocks.Record())
            {
                simpleMatrixExpectations();
                Expect.Call(_matrixConverter.Convert(_scheduleMatrix, false, false)).Return(bitArray).Repeat.Any();
                Expect.Call(_dataExtractor.Values()).Return(values).Repeat.Any();
            }

            // day counterparts are > febr 9, 10, 11, 12
            var result = _target.Execute(_scheduleMatrix, _dataExtractor);
            Assert.AreEqual(2, result.Count);
            Assert.AreNotEqual(result[0], result[1]);
        }

        [Test]
        public void SimpleLockedTest()
        {

            ILockableBitArray bitArray = createBitArray();
            bitArray.Lock(1, true);
            bitArray.Lock(4, true);

            using (_mocks.Record())
            {
                simpleMatrixExpectations();

                Expect.Call(_matrixConverter.Convert(_scheduleMatrix, false, false)).Return(bitArray).Repeat.Any();
                Expect.Call(_dataExtractor.Values()).Return(_values).Repeat.Any();
            }

            // day counterparts are > febr 9, 10, 11, 12
            IList<DateOnly> result = _target.Execute(_scheduleMatrix, _dataExtractor);
            Assert.AreEqual(2, result.Count);

            DateOnly mostUnderStaffingDay = new DateOnly(2010, 02, 10);
            DateOnly mostOverStaffingDay = new DateOnly(2010, 02, 11);

            Assert.AreEqual(mostUnderStaffingDay, result[0]);
            Assert.AreEqual(mostOverStaffingDay, result[1]);
        }

        private void simpleMatrixExpectations()
        {
            IDictionary<DateOnly, IScheduleDayPro> unlockedDaysDictionary = new Dictionary<DateOnly, IScheduleDayPro>();
            IDictionary<DateOnly, IScheduleDayPro> fullWeekPeriodDictionary = new Dictionary<DateOnly, IScheduleDayPro>();


            IList<IScheduleDayPro> outerPeriodList = new List<IScheduleDayPro>
                                                    {
                                                        _scheduleDayPro0201Mon,
                                                        _scheduleDayPro0202Tue,
                                                        _scheduleDayPro0203Wed,
                                                        _scheduleDayPro0204Thu,
                                                        _scheduleDayPro0205Fri,
                                                        _scheduleDayPro0206Sat,
                                                        _scheduleDayPro0207Sun,

                                                        _scheduleDayPro0208Mon,
                                                        _scheduleDayPro0209Tue,
                                                        _scheduleDayPro0210Wed,
                                                        _scheduleDayPro0211Thu,
                                                        _scheduleDayPro0212Fri,
                                                        _scheduleDayPro0213Sat,
                                                        _scheduleDayPro0214Sun,
                                                        
                                                        _scheduleDayPro0215Mon,
                                                        _scheduleDayPro0216Tue,
                                                        _scheduleDayPro0217Wed,
                                                        _scheduleDayPro0218Thu,
                                                        _scheduleDayPro0219Fri,
                                                        _scheduleDayPro0220Sat,
                                                        _scheduleDayPro0221Sun
                                                    };




            for (int i = 0; i < 21; i++)
            {
                DateOnly currentDate = new DateOnly(2010, 02, 01).AddDays(i);
                IScheduleDayPro scheduleDay = outerPeriodList[i];
                Expect.Call(scheduleDay.DaySchedulePart()).Return(_schedulePartShortMainShift).Repeat.Any();
                Expect.Call(scheduleDay.Day).Return(currentDate).Repeat.Any();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(currentDate)).Return(outerPeriodList[i]).Repeat.Any();

                if (i >= 7 && i <= 13)
                {
                    fullWeekPeriodDictionary.Add(currentDate, scheduleDay);
                }

                if (i >= 8 && i <= 11)
                {
                    unlockedDaysDictionary.Add(currentDate, scheduleDay);
                }
            }

            Expect.Call(_scheduleMatrix.FullWeeksPeriodDays)
                .Return(fullWeekPeriodDictionary.Values.ToArray()).Repeat.Any();
            Expect.Call(_scheduleMatrix.UnlockedDays)
                .Return(unlockedDaysDictionary.Values.ToHashSet()).Repeat.Any();
            addWorkShiftExpectationsToScheduleDay(_schedulePartShortMainShift, new TimeSpan());

        }

        #endregion

        #region Shift validation tests

        [Test]
        public void VerifyOnlyMainShiftsPlay()
        {

            ILockableBitArray bitArray = createBitArray();

            using (_mocks.Record())
            {
                matrixExpectationsWithMainShifts();
                Expect.Call(_scheduleDayPro0209Tue.DaySchedulePart()).Return(_schedulePartDayOff).Repeat.Any();
                Expect.Call(_scheduleDayPro0210Wed.DaySchedulePart()).Return(_schedulePartShortMainShift).Repeat.Any();
                Expect.Call(_scheduleDayPro0211Thu.DaySchedulePart()).Return(_schedulePartShortMainShift).Repeat.Any();
                Expect.Call(_scheduleDayPro0212Fri.DaySchedulePart()).Return(_schedulePartDayOff).Repeat.Any();

                Expect.Call(_matrixConverter.Convert(_scheduleMatrix, false, false)).Return(bitArray).Repeat.Any();
                Expect.Call(_dataExtractor.Values()).Return(_values).Repeat.Any();
            }

            // day counterparts are > febr 9, 10, 11, 12
            IList<DateOnly> result = _target.Execute(_scheduleMatrix, _dataExtractor);
            Assert.AreEqual(2, result.Count);

            DateOnly mostUnderStaffingDay = new DateOnly(2010, 02, 10);
            DateOnly mostOverStaffingDay = new DateOnly(2010, 02, 11);

            Assert.AreEqual(mostUnderStaffingDay, result[0]);
            Assert.AreEqual(mostOverStaffingDay, result[1]);
        }

        [Test]
        public void VerifyShorterMainShiftsDoesNotPlay()
        {

            ILockableBitArray bitArray = createBitArray();
 
            using (_mocks.Record())
            {
                matrixExpectationsWithMainShifts();

                Expect.Call(_scheduleDayPro0209Tue.DaySchedulePart()).Return(_schedulePartShortMainShift).Repeat.Any();
                Expect.Call(_scheduleDayPro0210Wed.DaySchedulePart()).Return(_schedulePartLongMainShift).Repeat.Any();
                Expect.Call(_scheduleDayPro0211Thu.DaySchedulePart()).Return(_schedulePartShortMainShift).Repeat.Any();
                Expect.Call(_scheduleDayPro0212Fri.DaySchedulePart()).Return(_schedulePartLongMainShift).Repeat.Any();

                Expect.Call(_matrixConverter.Convert(_scheduleMatrix, false, false)).Return(bitArray).Repeat.Any();
                Expect.Call(_dataExtractor.Values()).Return(_values).Repeat.Any();
            }

            // day counterparts are > febr 9, 10, 11, 12
            IList<DateOnly> result = _target.Execute(_scheduleMatrix, _dataExtractor);
            Assert.AreEqual(2, result.Count);

            DateOnly mostUnderStaffingDay = new DateOnly(2010, 02, 09);
            DateOnly mostOverStaffingDay = new DateOnly(2010, 02, 12);

            Assert.AreEqual(mostUnderStaffingDay, result[0]);
            Assert.AreEqual(mostOverStaffingDay, result[1]);
        }

        private void matrixExpectationsWithMainShifts()
        {
            IDictionary<DateOnly, IScheduleDayPro> unlockedDaysDictionary = new Dictionary<DateOnly, IScheduleDayPro>();
            IDictionary<DateOnly, IScheduleDayPro> fullWeekPeriodDictionary = new Dictionary<DateOnly, IScheduleDayPro>();

            IList<IScheduleDayPro> outerPeriodList = new List<IScheduleDayPro>
                                                    {
                                                        _scheduleDayPro0201Mon,
                                                        _scheduleDayPro0202Tue,
                                                        _scheduleDayPro0203Wed,
                                                        _scheduleDayPro0204Thu,
                                                        _scheduleDayPro0205Fri,
                                                        _scheduleDayPro0206Sat,
                                                        _scheduleDayPro0207Sun,

                                                        _scheduleDayPro0208Mon,
                                                        _scheduleDayPro0209Tue,
                                                        _scheduleDayPro0210Wed,
                                                        _scheduleDayPro0211Thu,
                                                        _scheduleDayPro0212Fri,
                                                        _scheduleDayPro0213Sat,
                                                        _scheduleDayPro0214Sun,
                                                        
                                                        _scheduleDayPro0215Mon,
                                                        _scheduleDayPro0216Tue,
                                                        _scheduleDayPro0217Wed,
                                                        _scheduleDayPro0218Thu,
                                                        _scheduleDayPro0219Fri,
                                                        _scheduleDayPro0220Sat,
                                                        _scheduleDayPro0221Sun
                                                    };




            for (int i = 0; i < 21; i++)
            {
                DateOnly currentDate = new DateOnly(2010, 02, 01).AddDays(i);
                IScheduleDayPro scheduleDay = outerPeriodList[i];
                Expect.Call(scheduleDay.Day).Return(currentDate).Repeat.Any();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(currentDate)).Return(outerPeriodList[i]).Repeat.Any();

                if (i >= 7 && i <= 13)
                {
                    fullWeekPeriodDictionary.Add(currentDate, scheduleDay);
                }

                if (i >= 8 && i <= 11)
                {
                    unlockedDaysDictionary.Add(currentDate, scheduleDay);
                }
            }
            Expect.Call(_scheduleMatrix.FullWeeksPeriodDays)
                .Return(fullWeekPeriodDictionary.Values.ToArray()).Repeat.Any();
            Expect.Call(_scheduleMatrix.UnlockedDays)
                .Return(unlockedDaysDictionary.Values.ToHashSet()).Repeat.Any();

            Expect.Call(_schedulePartDayOff.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();

            addWorkShiftExpectationsToScheduleDay(_schedulePartShortMainShift, new TimeSpan(5, 0, 0));
            addWorkShiftExpectationsToScheduleDay(_schedulePartLongMainShift, new TimeSpan(8, 0, 0));
        }

        #endregion

        private static ILockableBitArray createBitArray()
        {
            ILockableBitArray bitArray = new LockableBitArray(7, false, false, null);
            bitArray.PeriodArea = new MinMax<int>(1, 4);
            bitArray.Lock(0, true);
            bitArray.Lock(5, true);
            bitArray.Lock(6, true);
            return bitArray;
        }

        private void addWorkShiftExpectationsToScheduleDay(IScheduleDay scheduleDay, TimeSpan expectedWorkShiftLength)
        {
            IProjectionService projectionService = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

            Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            Expect.Call(scheduleDay.ProjectionService()).Return(projectionService).Repeat.Any();
            Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.Any();
            Expect.Call(visualLayerCollection.ContractTime()).Return(expectedWorkShiftLength).Repeat.Any();
        }
    }
}