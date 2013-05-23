using System.Collections;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using System.Collections.Generic;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class ScheduleMatrixBitArrayConverterTest
    {
        #region Variables

        private ScheduleMatrixBitArrayConverter _target;

        private IScheduleMatrixPro _scheduleMatrix;
        private MockRepository _mocks;

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
        private IScheduleDayPro _scheduleDayPro0222Mon;
        private IScheduleDayPro _scheduleDayPro0223Tue;
        private IScheduleDayPro _scheduleDayPro0224Wed;
        private IScheduleDayPro _scheduleDayPro0225Thu;
        private IScheduleDayPro _scheduleDayPro0226Fri;
        private IScheduleDayPro _scheduleDayPro0227Sat;
        private IScheduleDayPro _scheduleDayPro0228Sun;

        private IScheduleDay _schedulePartEmpty;
        private IScheduleDay _schedulePartDo;
        private IScheduleDay _schedulePartAbsence;
        private IScheduleDay _schedulePartEarly;
        private IScheduleDay _schedulePartLate;
        private IShiftCategory _early;

        #endregion

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();

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
            _scheduleDayPro0222Mon = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0223Tue = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0224Wed = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0225Thu = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0226Fri = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0227Sat = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro0228Sun = _mocks.StrictMock<IScheduleDayPro>();

            _schedulePartEmpty = _mocks.StrictMock<IScheduleDay>();
            _schedulePartDo = _mocks.StrictMock<IScheduleDay>();
            _schedulePartAbsence = _mocks.StrictMock<IScheduleDay>();
            _schedulePartEarly = _mocks.StrictMock<IScheduleDay>();
            _schedulePartLate = _mocks.StrictMock<IScheduleDay>();

            using (_mocks.Record())
            {
                mockExpectations();
                simplePeriod();
            }

            _target = new ScheduleMatrixBitArrayConverter();
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyOuterWeekPeriodDayOffsBitArrayCountIsAlwaysTheOuterWeeksPeriodDaysCount()
        {
            BitArray bitArray = _target.OuterWeekPeriodDayOffsBitArray(_scheduleMatrix);
            Assert.AreEqual(_scheduleMatrix.OuterWeeksPeriodDays.Count, bitArray.Count);
        }

        [Test]
        public void VerifyOuterWeekPeriodDayOffsBitArrayShowsDayOffs()
        {
            BitArray bitArray = _target.OuterWeekPeriodDayOffsBitArray(_scheduleMatrix);
            const string expectedArray = "1100000000001100100010000011";
            string resultArray = BitArrayHelper.ToString(bitArray);
            //Trace.WriteLine(resultArray);
            Assert.AreEqual(expectedArray, resultArray);
        }

        [Test]
        public void VerifyOuterWeekPeriodLockedDaysBitArrayCountIsAlwaysTheOuterWeeksPeriodDaysCount()
        {
            BitArray bitArray = _target.OuterWeekPeriodLockedDaysBitArray(_scheduleMatrix);
            Assert.AreEqual(_scheduleMatrix.OuterWeeksPeriodDays.Count, bitArray.Count);
        }

        [Test]
        public void VerifyOuterWeekPeriodLockedDaysBitArrayShowsLockedDays()
        {
            Assert.AreEqual(4, _scheduleMatrix.UnlockedDays.Count);
            BitArray bitArray = _target.OuterWeekPeriodLockedDaysBitArray(_scheduleMatrix);
            const string expectedArray = "1111111111111110000111111111";
            string resultArray = BitArrayHelper.ToString(bitArray);
            //Trace.WriteLine(resultArray);
            Assert.AreEqual(expectedArray, resultArray);
        }

        [Test]
        public void VerifyPeriodFirstAndLastIndex()
        {
            Assert.AreEqual(13, _target.PeriodIndexRange(_scheduleMatrix).Minimum);
            Assert.AreEqual(19, _target.PeriodIndexRange(_scheduleMatrix).Maximum);
        }

        private void mockExpectations()
        {
            var earlyShift = _mocks.StrictMock<IMainShift>();
            _early = ShiftCategoryFactory.CreateShiftCategory("XX");
            var lateShift = _mocks.StrictMock<IMainShift>();
            IShiftCategory late = ShiftCategoryFactory.CreateShiftCategory("YY");
            var earlyAssignment = _mocks.StrictMock<IPersonAssignment>();
            var lateAssignment = _mocks.StrictMock<IPersonAssignment>();


            IDictionary<DateOnly, IScheduleDayPro> periodDictionary = new Dictionary<DateOnly, IScheduleDayPro>();
            IDictionary<DateOnly, IScheduleDayPro> outerPeriodDictionary = new Dictionary<DateOnly, IScheduleDayPro>();
            IDictionary<DateOnly, IScheduleDayPro> weekBeforeOuterPeriodDictionary = new Dictionary<DateOnly, IScheduleDayPro>();
            IDictionary<DateOnly, IScheduleDayPro> weekAfterOuterPeriodDictionary = new Dictionary<DateOnly, IScheduleDayPro>();
            IDictionary<DateOnly, IScheduleDayPro> fullWeekPeriodDictionary = new Dictionary<DateOnly, IScheduleDayPro>();
            IDictionary<DateOnly, IScheduleDayPro> unlockedDaysDictionary = new Dictionary<DateOnly, IScheduleDayPro>();

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
                                                        _scheduleDayPro0222Mon,
                                                        _scheduleDayPro0223Tue,
                                                        _scheduleDayPro0224Wed,
                                                        _scheduleDayPro0225Thu,
                                                        _scheduleDayPro0226Fri,
                                                        _scheduleDayPro0227Sat,
                                                        _scheduleDayPro0228Sun,
                                                    };




            for (int i = 0; i < 28; i++)
            {
                DateOnly currentDate = new DateOnly(2010, 02, 01).AddDays(i);
                IScheduleDayPro scheduleDay = outerPeriodList[i];
                Expect.Call(scheduleDay.Day).Return(currentDate).Repeat.Any();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(currentDate)).Return(outerPeriodList[i]).Repeat.Any();

                outerPeriodDictionary.Add(currentDate, scheduleDay);
                if (i >= 13 && i <= 19)
                    periodDictionary.Add(currentDate, scheduleDay);
                if (i <= 20)
                    weekBeforeOuterPeriodDictionary.Add(currentDate, scheduleDay);
                if (i >= 7)
                    weekAfterOuterPeriodDictionary.Add(currentDate, scheduleDay);
                if (i >= 7 && i <= 20)
                    fullWeekPeriodDictionary.Add(currentDate, scheduleDay);
                if (i >= 15 && i <= 18)
                    unlockedDaysDictionary.Add(currentDate, scheduleDay);

            }

            Expect.Call(_schedulePartEmpty.SignificantPart()).Return(SchedulePartView.None).Repeat.Any();
            Expect.Call(_schedulePartDo.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
            Expect.Call(_schedulePartAbsence.SignificantPart()).Return(SchedulePartView.FullDayAbsence).Repeat.Any();
            Expect.Call(_schedulePartEarly.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            Expect.Call(_schedulePartEarly.AssignmentHighZOrder()).Return(earlyAssignment).Repeat.Any();
            Expect.Call(earlyAssignment.ToMainShift()).Return(earlyShift).Repeat.Any();
						Expect.Call(earlyAssignment.ShiftCategory).Return(_early).Repeat.Any();
            Expect.Call(_schedulePartLate.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            Expect.Call(_schedulePartLate.AssignmentHighZOrder()).Return(lateAssignment).Repeat.Any();
            Expect.Call(lateAssignment.ToMainShift()).Return(lateShift).Repeat.Any();
						Expect.Call(lateAssignment.ShiftCategory).Return(late).Repeat.Any();


            Expect.Call(_scheduleMatrix.UnlockedDays)
                .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(unlockedDaysDictionary.Values))).Repeat.Any();
            Expect.Call(_scheduleMatrix.GetScheduleDayByKey(new DateOnly(2010, 01, 31))).Return(null).
                    Repeat.Any();
            Expect.Call(_scheduleMatrix.EffectivePeriodDays)
                .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(periodDictionary.Values))).Repeat.Any();
            Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary)
                .Return(fullWeekPeriodDictionary).Repeat.Any();
            Expect.Call(_scheduleMatrix.FullWeeksPeriodDays)
                .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(fullWeekPeriodDictionary.Values))).Repeat.Any();
            Expect.Call(_scheduleMatrix.OuterWeeksPeriodDictionary).Return(outerPeriodDictionary).Repeat.Any();
            Expect.Call(_scheduleMatrix.OuterWeeksPeriodDays)
                .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(outerPeriodDictionary.Values))).Repeat.Any();
            Expect.Call(_scheduleMatrix.WeekBeforeOuterPeriodDictionary).Return(weekBeforeOuterPeriodDictionary).Repeat.Any();
            Expect.Call(_scheduleMatrix.WeekBeforeOuterPeriodDays)
                .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(weekBeforeOuterPeriodDictionary.Values))).Repeat.Any();
            Expect.Call(_scheduleMatrix.WeekAfterOuterPeriodDictionary).Return(weekAfterOuterPeriodDictionary).Repeat.Any();
            Expect.Call(_scheduleMatrix.WeekAfterOuterPeriodDays)
                .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>(weekAfterOuterPeriodDictionary.Values))).Repeat.Any();

        }

        private void simplePeriod()
        {
            Expect.Call(_scheduleDayPro0201Mon.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();         //week before 
            Expect.Call(_scheduleDayPro0202Tue.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();         //week before 
            Expect.Call(_scheduleDayPro0203Wed.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //week before 
            Expect.Call(_scheduleDayPro0204Thu.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //week before 
            Expect.Call(_scheduleDayPro0205Fri.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //week before 
            Expect.Call(_scheduleDayPro0206Sat.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //week before 
            Expect.Call(_scheduleDayPro0207Sun.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //week before 

            Expect.Call(_scheduleDayPro0208Mon.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //fullweek period
            Expect.Call(_scheduleDayPro0209Tue.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //fullweek period
            Expect.Call(_scheduleDayPro0210Wed.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //fullweek period
            Expect.Call(_scheduleDayPro0211Thu.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //fullweek period
            Expect.Call(_scheduleDayPro0212Fri.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //fullweek period
            Expect.Call(_scheduleDayPro0213Sat.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();         //fullweek period
            Expect.Call(_scheduleDayPro0214Sun.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();         //period day

            Expect.Call(_scheduleDayPro0215Mon.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //period day
            Expect.Call(_scheduleDayPro0216Tue.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //period day
            Expect.Call(_scheduleDayPro0217Wed.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();         //period day
            Expect.Call(_scheduleDayPro0218Thu.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //period day
            Expect.Call(_scheduleDayPro0219Fri.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //period day
            Expect.Call(_scheduleDayPro0220Sat.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //period day
            Expect.Call(_scheduleDayPro0221Sun.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();         //fullweek period

            Expect.Call(_scheduleDayPro0222Mon.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //week after
            Expect.Call(_scheduleDayPro0223Tue.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //week after
            Expect.Call(_scheduleDayPro0224Wed.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //week after
            Expect.Call(_scheduleDayPro0225Thu.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //week after
            Expect.Call(_scheduleDayPro0226Fri.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();      //week after
            Expect.Call(_scheduleDayPro0227Sat.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();         //week after
            Expect.Call(_scheduleDayPro0228Sun.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();         //week after
        }
    }
}
