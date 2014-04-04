using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class MoveTimeDecisionMaker2Test
    {
        private MoveTimeDecisionMaker2 _target;
        private MockRepository _mocks;

        private IScheduleMatrixPro _scheduleMatrix;
        private IScheduleMatrixLockableBitArrayConverter _matrixConverter;
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
            _target = new MoveTimeDecisionMaker2();

            _scheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _matrixConverter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
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

                Expect.Call(_matrixConverter.SourceMatrix).Return(_scheduleMatrix).Repeat.Any();
                Expect.Call(_matrixConverter.Convert(false, false)).Return(bitArray).Repeat.Any();
                Expect.Call(_dataExtractor.Values()).Return(_values).Repeat.Any();
            }

            // day counterparts are > febr 9, 10, 11, 12
            IList<DateOnly> result = _target.Execute(_matrixConverter, _dataExtractor);
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
                Expect.Call(_matrixConverter.SourceMatrix).Return(_scheduleMatrix).Repeat.Any();
                Expect.Call(_matrixConverter.Convert(false, false)).Return(bitArray).Repeat.Any();
                Expect.Call(_dataExtractor.Values()).Return(values).Repeat.Any();
            }

            // day counterparts are > febr 9, 10, 11, 12
            var result = _target.Execute(_matrixConverter, _dataExtractor);
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

                Expect.Call(_matrixConverter.SourceMatrix).Return(_scheduleMatrix).Repeat.Any();
                Expect.Call(_matrixConverter.Convert(false, false)).Return(bitArray).Repeat.Any();
                Expect.Call(_dataExtractor.Values()).Return(_values).Repeat.Any();
            }

            // day counterparts are > febr 9, 10, 11, 12
            IList<DateOnly> result = _target.Execute(_matrixConverter, _dataExtractor);
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
                                                        _scheduleDayPro0221Sun,
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
                .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(fullWeekPeriodDictionary.Values))).Repeat.Any();
            Expect.Call(_scheduleMatrix.UnlockedDays)
                .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(unlockedDaysDictionary.Values))).Repeat.Any();
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

                Expect.Call(_matrixConverter.SourceMatrix).Return(_scheduleMatrix).Repeat.Any();
                Expect.Call(_matrixConverter.Convert(false, false)).Return(bitArray).Repeat.Any();
                Expect.Call(_dataExtractor.Values()).Return(_values).Repeat.Any();
            }

            // day counterparts are > febr 9, 10, 11, 12
            IList<DateOnly> result = _target.Execute(_matrixConverter, _dataExtractor);
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

                Expect.Call(_matrixConverter.SourceMatrix).Return(_scheduleMatrix).Repeat.Any();
                Expect.Call(_matrixConverter.Convert(false, false)).Return(bitArray).Repeat.Any();
                Expect.Call(_dataExtractor.Values()).Return(_values).Repeat.Any();
            }

            // day counterparts are > febr 9, 10, 11, 12
            IList<DateOnly> result = _target.Execute(_matrixConverter, _dataExtractor);
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
                                                        _scheduleDayPro0221Sun,
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
                .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(fullWeekPeriodDictionary.Values))).Repeat.Any();
            Expect.Call(_scheduleMatrix.UnlockedDays)
                .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(unlockedDaysDictionary.Values))).Repeat.Any();

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

        #region Old commented staff

        //private void simpleMatrixExpectations()
        //{
        //    IMainShift earlyShift = _mocks.StrictMock<IMainShift>();
        //    _early = ShiftCategoryFactory.CreateShiftCategory("XX");
        //    IMainShift lateShift = _mocks.StrictMock<IMainShift>();
        //    IShiftCategory late = ShiftCategoryFactory.CreateShiftCategory("YY");
        //    IPersonAssignment earlyAssignment = _mocks.StrictMock<IPersonAssignment>();
        //    IPersonAssignment lateAssignment = _mocks.StrictMock<IPersonAssignment>();


        //    IDictionary<DateOnly, IScheduleDayPro> periodDictionary = new Dictionary<DateOnly, IScheduleDayPro>();
        //    IDictionary<DateOnly, IScheduleDayPro> outerPeriodDictionary = new Dictionary<DateOnly, IScheduleDayPro>();
        //    IDictionary<DateOnly, IScheduleDayPro> weekBeforeOuterPeriodDictionary = new Dictionary<DateOnly, IScheduleDayPro>();
        //    IDictionary<DateOnly, IScheduleDayPro> weekAfterOuterPeriodDictionary = new Dictionary<DateOnly, IScheduleDayPro>();
        //    IDictionary<DateOnly, IScheduleDayPro> fullWeekPeriodDictionary = new Dictionary<DateOnly, IScheduleDayPro>();
        //    IDictionary<DateOnly, IScheduleDayPro> unlockedDaysDictionary = new Dictionary<DateOnly, IScheduleDayPro>();

        //    IList<IScheduleDayPro> outerPeriodList = new List<IScheduleDayPro>
        //                                            {
        //                                                _scheduleDayPro0201Mon,
        //                                                _scheduleDayPro0202Tue,
        //                                                _scheduleDayPro0203Wed,
        //                                                _scheduleDayPro0204Thu,
        //                                                _scheduleDayPro0205Fri,
        //                                                _scheduleDayPro0206Sat,
        //                                                _scheduleDayPro0207Sun,
        //                                                _scheduleDayPro0208Mon,
        //                                                _scheduleDayPro0209Tue,
        //                                                _scheduleDayPro0210Wed,
        //                                                _scheduleDayPro0211Thu,
        //                                                _scheduleDayPro0212Fri,
        //                                                _scheduleDayPro0213Sat,
        //                                                _scheduleDayPro0214Sun,
        //                                                _scheduleDayPro0215Mon,
        //                                                _scheduleDayPro0216Tue,
        //                                                _scheduleDayPro0217Wed,
        //                                                _scheduleDayPro0218Thu,
        //                                                _scheduleDayPro0219Fri,
        //                                                _scheduleDayPro0220Sat,
        //                                                _scheduleDayPro0221Sun,
        //                                                _scheduleDayPro0222Mon,
        //                                                _scheduleDayPro0223Tue,
        //                                                _scheduleDayPro0224Wed,
        //                                                _scheduleDayPro0225Thu,
        //                                                _scheduleDayPro0226Fri,
        //                                                _scheduleDayPro0227Sat,
        //                                                _scheduleDayPro0228Sun,
        //                                            };




        //    for (int i = 0; i < 28; i++)
        //    {
        //        DateOnly currentDate = new DateOnly(2010, 02, 01).AddDays(i);
        //        IScheduleDayPro scheduleDay = outerPeriodList[i];
        //        Expect.Call(scheduleDay.Day).Return(currentDate).Repeat.Any();
        //        Expect.Call(_scheduleMatrix.GetScheduleDayByKey(currentDate)).Return(outerPeriodList[i]).Repeat.Any();

        //        outerPeriodDictionary.Add(currentDate, scheduleDay);
        //        if (i >= 13 && i <= 19)
        //            periodDictionary.Add(currentDate, scheduleDay);
        //        if (i <= 20)
        //            weekBeforeOuterPeriodDictionary.Add(currentDate, scheduleDay);
        //        if (i >= 7)
        //            weekAfterOuterPeriodDictionary.Add(currentDate, scheduleDay);
        //        if (i >= 7 && i <= 20)
        //            fullWeekPeriodDictionary.Add(currentDate, scheduleDay);
        //        if (i >= 15 && i <= 18)
        //            unlockedDaysDictionary.Add(currentDate, scheduleDay);

        //    }

        //    Expect.Call(_schedulePartEmpty.SignificantPart()).Return(SchedulePartView.None).Repeat.Any();
        //    Expect.Call(_schedulePartDo.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
        //    Expect.Call(_schedulePartAbsence.SignificantPart()).Return(SchedulePartView.FullDayAbsence).Repeat.Any();
        //    Expect.Call(_schedulePartEarly.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
        //    Expect.Call(_schedulePartEarly.AssignmentHighZOrder()).Return(earlyAssignment).Repeat.Any();
        //    Expect.Call(earlyAssignment.MainShift).Return(earlyShift).Repeat.Any();
        //    Expect.Call(earlyShift.ShiftCategory).Return(_early).Repeat.Any();
        //    Expect.Call(_schedulePartLate.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
        //    Expect.Call(_schedulePartLate.AssignmentHighZOrder()).Return(lateAssignment).Repeat.Any();
        //    Expect.Call(lateAssignment.MainShift).Return(lateShift).Repeat.Any();
        //    Expect.Call(lateShift.ShiftCategory).Return(late).Repeat.Any();


        //    Expect.Call(_scheduleMatrix.UnlockedDays)
        //        .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(unlockedDaysDictionary.Values))).Repeat.Any();
        //    Expect.Call(_scheduleMatrix.GetScheduleDayByKey(new DateOnly(2010, 01, 31))).Return(null).
        //            Repeat.Any();
        //    Expect.Call(_scheduleMatrix.SelectedPeriodDays)
        //        .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(periodDictionary.Values))).Repeat.Any();
        //    Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary)
        //        .Return(fullWeekPeriodDictionary).Repeat.Any();
        //    Expect.Call(_scheduleMatrix.FullWeeksPeriodDays)
        //        .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(fullWeekPeriodDictionary.Values))).Repeat.Any();
        //    Expect.Call(_scheduleMatrix.OuterWeeksPeriodDictionary).Return(outerPeriodDictionary).Repeat.Any();
        //    Expect.Call(_scheduleMatrix.OuterWeeksPeriodDays)
        //        .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(outerPeriodDictionary.Values))).Repeat.Any();
        //    Expect.Call(_scheduleMatrix.WeekBeforeOuterPeriodDictionary).Return(weekBeforeOuterPeriodDictionary).Repeat.Any();
        //    Expect.Call(_scheduleMatrix.WeekBeforeOuterPeriodDays)
        //        .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(weekBeforeOuterPeriodDictionary.Values))).Repeat.Any();
        //    Expect.Call(_scheduleMatrix.WeekAfterOuterPeriodDictionary).Return(weekAfterOuterPeriodDictionary).Repeat.Any();
        //    Expect.Call(_scheduleMatrix.WeekAfterOuterPeriodDays)
        //        .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(weekAfterOuterPeriodDictionary.Values))).Repeat.Any();

        //}

        #endregion

    }
}