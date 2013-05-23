using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture, SetUICulture("en-US")]
    public class BetweenDayOffBlockFinderTest
    {

        #region fields

        private BetweenDayOffBlockFinder _target;
        private IBlockFinder _interface;
        private IScheduleMatrixPro _matrix;
        private MockRepository _mocks;

        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDayPro _scheduleDayPro3;
        private IScheduleDayPro _scheduleDayPro4;
        private IScheduleDayPro _scheduleDayPro5;
        private IScheduleDayPro _scheduleDayPro6;
        private IScheduleDayPro _scheduleDayPro7;
        private IScheduleDayPro _scheduleDayPro8;
        private IScheduleDayPro _scheduleDayPro9;
        private IScheduleDayPro _scheduleDayPro10;
        private IScheduleDay _schedulePartEmpty;
        private IScheduleDay _schedulePartDo;
        private IScheduleDay _schedulePartAbsence;
        private IScheduleDay _schedulePartEarly;
        private IScheduleDay _schedulePartLate;
        private IShiftCategory _early;
    	private IScheduleDayPro _scheduleDayPro11;

        #endregion

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_target = new BetweenDayOffBlockFinder(_matrix, new EmptyDaysInBlockOutsideSelectedHandlerForTest());
			_interface = new BetweenDayOffBlockFinder(_matrix, new EmptyDaysInBlockOutsideSelectedHandlerForTest());
            _scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro3 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro4 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro5 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro6 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro7 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro8 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro9 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro10 = _mocks.StrictMock<IScheduleDayPro>();
            _schedulePartEmpty = _mocks.StrictMock<IScheduleDay>();
            _schedulePartDo = _mocks.StrictMock<IScheduleDay>();
            _schedulePartAbsence = _mocks.StrictMock<IScheduleDay>();
            _schedulePartEarly = _mocks.StrictMock<IScheduleDay>();
            _schedulePartLate = _mocks.StrictMock<IScheduleDay>();
			_scheduleDayPro11 = _mocks.StrictMock<IScheduleDayPro>();
        }

        [Test]
        public void VerifyFindsFirstValidBlockWhenFirstIsEmpty()
        {
            using(_mocks.Record())
            {
                MockExpectations(StandardUnlockedList(), false);
                SimplePeriod1();
            }

            IBlockFinderResult result = _interface.NextBlock();

            Assert.AreEqual(_schedulePartEarly.AssignmentHighZOrder().ShiftCategory, result.ShiftCategory);
            Assert.AreEqual(3, result.BlockDays.Count);
            Assert.AreEqual(_scheduleDayPro1.Day, result.BlockDays[0]);
            Assert.AreEqual(_scheduleDayPro4.Day, result.BlockDays[2]);
        }

        [Test]
        public void VerifyFindsFirstValidBlockWhenLastBlockIsValid()
        {
            using (_mocks.Record())
            {
                MockExpectations(StandardUnlockedList(), false);
                LastBlockValid();
            }

            IBlockFinderResult result = _interface.NextBlock();

            Assert.AreEqual(null, result.ShiftCategory);
            Assert.AreEqual(1, result.BlockDays.Count);
            Assert.AreEqual(_scheduleDayPro9.Day, result.BlockDays[0]);
        }

        [Test]
        public void VerifyFindBlockOnDate()
        {
            using (_mocks.Record())
            {
                MockExpectations(StandardUnlockedList(), false);
                SimplePeriod();
            }

            IBlockFinderResult result = _target.FindBlockForDate(_scheduleDayPro6.Day);

            Assert.AreEqual(3, result.BlockDays.Count);
            Assert.IsNull(result.ShiftCategory);
            Assert.AreEqual(_scheduleDayPro6.Day, result.BlockDays[1]);

            result = _target.FindBlockForDate(_scheduleDayPro1.Day);
            Assert.AreEqual(2, result.BlockDays.Count);
            Assert.IsNull(result.ShiftCategory);
            Assert.AreEqual(_scheduleDayPro3.Day, result.BlockDays[1]);
        }

        
        [Test]
        public void ValidBlockCannotContainFullDayAbsence()
        {
            using (_mocks.Record())
            {
                MockExpectations(StandardUnlockedList(), false);
                SimplePeriod1();
            }

            IBlockFinderResult result = _target.FindValidBlockForDate(_scheduleDayPro3.Day);
            Assert.AreEqual(3, result.BlockDays.Count);
            Assert.AreEqual(_schedulePartEarly.AssignmentHighZOrder().ShiftCategory, result.ShiftCategory);
        }

        [Test]
        public void VerifyFindValidBlockForDateReportsConflictingCategories()
        {
            using (_mocks.Record())
            {
                MockExpectations(StandardUnlockedList(), false);
                SimplePeriod();
            }

            IBlockFinderResult result = _target.FindValidBlockForDate(_scheduleDayPro5.Day);
            Assert.AreEqual(0, result.BlockDays.Count);
            Assert.IsNull(result.ShiftCategory);
            Assert.AreEqual(1, result.WorkShiftFinderResult.Count);
        }

        [Test]
        public void VerifyFindValidBlockForDateDoesNotReportConflictingCategoriesWhenNoEmptyDay()
        {
            using (_mocks.Record())
            {
                MockExpectations(StandardUnlockedList(), false);
                SimplePeriod1();
            }

            IBlockFinderResult result = _target.FindValidBlockForDate(_scheduleDayPro6.Day);
            Assert.AreEqual(0, result.BlockDays.Count);
            Assert.IsNull(result.ShiftCategory);
            Assert.AreEqual(0, result.WorkShiftFinderResult.Count);
        }

        [Test]
        public void VerifyFindValidBlockOnDate()
        {
            using (_mocks.Record())
            {
                MockExpectations(StandardUnlockedList(), false);
                SimplePeriod1();
            }

            IBlockFinderResult result = _target.FindValidBlockForDate(_scheduleDayPro6.Day);
            Assert.AreEqual(0, result.BlockDays.Count);
            Assert.IsNull(result.ShiftCategory);

            result = _target.FindValidBlockForDate(_scheduleDayPro7.Day);
            Assert.AreEqual(0, result.BlockDays.Count);
            Assert.IsNull(result.ShiftCategory);

            result = _target.FindValidBlockForDate(_scheduleDayPro2.Day);
            Assert.AreEqual(3, result.BlockDays.Count);
            Assert.AreEqual(_schedulePartEarly.AssignmentHighZOrder().ShiftCategory, result.ShiftCategory);
            Assert.AreEqual(_scheduleDayPro3.Day, result.BlockDays[1]);
        }

        [Test]
        public void VerifyNoBlockBeforeFirstMatrixDate()
        {
            using (_mocks.Record())
            {
                MockExpectations(StandardUnlockedList(), false);
                SimplePeriod();
            }

            IBlockFinderResult result = _target.FindValidBlockForDate(_scheduleDayPro1.Day.AddDays(-1));
            Assert.AreEqual(0, result.BlockDays.Count);
            Assert.IsNull(result.ShiftCategory);
        }

        [Test]
        public void VerifyQueryOnDayOffReturnsNoBlock()
        {
            using (_mocks.Record())
            {
                MockExpectations(StandardUnlockedList(), false);
                SimplePeriod();
            }

            IBlockFinderResult result = _target.FindValidBlockForDate(_scheduleDayPro4.Day);
            Assert.AreEqual(0, result.BlockDays.Count);
            Assert.IsNull(result.ShiftCategory);
        }

        [Test]
        public void VerifyNoNextBlock()
        {
            using (_mocks.Record())
            {
                MockExpectations(StandardUnlockedList(), false);
                AllScheduledPeriod();
            }

            IBlockFinderResult result = _interface.NextBlock();

            Assert.IsNull(result.ShiftCategory);
            Assert.AreEqual(0, result.BlockDays.Count);
        }

        [Test]
        public void VerifyMatrixProperty()
        {
            Assert.IsNotNull(_interface.ScheduleMatrix);
        }

        [Test]
        public void VerifyBlockWithUnscheduledDaysOutsideUnlockedIsNotValid()
        {
            using (_mocks.Record())
            {
                MockExpectations(ShortUnlockedList(), false);
                PartialEmptyBlock();
            }

            IBlockFinderResult result = _interface.NextBlock();
            Assert.IsNull(result.ShiftCategory);
        }

        [Test]
        public void VerifyNotReturningSameBlockTwiceWithoutReset()
        {
            using (_mocks.Record())
            {
                MockExpectations(StandardUnlockedList(), false);
                PartialEmptyBlock();
            }

            IBlockFinderResult result = _interface.NextBlock();
            Assert.IsNotNull(result.ShiftCategory);
            Assert.AreEqual(_scheduleDayPro1.Day, result.BlockDays[0]);

            result = _interface.NextBlock();
            Assert.IsNull(result.ShiftCategory);
            Assert.AreEqual(_scheduleDayPro7.Day, result.BlockDays[0]);

            result = _interface.NextBlock();
            Assert.IsNull(result.ShiftCategory);
            Assert.AreEqual(0, result.BlockDays.Count);

            _interface.ResetBlockPointer();

            result = _interface.NextBlock();
            Assert.IsNotNull(result.ShiftCategory);
            Assert.AreEqual(_scheduleDayPro1.Day, result.BlockDays[0]);
        }

        [Test]
        public void VerifyBug9841()
        {
            using (_mocks.Record())
            {
                MockExpectations(StandardUnlockedList(), false);
                Bug9841();
            }

            IBlockFinderResult result = _target.FindValidBlockForDate(_scheduleDayPro8.Day);
            Assert.AreEqual(0, result.BlockDays.Count);
            Assert.IsNull(result.ShiftCategory);
        }

        [Test]
        public void VerifyAbsenceBeforeWeekend()
        {
            using (_mocks.Record())
            {
                MockExpectations(StandardUnlockedList(), false);
                AbsencebeforeWeekend();
            }

            IBlockFinderResult result = _target.NextBlock();
            Assert.AreEqual(3, result.BlockDays.Count);
            Assert.IsNull(result.ShiftCategory);

            result = _target.NextBlock();
            Assert.AreEqual(2, result.BlockDays.Count);
            Assert.IsNull(result.ShiftCategory);

            result = _target.NextBlock();
            Assert.AreEqual(0, result.BlockDays.Count);
            Assert.IsNull(result.ShiftCategory);
        }

		[Test]
		public void ShouldFindEmptyBlockEndInNextPeriod()
		{
			using (_mocks.Record())
			{
				MockExpectations(StandardUnlockedList(), true);
				OutOfPeriod();
            }

            var result = _interface.NextBlock();

            Assert.AreEqual(3, result.BlockDays.Count);
			Assert.AreEqual(_scheduleDayPro8.Day, result.BlockDays[0]);
			Assert.AreEqual(_scheduleDayPro9.Day, result.BlockDays[1]);
			Assert.AreEqual(_scheduleDayPro10.Day, result.BlockDays[2]);
		}

        private IList<IScheduleDayPro> StandardUnlockedList()
        {
            IList<IScheduleDayPro> unlockedList = new List<IScheduleDayPro>
                                                    {   
                                                        _scheduleDayPro2,
                                                        _scheduleDayPro3,
                                                        _scheduleDayPro4,
                                                        _scheduleDayPro5,
                                                        _scheduleDayPro6,
                                                        _scheduleDayPro7,
                                                        _scheduleDayPro8,
                                                        _scheduleDayPro9,
                                                    };
            return unlockedList;
        }

        private IList<IScheduleDayPro> ShortUnlockedList()
        {
            IList<IScheduleDayPro> unlockedList = new List<IScheduleDayPro>
                                                    {
                                                        _scheduleDayPro3,
                                                        _scheduleDayPro4,
                                                        _scheduleDayPro5,
                                                        _scheduleDayPro6,
                                                        _scheduleDayPro7,
                                                        _scheduleDayPro8,
                                                    };
            return unlockedList;
        }

        private void MockExpectations(IList<IScheduleDayPro> unlockedList, bool lastDayOutOfPeriod)
        {
            IPerson person = PersonFactory.CreatePerson();
            var earlyShift = _mocks.StrictMock<IMainShift>();
            _early = ShiftCategoryFactory.CreateShiftCategory("XX");
            var lateShift = _mocks.StrictMock<IMainShift>();
            IShiftCategory late = ShiftCategoryFactory.CreateShiftCategory("YY");
            var earlyAssignment = _mocks.StrictMock<IPersonAssignment>();
            var lateAssignment = _mocks.StrictMock<IPersonAssignment>();
            IList<IScheduleDayPro> periodList = new List<IScheduleDayPro>
                                                    {
                                                        _scheduleDayPro1,
                                                        _scheduleDayPro2,
                                                        _scheduleDayPro3,
                                                        _scheduleDayPro4,
                                                        _scheduleDayPro5,
                                                        _scheduleDayPro6,
                                                        _scheduleDayPro7,
                                                        _scheduleDayPro8,
                                                        _scheduleDayPro9,
                                                        _scheduleDayPro10
                                                    };

            

            Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(periodList)).Repeat.Any();
            for (int i = 0; i < 10; i++)
            {
                var day = periodList[i];
                Expect.Call(day.Day).Return(new DateOnly(2010, 1, 1).AddDays(i)).Repeat.Any();

				if(i >= 9 && lastDayOutOfPeriod)
					Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly(2010, 1, 1).AddDays(i))).Return(_scheduleDayPro11).Repeat.Any();
				else
					Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly(2010, 1, 1).AddDays(i))).Return(periodList[i]).Repeat.Any();
            }
            Expect.Call(_schedulePartEmpty.SignificantPart()).Return(SchedulePartView.None).Repeat.Any();
            Expect.Call(_schedulePartDo.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
            Expect.Call(_schedulePartAbsence.SignificantPart()).Return(SchedulePartView.FullDayAbsence).Repeat.Any();
            Expect.Call(_schedulePartEarly.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
			Expect.Call(_schedulePartEmpty.IsScheduled()).Return(false).Repeat.Any();
			Expect.Call(_schedulePartDo.IsScheduled()).Return(true).Repeat.Any();
			Expect.Call(_schedulePartAbsence.IsScheduled()).Return(true).Repeat.Any();
			Expect.Call(_schedulePartEarly.IsScheduled()).Return(true).Repeat.Any();
			Expect.Call(_schedulePartLate.IsScheduled()).Return(true).Repeat.Any();
            Expect.Call(_schedulePartEarly.AssignmentHighZOrder()).Return(earlyAssignment).Repeat.Any();
            Expect.Call(earlyAssignment.ToMainShift()).Return(earlyShift).Repeat.Any();
						Expect.Call(earlyAssignment.ShiftCategory).Return(_early).Repeat.Any();
            Expect.Call(_schedulePartLate.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            Expect.Call(_schedulePartLate.AssignmentHighZOrder()).Return(lateAssignment).Repeat.Any();
            Expect.Call(lateAssignment.ToMainShift()).Return(lateShift).Repeat.Any();
						Expect.Call(lateAssignment.ShiftCategory).Return(late).Repeat.Any();
            Expect.Call(_matrix.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(unlockedList)).Repeat.Any();
            Expect.Call(_matrix.FullWeeksPeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(periodList)).Repeat.Any();
            Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly(2009, 12, 31))).Return(null).
                    Repeat.Any();
            Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly(2010, 01, 11))).Return(null).
                    Repeat.Any();
            Expect.Call(_matrix.Person).Return(person).Repeat.Any();
            
        }

        private void SimplePeriod()
        {
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartLate).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
        }

        private void SimplePeriod1()
        {
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartLate).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
        }

        private void AllScheduledPeriod()
        {
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartLate).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
        }

        private void LastBlockValid()
        {
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartLate).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
        }

		private void OutOfPeriod()
		{
			Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
			Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
			Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
			Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
			Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartLate).Repeat.Any();
			Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartLate).Repeat.Any();
			Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
			Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
			Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
			Expect.Call(_scheduleDayPro11.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
		}

        private void PartialEmptyBlock()
        {
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
        }

        private void Bug9841()
        {
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartLate).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartLate).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
        }

        private void AbsencebeforeWeekend()
        {
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
            Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
        }
    }

}