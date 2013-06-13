using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SchedulePeriodBlockFinderTest
    {
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
    	private IEmptyDaysInBlockOutsideSelectedHandler _emptyDaysInBlockOutsideSelectedHandler;

    	[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            //_target = new SchedulePeriodBlockFinder(_matrix);
        	_emptyDaysInBlockOutsideSelectedHandler = new EmptyDaysInBlockOutsideSelectedHandlerForTest();
			_interface = new SchedulePeriodBlockFinder(_matrix, _emptyDaysInBlockOutsideSelectedHandler);
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
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "InEffective"), Test]
        public void VerifyAllEmptyDaysInEffectivePeriodIsReturned()
        {
            using(_mocks.Record())
            {
                mockExpectations();
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            }

            IBlockFinderResult result = _interface.NextBlock();

            Assert.IsNull(result.ShiftCategory);
            Assert.AreEqual(new DateOnly(2010, 1, 2), result.BlockDays[0]);
            Assert.AreEqual(new DateOnly(2010, 1, 3), result.BlockDays[1]);
            Assert.AreEqual(new DateOnly(2010, 1, 6), result.BlockDays[2]);
            Assert.AreEqual(new DateOnly(2010, 1, 7), result.BlockDays[3]);
            Assert.AreEqual(new DateOnly(2010, 1, 9), result.BlockDays[4]);
        }

        [Test]
        public void VerifyAbsenceDaysIsNotIncluded()
        {
            using (_mocks.Record())
            {
                mockExpectations();
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
                Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartAbsence).Repeat.Any();
                Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            }

            IBlockFinderResult result = _interface.NextBlock();

            Assert.IsNull(result.ShiftCategory);
            Assert.AreEqual(new DateOnly(2010, 1, 6), result.BlockDays[0]);
            Assert.AreEqual(new DateOnly(2010, 1, 7), result.BlockDays[1]);
            Assert.AreEqual(new DateOnly(2010, 1, 9), result.BlockDays[2]);
        }

        [Test]
        public void VerifyOneOrMoreDayWithSameShiftCategory()
        {
            using (_mocks.Record())
            {
                mockExpectations();
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
                Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
                Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            }

            IBlockFinderResult result = _interface.NextBlock();

            Assert.AreEqual(_early, result.ShiftCategory);
            Assert.AreEqual(new DateOnly(2010, 1, 6), result.BlockDays[0]);
            Assert.AreEqual(new DateOnly(2010, 1, 7), result.BlockDays[1]);
            Assert.AreEqual(new DateOnly(2010, 1, 9), result.BlockDays[2]);
        }

        [Test]
        public void VerifyOneOrMoreDayWithDifferentShiftCategory()
        {
            using (_mocks.Record())
            {
                mockExpectations();
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
                Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartLate).Repeat.Any();
                Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            }

            IBlockFinderResult result = _interface.NextBlock();

            Assert.IsNull(result.ShiftCategory);
            Assert.AreEqual(0, result.BlockDays.Count);

        }

        [Test]
        public void VerifyDayWithDifferentShiftCategoryReportsCorrect()
        {
            using (_mocks.Record())
            {
                mockExpectations();
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
                Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartLate).Repeat.Any();
                Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
                Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartLate).Repeat.Any();
                Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            }

            IBlockFinderResult result = _interface.NextBlock();

            Assert.IsNull(result.ShiftCategory);
            Assert.AreEqual(0, result.BlockDays.Count);
            Assert.AreEqual(3, result.WorkShiftFinderResult.Count);

        }

        [Test]
        public void VerifyDayWithDifferentShiftCategoryReportsCorrectIfNoEmptyDay()
        {
            using (_mocks.Record())
            {
                mockExpectations();
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
                Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartLate).Repeat.Any();
                Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
                Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartLate).Repeat.Any();
                Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartLate).Repeat.Any();
                Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            }

            IBlockFinderResult result = _interface.NextBlock();

            Assert.IsNull(result.ShiftCategory);
            Assert.AreEqual(0, result.BlockDays.Count);
            Assert.AreEqual(0, result.WorkShiftFinderResult.Count);

        }

        [Test]
        public void VerifyMatrixProperty()
        {
            Assert.IsNotNull(_interface.ScheduleMatrix);
        }

        [Test]
        public void VerifyResetHasToBeCalledBeforeGettingSameBlock()
        {
            using (_mocks.Record())
            {
                mockExpectations();
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEarly).Repeat.Any();
                Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
                Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
                Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            }

            IBlockFinderResult result = _interface.NextBlock();
            Assert.IsNotNull(result.ShiftCategory);
            Assert.AreEqual(4, result.BlockDays.Count);

            result = _interface.NextBlock();
            Assert.IsNull(result.ShiftCategory);
            Assert.AreEqual(0, result.BlockDays.Count);

            _interface.ResetBlockPointer();
            result = _interface.NextBlock();
            Assert.IsNotNull(result.ShiftCategory);
            Assert.AreEqual(4, result.BlockDays.Count);

        }

        private void mockExpectations()
        {
            IPerson person = PersonFactory.CreatePerson();
            IMainShift earlyShift = _mocks.StrictMock<IMainShift>();
            _early = ShiftCategoryFactory.CreateShiftCategory("XX");
            IMainShift lateShift = _mocks.StrictMock<IMainShift>();
            IShiftCategory late = ShiftCategoryFactory.CreateShiftCategory("YY");
            IPersonAssignment earlyAssignment = _mocks.StrictMock<IPersonAssignment>();
            IPersonAssignment lateAssignment = _mocks.StrictMock<IPersonAssignment>();
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
                IScheduleDayPro day = periodList[i];
                Expect.Call(day.Day).Return(new DateOnly(2010, 1, 1).AddDays(i)).Repeat.Any();
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
			Expect.Call(earlyAssignment.ShiftCategory).Return(_early).Repeat.Any();
            Expect.Call(_schedulePartLate.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            Expect.Call(_schedulePartLate.AssignmentHighZOrder()).Return(lateAssignment).Repeat.Any();
			Expect.Call(lateAssignment.ShiftCategory).Return(late).Repeat.Any();
            Expect.Call(_matrix.Person).Return(person).Repeat.Any();
        }
    }

	internal class EmptyDaysInBlockOutsideSelectedHandlerForTest :IEmptyDaysInBlockOutsideSelectedHandler
	{
		public IList<DateOnly> CheckDates(IList<DateOnly> blockDates, IScheduleMatrixPro matrixPro)
		{
			return blockDates;
		}
	}
}